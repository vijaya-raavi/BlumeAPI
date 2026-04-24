using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Interface.Status
{
    public interface IStatusRepository
    {
        Task<IEnumerable<OntecSelectListItem>> GetStatusMaster();
    }
}
