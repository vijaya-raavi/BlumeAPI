using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.PropertyUser;
using Ontec.Core.Domain.Requests.PropertyUser.Command;

namespace Ontec.Core.Domain.Interface.PropertyUser
{
    public interface IPropertyUserRepository
    {
        Task<EditPropertyUserMasters> GetEditPropertyUserMasters(int userId);
        Task<bool> IsPropertyUserIdExist(int propertyUserId);
        Task<AddEditPropertyUser> GetPropertyUserById(int propertyUserId);
        Task DeletePropertyUserById(int propertyUserId);

        Task<bool> IsPropertyUserExist(int propertyId, int UserId);
        Task<int> AddPropertyUser(AddUpdatePropertyUser propertyUser, int propertyUserId, int serialNumber);
        Task<int> UpdatePropertyUser(AddUpdatePropertyUser propertyUser, int propertyUserId);
        Task<PropertyUserDto> GetPropertyUserLists(int propertyId);
        Task<PropertyOwnerDto> GetPropertyOwnerIdByPropertyUserId(int propertyUserId);

        #region DeletePropertyAssociateUserById
        Task DeletePropertyAssociateUserById(int propertyId);
        Task<bool> IsPropertyIdExist(int propertyUserId);
        Task<bool> IsTenantValid(int propertyId, int currentUser);
        Task<IEnumerable<int>> GetPropertyUsersByPropertyId(int propertyId);
        #endregion

        #region AssociateUserSettings
        Task<int> IsAssociateUserSettingsExist(int propertyAssociateUserId);
        Task<int> AddAssociateUserSettings(AddUpdateAssociateUserSettingsQuery request);
        Task<int> UpdateAssociateUserSettings(AddUpdateAssociateUserSettingsQuery request);

        #endregion
        Task<GetPropertyOwnerDeatilsDto> GetPropertyOwnerDetails(int propertyId, int UserId);
        Task<int> IsPropertyUserInActive(int propertyId, int userId);

        Task<GetPropertyOwnerDeatilsDto> GetPropertyTenantDetails(int propertyId, int UserId);
        Task DeleteAssociateUserSetting(int propertyUserId);
        Task<AddEditPropertyUser> GetPropertyUserByPropertyId(int propertyId);
        Task<int> GetSerialNumber(int propertyId);
        Task<int> GetSerialNumberByPropertyUserId(int propertyId, int userId);
        Task<int>  getUserIdBySerialNumberPropertyId(int serialNo, int propertyId);
    }
}
