using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.PropertyUser.Command;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(RegisterUserCommand.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.EmailId).NotNullAndEmptyAsync().IsValidEmailId();
            RuleFor(x => x.MobileNumber).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeEqualAsyncMobileNumber(nameof(RegisterUserCommand.MobileNumber).SplitPascalCase(), 10);
            RuleFor(x => x.Otp).NotNullAndEmptyAsyncOTP().IsValidNumber().LengthShouldBeEqualAsync(nameof(RegisterUserCommand.Otp).ToUpper(), 6);
            RuleFor(x => x.Password).NotNullAndEmptyAsync().IsValidPassword().GreaterThanOrEqualToAsync(nameof(RegisterUserCommand.Password), 8).LengthShouldBeLessOrEqualToAsync(nameof(RegisterUserCommand.Password), 15);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int id = await _userRepository.IsEmailExist(model.EmailId, model.CompanyId,true).ConfigureAwait(false);
                if (id != 0)
                {
                    context.AddFailure(nameof(RegisterUserCommand.EmailId), string.Format(CommonConstants.AlreadyExist, nameof(RegisterUserCommand.EmailId)));

                }               
                id = await _userRepository.IsMobileExist(model.MobileNumber, model.CompanyId,true).ConfigureAwait(false);
                if (id != 0)
                {
                    //context.AddFailure(nameof(RegisterUserCommand.MobileNumber), string.Format(CommonConstants.AlreadyExist, nameof(RegisterUserCommand.MobileNumber)));
                    context.AddFailure(nameof(RegisterUserCommand.EmailId), "Mobile number already registered");
                }
                var userIdByEmail = await _userRepository.IsEmailInTempUserExist(model.EmailId, model.CompanyId).ConfigureAwait(false);
                var userIdByMobile = await _userRepository.IsMobileInTempUserExist(model.MobileNumber, model.CompanyId).ConfigureAwait(false);
                if (userIdByEmail == 0 && userIdByMobile > 0)
                {
                    context.AddFailure(nameof(RegisterUserCommand.MobileNumber), "Mobile already registered with other email id.");
                }
                else if (userIdByEmail > 0 && userIdByMobile == 0)
                {
                    context.AddFailure(nameof(RegisterUserCommand.EmailId), "Email already registered with other mobile number.");
                }
            });
        }
    }
}
