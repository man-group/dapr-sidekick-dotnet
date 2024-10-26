using Dapr.Client;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowConsoleApp.Activities
{
    public class ReserveInventoryActivity(ILoggerFactory loggerFactory, DaprClient client) : WorkflowActivity<InventoryRequest, InventoryResult>
    {
        private const string StoreName = "statestore";

        private readonly ILogger _logger = loggerFactory.CreateLogger<ReserveInventoryActivity>();

        public override async Task<InventoryResult> RunAsync(WorkflowActivityContext context, InventoryRequest req)
        {
            _logger.LogInformation(
                "Reserving inventory for order '{requestId}' of {quantity} {name}",
                req.RequestId,
                req.Quantity,
                req.ItemName);

            // Ensure that the store has items
            var item = await client.GetStateAsync<InventoryItem>(
                StoreName,
                req.ItemName.ToLowerInvariant());

            // Catch for the case where the statestore isn't setup
            if (item == null)
            {
                // Not enough items.
                return new InventoryResult(false, item);
            }

            _logger.LogInformation(
                "There are {quantity} {name} available for purchase",
                item.Quantity,
                item.Name);

            // See if there're enough items to purchase
            if (item.Quantity >= req.Quantity)
            {
                // Simulate slow processing
                await Task.Delay(TimeSpan.FromSeconds(2));

                return new InventoryResult(true, item);
            }

            // Not enough items.
            return new InventoryResult(false, item);
        }
    }
}
