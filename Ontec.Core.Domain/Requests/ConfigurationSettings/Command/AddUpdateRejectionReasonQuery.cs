using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class AddUpdateRejectionReasonQuery:IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string RejectionReason { get; set; }
        public int RejectionReasonFor {  get; set; }
        
        
    }
}
