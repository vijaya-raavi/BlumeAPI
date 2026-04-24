using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class ApproveRejectRegistrtionRequestQuery : IRequest<AddUpdateResultDto>
    {
        public int ID { get; set; }
        public bool IsApproved { get; set; }
        public string? Comments { get; set; }
        public int StatusId { get; set; }
    }
}
