using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public  class AddEstateRequestCommand :IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string Estate { get; set; }
        public int StatusId {  get; set; }
    }
}
