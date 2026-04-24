using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateStatusQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        public int UpdateTo {get;set;}
        public bool IsVerified { get; set; } 
        public string TermConditionsVersion { get; set; }
    }
}
