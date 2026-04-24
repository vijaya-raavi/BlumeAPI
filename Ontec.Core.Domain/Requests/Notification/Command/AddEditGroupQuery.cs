using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public  class AddEditGroupQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string GroupName {  get; set; }
        public int? EstateId { get; set; }
    }
}
