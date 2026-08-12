using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public class ResetPasswordValidator : AbstractValidator<ResetPassword>
    {
        public ResetPasswordValidator(IUserRepository _userRepository)
        {

            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(ResetPassword.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.EmailMobile).NotNullAndEmptyAsync().IsValidEmailMobile().NotNullAndEmptyAsync();
            RuleFor(x => x.Password).NotNullAndEmptyAsync().IsValidPassword().GreaterThanOrEqualToAsync(nameof(ResetPassword.Password), 8).LengthShouldBeLessOrEqualToAsync(nameof(ResetPassword.Password), 15);
            RuleFor(x => x.ConfirmPassword).IsValidPassword().NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(ResetPassword.ConfirmPassword).SplitPascalCase(), 8).LengthShouldBeLessOrEqualToAsync(nameof(ResetPassword.ConfirmPassword).SplitPascalCase(), 15);
            RuleFor(x => x.VerifiedKey).NotNullAndEmptyAsync();
            RuleFor(x => x.LastOtp).IsValidNumber().NotNullAndEmptyAsync();
            _ = RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                 var user = await _userRepository.IsUserPending(model.EmailMobile,model.CompanyId).ConfigureAwait(false);
                 int tempUserId = await _userRepository.IsUserTemporary(model.EmailMobile).ConfigureAwait(false);
                if (user > 0 && tempUserId ==0)
                {
                    context.AddFailure(nameof(ResetPassword.EmailMobile), "Your registration request is sent to admin for approval");
                }
                if (user > 0 && tempUserId > 0)
                {
                    context.AddFailure(nameof(ResetPassword.EmailMobile), "Kindly register your profile in the system");
                }
                else if (model.Password != model.ConfirmPassword)
                {
                    context.AddFailure(nameof(ResetPassword.ConfirmPassword), string.Format(CommonConstants.NotMatch, nameof(ResetPassword.ConfirmPassword)));
                }
                else
                {
                    var getUserByEmailMobile = new GetUserByEmailComapnyId
                    {
                        CompanyId = model.CompanyId,
                        Email = model.EmailMobile
                    };
                    var data = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
                    if (data != null && data.Password == model.Password)
                    {
                        context.AddFailure(nameof(ResetPassword.Password), string.Format(CommonConstants.NotAllowed, nameof(ResetPassword.Password)));
                    }
                    if (data == null)
                    {
                        context.AddFailure(nameof(ResetPassword.EmailMobile), string.Format(CommonConstants.NotExist, nameof(ResetPassword.EmailMobile)));
                    }
                    //if (model.IsAdmin)
                    //{
                    //    context.AddFailure(nameof(GetUserByEmailQuery.Email), string.Format(CommonConstants.Unauthorized));
                    //}
                }
            });
        }
    }
}
