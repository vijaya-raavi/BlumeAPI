using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.CommunicationSetting;

namespace Ontec.Core.Domain.Interface.Communication
{
    public interface ICommunicationRepository
    {
        Task<IEnumerable<CommunicationType>> GetCommunicationsByUserId(int userId);
        Task InsertUserCommunications(IEnumerable<int> communicationsIds, int userId);
        Task UpdateUserCommunications(IEnumerable<int> communicationsIds, int userId, IEnumerable<int> ExistcommIds);
        Task<IEnumerable<OntecSelectListItem>> GetCommunicationMasters();
        Task<IEnumerable<CommunicationEnumModel>> GetCommunicationEnumModel();
        Task InsertUserSettings(int communicationsId, int userId, int statusId);
        Task DeleteUserSettings(int userId);
    }
}
