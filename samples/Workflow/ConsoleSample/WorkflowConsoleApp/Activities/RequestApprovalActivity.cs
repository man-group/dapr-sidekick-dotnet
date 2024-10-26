using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowConsoleApp.Activities
{
    public class RequestApprovalActivity(ILoggerFactory loggerFactory) : WorkflowActivity<OrderPayload, object>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<RequestApprovalActivity>();

        public override Task<object> RunAsync(WorkflowActivityContext context, OrderPayload input)
        {
            var orderId = context.InstanceId;
            _logger.LogInformation("Requesting approval for order {orderId}", orderId);

            return Task.FromResult<object>(null);
        }
    }
}
