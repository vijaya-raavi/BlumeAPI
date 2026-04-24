using MediatR;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.User;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class GetRegistrationRequestQuery: BaseDatatableQuery<int?>, IRequest<DatatableModel<GetRegistrationRequestDto>>
    {
        public string? SearchText { get; set; }
    }
}
