using Microsoft.AspNetCore.SignalR;
using Ontec.Core.Domain.Interface.Services;

namespace Ontec.Infrastructure.Services
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<OntecHub> _ontecHubContext;
        public SignalRService(IHubContext<OntecHub> ontecHubContext)
        {
                _ontecHubContext = ontecHubContext;
        }

        public async Task SendMessage(string connectionId, string message)
        {
            if (!string.IsNullOrEmpty(connectionId))
            {
                var signalRClient= _ontecHubContext.Clients.Client(connectionId);
                await signalRClient.SendAsync("ontecreceivemsg", message);
            }
        }
    }
}
