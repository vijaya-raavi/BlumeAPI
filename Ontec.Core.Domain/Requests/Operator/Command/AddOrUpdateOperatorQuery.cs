using Ontec.Core.Domain.Models.Dto.Common;
using MediatR;
namespace Ontec.Core.Domain.Requests.Operator.Command
{
    public class AddOrUpdateOperatorQuery: IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string Password { get; set; }
        public int CompanyId { get; set; }
        public string Role { get; set; }
        public int RoleId { get; set; }
        public int StatusId {  get; set; }

    }
}
