using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowConsoleApp.Activities
{
    public class NotifyActivity(ILoggerFactory loggerFactory) : WorkflowActivity<Notification, object>
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<NotifyActivity>();

        public override Task<object> RunAsync(WorkflowActivityContext context, Notification notification)
        {
            _logger.LogInformation(notification.Message);

            return Task.FromResult<object>(null);
        }
    }

    public record Notification(string Message);
}
