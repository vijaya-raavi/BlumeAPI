using MediatR;
using Newtonsoft.Json;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class AddMasterNotifications : IRequest<AddUpdateResultDto>
    {
        public string MeterNumber { get; set; }

        public int NotificationType { get; set; }

        public string NoteStatus { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        
    }
}
