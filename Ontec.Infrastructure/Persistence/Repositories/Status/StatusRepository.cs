using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Status;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Infrastructure.Persistence.Repositories.Status
{
    public class StatusRepository : IStatusRepository
    {
        private readonly IGenericRepository _genericRepository;

        public StatusRepository(IGenericRepository genericRepository)
        {
                _genericRepository = genericRepository; 
        }
        public async Task<IEnumerable<OntecSelectListItem>> GetStatusMaster()
        {
            var sQuery = @" SELECT id, name
	                        FROM public.ohd_enum_status;";
            return await _genericRepository.GetAsync<OntecSelectListItem>(sQuery).ConfigureAwait(false);    
        }
    }
}
