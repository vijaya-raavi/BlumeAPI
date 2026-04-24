using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public  class AddOrUpdateNotificationsQuery: IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int NotificationType { get; set; }
        public int IsRead { get; set; }
        public string? NoteStatus {  get; set; }
        public string? MeterNumber {  get; set; }
   
    }
}
