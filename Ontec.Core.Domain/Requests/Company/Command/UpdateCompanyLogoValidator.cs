using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Company;

namespace Ontec.Core.Domain.Requests.Company.Command
{
    public class UpdateCompanyLogoValidator : AbstractValidator<UpdateCompanyLogo>
    {
        public UpdateCompanyLogoValidator(ICompanyRepository _companyRepository)
        {
            RuleFor(x => x.CompanyLogo).IsImageValid(nameof(UpdateCompanyLogo.CompanyLogo).SplitPascalCase(), 5); // 5 MB 
            RuleFor(x => x.Id).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(UpdateCompanyLogo.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await _companyRepository.IsCompanyExist(model.Id).ConfigureAwait(false);

                if (!isExist)
                {
                    context.AddFailure(nameof(UpdateCompanyLogo.Id), "Company id does not exist");
                }

                if (model.CompanyLogo == null)
                {
                    context.AddFailure(nameof(UpdateCompanyLogo.CompanyLogo), "Company logo is required.");
                }
            });
        }


    }
}
