using Dapr.Client;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowConsoleApp.Activities
{
    public class UpdateInventoryActivity(ILoggerFactory loggerFactory, DaprClient client) : WorkflowActivity<PaymentRequest, object>
    {
        private const string StoreName = "statestore";
        private readonly ILogger _logger = loggerFactory.CreateLogger<UpdateInventoryActivity>();

        public override async Task<object> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Checking inventory for order '{requestId}' for {amount} {name}",
                req.RequestId,
                req.Amount,
                req.ItemName);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Determine if there are enough Items for purchase
            var item = await client.GetStateAsync<InventoryItem>(
                StoreName,
                req.ItemName.ToLowerInvariant());
            var newQuantity = item.Quantity - req.Amount;
            if (newQuantity < 0)
            {
                _logger.LogInformation(
                    "Payment for request ID '{requestId}' could not be processed. Insufficient inventory.",
                    req.RequestId);
                throw new InvalidOperationException($"Not enough '{req.ItemName}' inventory! Requested {req.Amount} but only {item.Quantity} available.");
            }

            // Update the statestore with the new amount of the item
            await client.SaveStateAsync(
                StoreName,
                req.ItemName.ToLowerInvariant(),
                new InventoryItem(Name: req.ItemName, PerItemCost: item.PerItemCost, Quantity: newQuantity));

            _logger.LogInformation(
                "There are now {quantity} {name} left in stock",
                newQuantity,
                item.Name);

            return null;
        }
    }
}
