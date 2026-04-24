using MediatR;
using Ontec.Core.Domain.Models.Dto.Property;

namespace Ontec.Core.Domain.Requests.Dashboard.Queries
{
    public class GetPropertyDetailsQuery : IRequest<PropertyDetailsDto>
    {
        public int UserId { get; set; }
    }

}
