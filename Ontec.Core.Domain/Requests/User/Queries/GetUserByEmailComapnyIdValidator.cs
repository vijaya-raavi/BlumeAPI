using FluentValidation;
using Ontec.Core.Domain.Requests.Login.Queries;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class GetUserByEmailComapnyIdValidator : AbstractValidator<GetUserByEmailComapnyId>
    {
        public GetUserByEmailComapnyIdValidator()
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetRegisterOtpQuery.CompanyId), 1);
            RuleFor(x => x.Email).IsValidEmailMobile().LengthShouldBeLessOrEqualToAsync(nameof(GetRegisterOtpQuery.Email), 100);
        }
    }
}
