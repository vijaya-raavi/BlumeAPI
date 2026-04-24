using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Company;

namespace Ontec.Core.Domain.Requests.Company.Command
{
    public class AddUpdateCompanyQueryValidator : AbstractValidator<AddUpdateCompanyQuery>
    {
        public AddUpdateCompanyQueryValidator(ICompanyRepository companyRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualToAsync(nameof(AddUpdateCompanyQuery.Id), 1);
            RuleFor(m => m.FirstName).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.FirstName).SplitPascalCase(), 10);
            RuleFor(m => m.LastName).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.LastName).SplitPascalCase(), 10);
            RuleFor(m => m.Address).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.Address), 100);
            RuleFor(m => m.EmailId).IsValidEmailId().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.EmailId).SplitPascalCase(), 200);
            RuleFor(x => x.MobileNumber).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.MobileNumber).SplitPascalCase(), 10);
            RuleFor(x => x.City).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.City), 10);
            RuleFor(x => x.Zip).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.Zip), 8);
            RuleFor(x => x.CountryId).NotNull().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.CountryId).SplitPascalCase(), 1);
            RuleFor(x => x.StateId).NotNull().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdateCompanyQuery.StateId).SplitPascalCase(), 1);

            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await companyRepository.IsCompanyExist(model.Id).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(AddUpdateCompanyQuery.Id), "company id does not exist");

                }
                var isCountryExist = await companyRepository.IsCountryExist(model.CountryId).ConfigureAwait(false);
                if (!isCountryExist)
                {
                    context.AddFailure(nameof(AddUpdateCompanyQuery.CountryId), "Country id does not exist");

                }
                var isStateExist = await companyRepository.IsStateExist(model.StateId).ConfigureAwait(false);
                if (!isStateExist)
                {
                    context.AddFailure(nameof(AddUpdateCompanyQuery.StateId), "State id does not exist");

                }
               
            });
        }
    }
}
