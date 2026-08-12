using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Property.Command
{
    public class AddOrUpdatePropertyQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? EstateId { get; set; }
        public string UnitNumber { get; set; }
        public int OwnerId { get; set; }
        public int CompanyId { get; set; }
        public int StatusId { get; set; }
        public string AddressLine1 { get; set; }
        //public string AddressLine2 { get; set; }
        //public string City { get; set; }
        //public string State { get; set; }
        //public string Country { get; set; }
       
    }
}
