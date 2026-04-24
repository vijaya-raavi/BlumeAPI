using FluentValidation;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.Otp.Command
{
    public class VerifyForgotPasswordOtpQueryValidator:AbstractValidator<VerifyForgotPasswordOtpQuery>
    {
        public VerifyForgotPasswordOtpQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(VerifyForgotPasswordOtpQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.EmailMobile).NotNullAndEmptyAsync().IsValidEmailMobile();
            RuleFor(x => x.OTP).IsValidNumber().NotNullAndEmptyAsync().LengthShouldBeEqualAsync(nameof(VerifyForgotPasswordOtpQuery.OTP), 6);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var getUserByEmailMobile = new GetUserByEmailComapnyId
                {
                    CompanyId = model.CompanyId,
                    Email = model.EmailMobile
                };

                var data = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
                if (data == null)
                {
                    context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), " User does not exist in the system.");
                }
            });
        }
    }
}
