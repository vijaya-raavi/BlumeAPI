using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class AddUpdatePropertyUser:IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int TitleId { get; set; }
        public int CurrentUserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int PropertyId { get; set; }
        public int PropertyUserTypeId { get; set; }
        public int? StatusId { get; set; }
        public int CompanyId { get; set; }

        public string TaxNumber {  get; set; }
    }
}
