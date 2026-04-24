using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Property.Handler
{
    public class GetPropertiesByOwnerIdQueryValidator : AbstractValidator<GetPropertiesByOwnerIdQuery>
    {
        public GetPropertiesByOwnerIdQueryValidator(IUserRepository _userRepository
                                                    , ICompanyRepository _companyRepository)
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetPropertiesByOwnerIdQuery.UserId).SplitPascalCase(), 1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetPropertiesByOwnerIdQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertiesByOwnerIdQuery.CompanyId),"Company id does not exist");

                isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertiesByOwnerIdQuery.UserId), "User id does not exist");


            });
        }
    }
}
