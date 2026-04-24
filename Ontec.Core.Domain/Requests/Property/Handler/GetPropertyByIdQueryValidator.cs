using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Property.Handler
{
    public class GetPropertyByIdQueryValidator : AbstractValidator<GetPropertyByIdQuery>
    {
        public GetPropertyByIdQueryValidator(IPropertyRepository _propertyRepository
                                            , ICompanyRepository _companyRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyByIdQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetPropertyByIdQuery.Id)));

                isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetPropertyByIdQuery.CompanyId), string.Format(CommonConstants.NotExist, nameof(GetPropertyByIdQuery.CompanyId)));
            });
        }
    }
}
