using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Command;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetRegisterOtpQueryValidator : AbstractValidator<GetRegisterOtpQuery>
    {
        public GetRegisterOtpQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetRegisterOtpQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.Email).IsValidEmailId().LengthShouldBeLessOrEqualToAsync(nameof(GetRegisterOtpQuery.Email), 100);
            RuleFor(x => x.MobileNumber).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeEqualAsync(nameof(GetRegisterOtpQuery.MobileNumber).SplitPascalCase(), 10);
            RuleFor(x => x.Password).IsValidPassword();
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int inactiveUserEmail = await _userRepository.IsUserInactive(model.Email, model.CompanyId).ConfigureAwait(false);
                int inactiveUserMobile = await _userRepository.IsUserInactive(model.MobileNumber, model.CompanyId).ConfigureAwait(false);

                if (inactiveUserEmail != 0 || inactiveUserMobile != 0)
                {
                    context.AddFailure(nameof(GetRegisterOtpQuery.Email), string.Format(CommonConstants.InActiveUserExist));
                }
                else
                {
                    int id = await _userRepository.IsEmailExist(model.Email, model.CompanyId, true).ConfigureAwait(false);
                    if (id != 0)
                    {
                        context.AddFailure(nameof(GetRegisterOtpQuery.Email), string.Format(CommonConstants.AlreadyExist, nameof(GetRegisterOtpQuery.Email)));
                    }
                    id = await _userRepository.IsMobileExist(model.MobileNumber, model.CompanyId,true).ConfigureAwait(false);
                    if (id != 0)
                    {
                        context.AddFailure(nameof(GetRegisterOtpQuery.MobileNumber), "Mobile number already registered.");
                    }
                    var userIdByEmail = await _userRepository.IsEmailInTempUserExist(model.Email, model.CompanyId).ConfigureAwait(false);
                    var userIdByMobile = await _userRepository.IsMobileInTempUserExist(model.MobileNumber, model.CompanyId).ConfigureAwait(false);
                    if (userIdByEmail == 0 && userIdByMobile > 0)
                    {
                        context.AddFailure(nameof(GetRegisterOtpQuery.MobileNumber), "Mobile already registered with other email id.");
                    }
                    else if (userIdByEmail > 0 && userIdByMobile == 0)
                    {
                        context.AddFailure(nameof(GetRegisterOtpQuery.Email), "Email already registered with other mobile number.");
                    }
                }
            });
        }
    }
}
