using System.Text;
using System.Threading.Tasks;
using Common;
using Core.Settings;
using LkeServices.Http;

namespace LkeServices.Notifications
{
    public interface ISrvSlackNotifications
    {
        Task SendNotification(string type, string message, string sender = null);
    }

    public class SrvSlackNotifications : ISrvSlackNotifications
    {
        private readonly SlackIntegrationSettings _slackIntegrationSettings;
        public SrvSlackNotifications(SlackIntegrationSettings slackIntegrationSettings)
        {
            _slackIntegrationSettings = slackIntegrationSettings;
        }

        public async Task SendNotification(string type, string message, string sender = null)
        {
            var webHookUrl = _slackIntegrationSettings.GetChannelWebHook(type);
            if (webHookUrl != null)
            {
                var text = new StringBuilder();

                if (!string.IsNullOrEmpty(_slackIntegrationSettings.Env))
                    text.AppendLine($"Environment: {_slackIntegrationSettings.Env}");

                text.AppendLine(sender != null ? $"{sender} : {message}" : message);

                await
                    new HttpRequestClient().PostRequest(new { text = text.ToString() }.ToJson(),
                        webHookUrl);
            }
        }
    }
}
