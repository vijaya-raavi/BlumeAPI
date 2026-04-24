using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Content.Command
{
    public class AddContentQuery: IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public string Version {  get; set; }
        public string Content {  get; set; }
    }
}
