using System.Text.RegularExpressions;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.User;

namespace Ontec.Core.Application.Common.Helper
{
    public interface IUserBlockService
    {
        Task<string> ValidateUser(string email, int companyId, int userStatus, string password);
        Task<string> BlockUser(LoginAttemptUserDto user,int companyId);
    }

    public class Userblock : IUserBlockService
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IUserRepository _userRepository;
        IEncryptionandDecryption _encryptionandDecryption;
        private IOtpService _otpService;
        ICommunicationRepository _commRepository;
        public Userblock(IGenericRepository genericRepository, IUserRepository userRepository
                                        , IEncryptionandDecryption encryptionandDecryption
                                        , IOtpService otpService
                                        , ICommunicationRepository commRepository)
        {
            _genericRepository = genericRepository;
            _userRepository = userRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _otpService = otpService;
            _commRepository= commRepository;
        }
        public async Task<string> ValidateUser(string email, int companyId, int userStatus, string password)
        {
            var user = await _userRepository.GetLoginAttemptByEmailMobile(email, companyId).ConfigureAwait(false);
            var isUserExist = await _userRepository.IsEmailExist(email, companyId).ConfigureAwait(false);
            string regexPwd = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~])[A-Za-z\d!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~]{8,15}$";
            string errMsg = "";
            string userStatusBasedErr = "";
            int remainingTime = 0;
            if (Regex.IsMatch(password, regexPwd) && userStatus == (int)StatusEnum.Inactive)
            {
                userStatusBasedErr = CommonConstants.InActiveUserExist;
            }

            else if (Regex.IsMatch(password, regexPwd) && userStatus == (int)StatusEnum.Rejected)
            {
                userStatusBasedErr = CommonConstants.InRejectedUserExist;
            }
            else if (Regex.IsMatch(password, regexPwd) && userStatus == (int)StatusEnum.Pending )
            {
                userStatusBasedErr = "Your registration request is sent to admin for approval.";
            }
            else if (isUserExist != 0 && userStatus == (int)StatusEnum.Active)
            {
                var encryptedPassword = _encryptionandDecryption.Encrypt(password); 

                if (user != null && !user.IsBlocked && encryptedPassword != user.Password)
                {
                    errMsg = await BlockUser(user,companyId).ConfigureAwait(false);
                }
                else if (user != null && user.IsBlocked && encryptedPassword != user.Password)
                {
                    errMsg = await BlockUser(user,companyId).ConfigureAwait(false);
                }
                else if (user != null && user.FailedCountAttempted == 5 && user.MinutesFromLastFailledattempts < 21 && encryptedPassword == user.Password)
                {
                    remainingTime = 20 - user.MinutesFromLastFailledattempts;
                    if(remainingTime > 0)
                    {
                        errMsg = "Your account is temporarily locked due to invalid login attempts. Please try again in " + remainingTime + " minutes.";
                    }
                    if (remainingTime == 0)
                    {
                        errMsg = "Allow Login";
                    }
                }
                else
                {
                    errMsg = "Allow Login";
                }
            }
            if (userStatusBasedErr != "")
                errMsg = userStatusBasedErr;
            return errMsg;
        }

        public async Task<string> BlockUser(LoginAttemptUserDto user ,int companyId)
        {
            int remainingAttempt = 0;
            string blockMsg = "";
            int remainingTime = 0;

            if (user.FailedCountAttempted == 0)
            {
                user.FailedCountAttempted += 1;
                remainingAttempt = 5 - user.FailedCountAttempted;
                blockMsg = "You have " + remainingAttempt + " remaining attempts, after which the account will be locked for 20 minutes.";
                await _userRepository.UpdateLoginAttempt(user.Id, user.FailedCountAttempted, false).ConfigureAwait(false);
            }
            else if (user.FailedCountAttempted < 4)
            {
                user.FailedCountAttempted += 1;
                remainingAttempt = 5 - user.FailedCountAttempted;
                blockMsg = "You have " + remainingAttempt + " remaining attempts, after which the account will be locked for 20 minutes.";
                await _userRepository.UpdateLoginAttempt(user.Id, user.FailedCountAttempted, false).ConfigureAwait(false);
            }
            else if (user.FailedCountAttempted == 4)
            {
                user.FailedCountAttempted += 1;
                remainingAttempt = 5 - user.FailedCountAttempted;
                await _userRepository.UpdateLoginAttempt(user.Id, user.FailedCountAttempted, true).ConfigureAwait(false);

                if (remainingAttempt > 0)
                {
                    blockMsg = "You have " + remainingAttempt + " remaining attempts, after which the account will be locked for 20 minutes.";
                }
                else
                {
                    blockMsg = "Your account is temporarily locked for 20 minutes.";
                }
                await _userRepository.UpdateLastLoginDate(user.Id).ConfigureAwait(false);
                var commId = await _commRepository.GetCommunicationsByUserId(user.Id);
                EmailModelClass obj = new()
                {

                    title = "Failed login attempt",
                    email = user.Email,
                    forEvent = "FailedLoginAttempt",
                    subtitle = "",
                    mobile = user.Mobile,
                    propertyUser = user.FirstName,
                    body = "",
                    documentPath = "",
                    companyId = companyId
                };
                if (commId.Any(c => c.Id == (int)CommunicationTypeEnum.Email))
                {
                    await _otpService.SendEventMail(obj).ConfigureAwait(false);
                }
                
            }
            else if (user.FailedCountAttempted.Equals(5))
            {
                remainingTime = 20 - user.MinutesFromLastFailledattempts;
                if (remainingTime > 0)
                {
                    blockMsg = "Your account is temporarily locked due to invalid login attempts. Please try again in " + remainingTime + " minutes.";
                }
                if (remainingTime == 0)
                {
                    blockMsg = "Allow Login";
                }
            }
            return blockMsg;
        }

    }
}
