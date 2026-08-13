using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Biometric;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.BiometricVerification.Command;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.Operator.Command;
using Ontec.Core.Domain.Requests.Operator.Queries;
using Ontec.Core.Domain.Requests.User.Commands;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Interface.User
{
    public interface IUserRepository
    {
        Task<int> RegisterUser(RegisterUserCommand request);
        Task<UserDto> GetUserByEmailComapnyId(GetUserByEmailComapnyId request);
        Task<UserProfileDto> GetUserById(int id);
        Task<bool> IsUserIdExist(int id);
        Task<LoginResult> IsUserExist(GetUserByEmailQuery model);
        Task<int> IsEmailExist(string email, int companyId ,bool isSignup = false);
        Task<int> IsMobileExist(string mobile, int companyId, bool isSignup = false);
        Task<int> IsEmailInTempUserExist(string email, int companyId);
        Task<int> IsMobileInTempUserExist(string mobile, int companyId);
        Task<UserDto> GetUserByPayerReferenceNumber(string payerReferenceNumber);
        Task<int> UpdateUser(UpdateUserCommand request, int proofDocumentId,bool isVerified);
        Task<int> AddUser(UserProfileDto request,string password,int propertyusertype);
        Task<int> IsUserTemporary(string emialMobile);
        Task<int> UpdateUserPofilePic(string profileUrl, int id);
        Task<UserProfileDto> GetNewRegisterUserById(int id);

        #region Operator
        Task<DatatableModel<OperatorDto>> GetOperators(GetOperatorsQuery request);
        Task<int> UpdateOperatorUser(AddOrUpdateOperatorQuery request);
        Task<int> AddOperatorUser(AddOrUpdateOperatorQuery request);
        Task DeleteOperatorUserById(int userId);
        Task<int> IsOperatorEmailExist(string email, int companyId);
        Task<int> IsOperatorMobileExist(string mobile, int companyId);
        #endregion

        #region GetRegistrationRequest
        Task<DatatableModel<GetRegistrationRequestDto>> GetRegistrationRequests(GetRegistrationRequestQuery request);
        #endregion

        #region ApproveRejectRegistrationRequest
        Task<bool> IsPendingUserIdExist(int id);
        Task<int> GetUserStatus(string emailMobile, int companyId);
        Task<int> ApproveRejectRegistrationRequestById(ApproveRejectRegistrtionRequestQuery request);
        #endregion

        #region ResetPassword
        Task<int> UpdatePassword(string password, int id);
        #endregion

        Task<bool> IsUserByEmailMobileActive(string emialMobile);
        Task<LoginAttemptUserDto> GetLoginAttemptByEmailMobile(string emailMobile, int companyId);
        Task<int> UpdateLoginAttempt(int id, int count, bool block);
        Task DeleteUserById(int userId);
        Task<bool> IsInActiveUserExist(GetUserByEmailQuery model);
        Task<int> UpdateTemporaryUser(RegisterUserCommand request);
        #region UserSettings
        Task<int> IsUserSettingsExist(int UserId);
        Task<int> SaveUserSettings(SaveUserSettingsQuery request);
        
        #endregion

        #region ChangePassword
        Task<bool> IsOldPasswordExist(ChangePasswordQuery request);
        Task<int> ChangePassword(ChangePasswordQuery request);
        #endregion

        #region UpdateUserDocument
        Task<int> UpdateUserDocument(UpdateUserDocumentQuery request, int proofDocumentId);
        #endregion
        Task<int> SaveUserSentNotifications(int UserId, string ReceieverMail, string SenderMail, string NotificationType, int StatusId);
        Task<int> UpdateLastLoginDate(int id);
        Task<bool> IsInActiveUserEmailExist(GetUserByEmailQuery model);
        Task<int> IsUserInactive(string emailMobile, int companyId);
        Task<int> IsUserRejected(string emailMobile, int companyId);
        Task<int> IsUserPending(string emailMobile, int companyId);
        Task DeActiveUserById(int userId);
        Task<int> IsUserExist(int Userid);
        Task<int> InsertDeviceToken(int UserId, string DeviceToken);
        Task<string> GetDeviceToken(int UserId);
        Task DeleteDeviceToken(int UserId, string devicetoken);
        Task<UserDto> GetUserByMeterId(int meterId);
        Task SaveUserLogInDeatils(int userId, string accessToken);
        Task UpdateUserLogOutDeatils(int userId, string sessionKey);
        Task<string> GenerateSessionKey(string numericCharacter);
        Task<int> IsSessionKeyExist(string sessionKey);
        Task LogOutUsersSessions(int userId, string sessionKey);
        Task LogOutUsersAllSessions(int userId);
        Task LogOutUsersCurrentSession(int userId, string sessionKey);
        Task LogOutUserSessionDeivcetoken(int userId);
        Task<string> GetUserPassword(int userId);
        Task<string> GetInactiveRejPendUserPassword(string email, int companyId);
        Task<UserDto> GetPropertyUserById(int userId,int propertyId);
        Task<int> UpdateUserContacts(string contact, int userId, bool isEmail);
        Task<int> VeirfyUserById(int userId);
        Task<bool> IsUserIdNotInprocess(int id);
        Task LogOutUsersDeivcetoken(int userId);
        Task<IEnumerable<DeviceTokensDto>> GetDeviceTokens();
        Task<int> GetUserRoleByEmailMobile(string emailMobile, int companyId);
        Task<IEnumerable<int>> GetActiveUserIds(List<int> userIds);
        Task<int> IsBiometricExist(int userId, string deviceToken);
        Task<int> UpdateBiometric(int id, string key);
        Task<int> RegisterBiometric(RegisterBiometricRequest request);
        Task<UserBiometric> GetUserBiometricByDeviceIdUserId(string deviceId, int userId);
        Task<LoginResult> IsBiometricUserExist(GetUserByEmailQuery model);
        Task DeleteUserPermanentById(int userId);

        Task<int> BulkInsertUsers(IEnumerable<BulkUsers> users);
        Task<bool> IsBulkUserEmailExist(string email, int companyId);
        Task<bool> IsBulkUserMobileExist(string email, int companyId);

        Task<int> GetCountryCodeId(string mobile, int companyId);
        Task<int> UpdateBulkRegisterUser(RegisterUserCommand request, int userId);
        Task<int> GetBulkUserId(string email, string mobile, int companyId);
        Task<bool> GetUserRegisteredStatus(string emailMobile, int companyId);
        Task<int> InsertUserDocument(int userId, BulkUserDocument doc);
        Task<int> InsertUser(BulkUsers user, int docId);
        Task InsertPropertiesParallel(int userId, List<PropertyRequestDto> properties, int companyId);
        Task InsertMetersParallel(int propertyId, List<PropertyMeterRequestDto> meters, int companyId);

    }
}
