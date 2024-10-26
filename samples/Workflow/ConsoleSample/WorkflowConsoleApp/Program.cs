using Dapr.Client;
using Dapr.Workflow;
using Man.Dapr.Sidekick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WorkflowConsoleApp;
using WorkflowConsoleApp.Activities;
using WorkflowConsoleApp.Workflows;

const string StoreName = "statestore";

// The workflow host is a background service that connects to the sidecar over gRPC
var builder = Host.CreateDefaultBuilder(args).ConfigureServices((context, services) =>
{
    // Add Serilog
    services.AddSerilog(x => x
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

    // Add Dapr Sidekick
    services.AddDaprSidekick(context.Configuration);

    // Add workflows
    services.AddDaprWorkflow(options =>
    {
        // Note that it's also possible to register a lambda function as the workflow
        // or activity implementation instead of a class.
        options.RegisterWorkflow<OrderProcessingWorkflow>();

        // These are the activities that get invoked by the workflow(s).
        options.RegisterActivity<NotifyActivity>();
        options.RegisterActivity<ReserveInventoryActivity>();
        options.RegisterActivity<RequestApprovalActivity>();
        options.RegisterActivity<ProcessPaymentActivity>();
        options.RegisterActivity<UpdateInventoryActivity>();
    });
});

// Dapr uses a random port for gRPC by default. If we don't know what that port
// is (because this app was started separate from dapr), then assume 4001.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")))
{
    Environment.SetEnvironmentVariable("DAPR_GRPC_PORT", "4001");
}

// Start the app - this is the point where we connect to the Dapr sidecar to
// listen for workflow work-items to execute.
using var host = builder.Build();
host.Start();

// Wait for Sidekick to finish initializing the sidecar
var sidecarHost = host.Services.GetRequiredService<IDaprSidecarHost>();
while (!await sidecarHost.CheckHealthAsync())
{
    Thread.Sleep(TimeSpan.FromSeconds(5));
}

DaprClient daprClient;
var apiToken = Environment.GetEnvironmentVariable("DAPR_API_TOKEN");
if (!string.IsNullOrEmpty(apiToken))
{
    daprClient = new DaprClientBuilder().UseDaprApiToken(apiToken).Build();
}
else
{
    daprClient = new DaprClientBuilder().Build();
}

var baseInventory = new List<InventoryItem>
{
    new(Name: "Paperclips", PerItemCost: 5, Quantity: 100),
    new(Name: "Cars", PerItemCost: 15000, Quantity: 100),
    new(Name: "Computers", PerItemCost: 500, Quantity: 100),
};

// Populate the store with items
await RestockInventory(daprClient, baseInventory);

// Start the input loop
using (daprClient)
{
    var quit = false;
    Console.CancelKeyPress += (sender, e) =>
    {
        quit = true;
        Console.WriteLine("Shutting down the example.");
    };

    while (!quit)
    {
        // Get the name of the item to order and make sure we have inventory
        var items = string.Join(", ", baseInventory.Select(i => i.Name));
        Console.WriteLine($"Enter the name of one of the following items to order [{items}].");
        Console.WriteLine("To restock items, type 'restock'.");
        var itemName = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(itemName))
        {
            continue;
        }
        else if (string.Equals("restock", itemName, StringComparison.OrdinalIgnoreCase))
        {
            await RestockInventory(daprClient, baseInventory);
            continue;
        }

        InventoryItem item = baseInventory.Find(item => string.Equals(item.Name, itemName, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"We don't have {itemName}!");
            Console.ResetColor();
            continue;
        }

        Console.WriteLine($"How many {itemName} would you like to purchase?");
        var amountStr = Console.ReadLine().Trim();
        if (!int.TryParse(amountStr, out var amount) || amount <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Invalid input. Assuming you meant to type '1'.");
            Console.ResetColor();
            amount = 1;
        }

        var daprWorkflowClient = host.Services.GetRequiredService<DaprWorkflowClient>();

        // Construct the order with a unique order ID
        var orderId = $"{itemName.ToLowerInvariant()}-{Guid.NewGuid().ToString()[..8]}";
        var totalCost = amount * item.PerItemCost;
        var orderInfo = new OrderPayload(itemName.ToLowerInvariant(), totalCost, amount);

        // Start the workflow using the order ID as the workflow ID
        Console.WriteLine($"Starting order workflow '{orderId}' purchasing {amount} {itemName}");
        await daprWorkflowClient.ScheduleNewWorkflowAsync(
            name: nameof(OrderProcessingWorkflow),
            instanceId: orderId,
            input: orderInfo);

        // Wait for the workflow to start and confirm the input
        var state = await daprWorkflowClient.WaitForWorkflowStartAsync(
            instanceId: orderId);

        Console.WriteLine($"{nameof(OrderProcessingWorkflow)} (ID = {orderId}) started successfully with {state.ReadInputAs<OrderPayload>()}");

        // Wait for the workflow to complete
        while (true)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                state = await daprWorkflowClient.WaitForWorkflowCompletionAsync(
                    instanceId: orderId,
                    cancellation: cts.Token);
                break;
            }
            catch (OperationCanceledException)
            {
                // Check to see if the workflow is blocked waiting for an approval
                state = await daprWorkflowClient.GetWorkflowStateAsync(
                    instanceId: orderId);

                if (state.ReadCustomStatusAs<string>()?.Contains("Waiting for approval") == true)
                {
                    Console.WriteLine($"{nameof(OrderProcessingWorkflow)} (ID = {orderId}) requires approval. Approve? [Y/N]");
                    var approval = Console.ReadLine();
                    var approvalResult = ApprovalResult.Unspecified;
                    if (string.Equals(approval, "Y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Approving order...");
                        approvalResult = ApprovalResult.Approved;
                    }
                    else if (string.Equals(approval, "N", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Rejecting order...");
                        approvalResult = ApprovalResult.Rejected;
                    }

                    if (approvalResult != ApprovalResult.Unspecified)
                    {
                        // Raise the workflow event to the workflow
                        await daprWorkflowClient.RaiseEventAsync(
                            instanceId: orderId,
                            eventName: "ManagerApproval",
                            eventPayload: approvalResult);
                    }

                    // otherwise, keep waiting
                }
            }
        }

        if (state.RuntimeStatus == WorkflowRuntimeStatus.Completed)
        {
            var result = state.ReadOutputAs<OrderResult>();
            if (result.Processed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Order workflow is {state.RuntimeStatus} and the order was processed successfully ({result}).");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Order workflow is {state.RuntimeStatus} but the order was not processed.");
            }
        }
        else if (state.RuntimeStatus == WorkflowRuntimeStatus.Failed)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The workflow failed - {state.FailureDetails}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }
}

static async Task RestockInventory(DaprClient daprClient, List<InventoryItem> inventory)
{
    Console.WriteLine("*** Restocking inventory...");
    foreach (var item in inventory)
    {
        Console.WriteLine($"*** \t{item.Name}: {item.Quantity}");
        await daprClient.SaveStateAsync(StoreName, item.Name.ToLowerInvariant(), item);
    }
}
