using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class AuditEventSink : IEventSink
    {

        private readonly ILogger Logger;

        public AuditEventSink(ILogger<AuditEventSink> logger)
        {
            Logger = logger;
        }

        public Task PersistAsync(Event evt)
        {
            var loginSuccessEvent = evt as UserLoginSuccessEvent;
            var logoutSuccessEvent = evt as UserLogoutSuccessEvent;
            var loginFailureEvent = evt as UserLoginFailureEvent;

            if (loginSuccessEvent != null)
            {
                Logger.LogInformation(
                    $"{loginSuccessEvent.Name}:{loginSuccessEvent.Message}: Name:{loginSuccessEvent.DisplayName}; Username:{loginSuccessEvent.Username}; IPAddress:{loginSuccessEvent.RemoteIpAddress};");
            }
            else if (loginFailureEvent != null)
            {
                Logger.LogWarning(
                    $"{loginFailureEvent.Name}:{loginFailureEvent.Message}: Username:{loginFailureEvent.Username}; IPAddress:{loginFailureEvent.RemoteIpAddress};");
            }
            else if (logoutSuccessEvent != null)
            {
                Logger.LogInformation(
                    $"{logoutSuccessEvent.Name}:{logoutSuccessEvent.Message}: Username:{logoutSuccessEvent.DisplayName}; IPAddress:{logoutSuccessEvent.RemoteIpAddress};");
            }
            else
            {
                switch (evt.EventType)
                {
                    case EventTypes.Error:
                        break;
                    case EventTypes.Failure:
                        break;
                    case EventTypes.Information:
                        break;
                    case EventTypes.Success:
                        break;
                }
            }

            return Task.CompletedTask;
        }

    }
}
