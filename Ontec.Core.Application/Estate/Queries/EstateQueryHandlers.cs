using MediatR;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Models.Dto.Estate;
using Ontec.Core.Domain.Requests.Estate.Queries;

namespace Ontec.Core.Application.Estate.Queries
{
    public class EstateQueryHandlers : IRequestHandler<GetEstatesRequestQuery, IEnumerable<EstateDto>>
    {
        private readonly IEstateRepository _estateRepository;
        public EstateQueryHandlers(IEstateRepository estateRepository)
        {
            _estateRepository = estateRepository;

        }
        public async Task<IEnumerable<EstateDto>> Handle(GetEstatesRequestQuery request, CancellationToken cancellationToken)
        {
            return await _estateRepository.GetEstateList().ConfigureAwait(false);

        }
    }
}
