using MediatR;
using Ontec.Core.Domain.Models.Dto.Property;

namespace Ontec.Core.Domain.Requests.Property.Command
{
    public class ActivePropertyCommandRequest : IRequest<PropertyActiveDto>
    {
        public int Id { get; set; }
        public bool CheckValidation { get; set; }
    }
}
