using MediatR;
using Ontec.Core.Domain.Models.Dto.Content;

namespace Ontec.Core.Domain.Requests.Content.Queries
{
    public class GetContentQuery : IRequest<ContentMaster>
    {
        public string ContentName { get; set; }

    }
}
