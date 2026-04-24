using MediatR;
using Ontec.Core.Domain.Models.Dto.Configuration;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Queries
{
    public class GetRejectionReasonQuery:IRequest<IEnumerable<RejectionReasonDto>>
    {
        public int RejectionReasonFor {  get; set; }
        public int Id {  get; set; }
    }
}
