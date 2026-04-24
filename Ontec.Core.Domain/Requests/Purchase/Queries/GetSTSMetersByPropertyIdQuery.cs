using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Purchase.Queries
{
    public class GetSTSMetersByPropertyIdQuery:IRequest <IEnumerable<OntecSelectListItem>>
    {
        public int OwnerId {  get; set; }
        public int CompanyId {  get; set; }
    }
}
