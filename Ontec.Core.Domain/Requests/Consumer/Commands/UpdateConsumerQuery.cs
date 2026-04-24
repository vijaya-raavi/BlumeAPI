using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class UpdateConsumerQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public int RoleId { get; set; }
        public int CompanyId { get; set; }
        public string Address {  get; set; }
        public bool Status { get; set; }
    }
}
