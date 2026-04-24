using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using FluentValidation;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.User;



namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
    {
        public GetUserByEmailQueryValidator(IUserRepository _userRepository,
                                    IEncryptionandDecryption _encryptionandDecryption,
                               IOtpService otpService,
                               ICommunicationRepository _communicationRepository,
                                IUserBlockService _userBlockService)
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetUserByEmailQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.Email).IsValidEmailMobile().LengthShouldBeLessOrEqualToAsync("Email", 100);
            RuleFor(x => x.Password).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(GetUserByEmailQuery.Password), 15);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int userStatus = await _userRepository.GetUserStatus(model.Email.ToLower(), model.CompanyId).ConfigureAwait(false);
               
                string regexPwd = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~])[A-Za-z\d!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~]{8,15}$";
                var isUserEmailExist = await _userRepository.IsInActiveUserEmailExist(model).ConfigureAwait(false);
                string failedAttemptMessage = await _userBlockService.ValidateUser(model.Email.ToLower(), model.CompanyId, userStatus, model.Password).ConfigureAwait(false);

                if (!Regex.IsMatch(model.Password, regexPwd)&& failedAttemptMessage.Equals(""))
                {
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                }
                else if (!Regex.IsMatch(model.Password, regexPwd) && failedAttemptMessage.Equals(""))
                {
                    var user = await _userRepository.GetLoginAttemptByEmailMobile(model.Email.ToLower(), model.CompanyId).ConfigureAwait(false);
                   if(user.IsBlocked)
                    context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                }
                else if (!Regex.IsMatch(model.Password, regexPwd) && !failedAttemptMessage.Equals(""))
                {
                    var user = await _userRepository.GetLoginAttemptByEmailMobile(model.Email.ToLower(), model.CompanyId).ConfigureAwait(false);
                    if (user.IsBlocked && !failedAttemptMessage.Equals(""))
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), failedAttemptMessage);
                    }
                    else
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), failedAttemptMessage);
                    }
                }
                else if (!Regex.IsMatch(model.Password, regexPwd) && !failedAttemptMessage.Equals("") && !failedAttemptMessage.Equals("Allow Login"))
                {
                    context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                    context.AddFailure(nameof(GetUserByEmailQuery.Password), failedAttemptMessage);
                }
                else if (Regex.IsMatch(model.Password, regexPwd) && !failedAttemptMessage.Equals("") && !failedAttemptMessage.Equals("Allow Login"))
                {
                    var user = await _userRepository.GetLoginAttemptByEmailMobile(model.Email.ToLower(), model.CompanyId).ConfigureAwait(false);
                    var encryptedPassword = _encryptionandDecryption.Encrypt(model.Password);
                    if (user !=null && !user.IsBlocked && !user.Password.Equals(encryptedPassword))
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), failedAttemptMessage);
                    }
                    else if (user != null && user.IsBlocked && !user.Password.Equals(encryptedPassword))
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                    }
                    else if (user != null && user.IsBlocked && user.Password.Equals(encryptedPassword))
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Email), failedAttemptMessage);
                    }

                    else if (userStatus.Equals((int)StatusEnum.Inactive) || userStatus.Equals((int)StatusEnum.Pending) || userStatus.Equals((int)StatusEnum.Rejected))
                    {
                        string password = await _userRepository.GetInactiveRejPendUserPassword(model.Email.ToLower(), model.CompanyId).ConfigureAwait(false);
                        if (password != encryptedPassword)
                        {
                            context.AddFailure(nameof(GetUserByEmailQuery.Password), "Incorrect password entered, please verify.");
                            
                        }
                        else
                        {
                            context.AddFailure(nameof(GetUserByEmailQuery.Email), failedAttemptMessage);
                        }

                    }
                    
                }
                else if (!isUserEmailExist)
                {
                    context.AddFailure(nameof(GetUserByEmailQuery.Email), "Invalid email, Please verify!");
                }
                else if (userStatus == (int)StatusEnum.Active)
                {
                    var user = await _userRepository.GetLoginAttemptByEmailMobile(model.Email, model.CompanyId).ConfigureAwait(false);
                    if (user != null && string.IsNullOrEmpty(model.Email) && !user.IsBlocked)
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Email), string.Format(CommonConstants.InValid));
                        if (model.IsAdmin)
                        {
                            context.AddFailure(nameof(GetUserByEmailQuery.Email), string.Format(CommonConstants.Unauthorized));
                        }
                    }
                    else if (user != null && user.IsBlocked && failedAttemptMessage.Equals("Allow Login"))
                    {
                        user.IsBlocked = false;
                        user.FailedCountAttempted = 0;
                        await _userRepository.UpdateLoginAttempt(user.Id, user.FailedCountAttempted, false).ConfigureAwait(false);
                    }
                    else if (user != null && !user.IsBlocked && user.FailedCountAttempted == 5 && user.MinutesFromLastFailledattempts < 21)
                    {
                        context.AddFailure(nameof(GetUserByEmailQuery.Email), failedAttemptMessage);
                    }
                }
                //else
                //{
                //    var isUserEmailExist = await _userRepository.IsEmailExist(model.Email,model.CompanyId).ConfigureAwait(false);
                //    if (isUserEmailExist == 0)
                //    {
                //        context.AddFailure(nameof(GetUserByEmailQuery.Email), "Invalid email, Please verify!");
                //    }
                    
                //}

            });
        }
    }
}