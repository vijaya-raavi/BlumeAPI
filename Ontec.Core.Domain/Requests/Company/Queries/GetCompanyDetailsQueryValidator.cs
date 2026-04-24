using FluentValidation;
using Ontec.Core.Domain.Interface.Company;

namespace Ontec.Core.Domain.Requests.Company.Queries
{
    public  class GetCompanyDetailsQueryValidator: AbstractValidator<GetCompanyDetailsQuery>
    {
        public GetCompanyDetailsQueryValidator(ICompanyRepository companyRepository)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await companyRepository.IsCompanyExist(model.Id).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(GetCompanyDetailsQuery.Id), "Company id does not exist");

                }
            });
        }
    }
}
