using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowConsoleApp.Activities
{
    public class ProcessPaymentActivity(ILoggerFactory loggerFactory) : WorkflowActivity<PaymentRequest, object>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ProcessPaymentActivity>();

        public override async Task<object> RunAsync(WorkflowActivityContext context, PaymentRequest req)
        {
            _logger.LogInformation(
                "Processing payment: {requestId} for {amount} {item} at ${currency}",
                req.RequestId,
                req.Amount,
                req.ItemName,
                req.Currency);

            // Simulate slow processing
            await Task.Delay(TimeSpan.FromSeconds(7));

            _logger.LogInformation(
                "Payment for request ID '{requestId}' processed successfully",
                req.RequestId);

            return null;
        }
    }
}
