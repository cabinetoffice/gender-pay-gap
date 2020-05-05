using System.Threading.Tasks;
using GenderPayGap.Core.Classes.Logger;
using IdentityServer4.Events;
using IdentityServer4.Services;

namespace GenderPayGap.IdentityServer4.Classes
{
    public class AuditEventSink : IEventSink
    {

        public Task PersistAsync(Event evt)
        {
            var loginSuccessEvent = evt as UserLoginSuccessEvent;
            var logoutSuccessEvent = evt as UserLogoutSuccessEvent;
            var loginFailureEvent = evt as UserLoginFailureEvent;

            if (loginSuccessEvent != null)
            {
                CustomLogger.Information(
                    $"{loginSuccessEvent.Name}:{loginSuccessEvent.Message}: Name:{loginSuccessEvent.DisplayName}; Username:{loginSuccessEvent.Username}; IPAddress:{loginSuccessEvent.RemoteIpAddress};");
            }
            else if (loginFailureEvent != null)
            {
                CustomLogger.Warning(
                    $"{loginFailureEvent.Name}:{loginFailureEvent.Message}: Username:{loginFailureEvent.Username}; IPAddress:{loginFailureEvent.RemoteIpAddress};");
            }
            else if (logoutSuccessEvent != null)
            {
                CustomLogger.Information(
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
