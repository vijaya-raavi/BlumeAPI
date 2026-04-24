using System.Threading;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Requests.Transaction.Queries;

namespace Ontec.Core.Domain.Requests.Purchase.Queries
{
    public class GetSTSMetersByPropertyIdQueryValidator:AbstractValidator<GetSTSMetersByPropertyIdQuery>
    {
        public GetSTSMetersByPropertyIdQueryValidator(IUserRepository _userRepository
                                                  , ICompanyRepository _companyRepository
                                                  , IWorkContext _workContext)
        {
            RuleFor(m => m.OwnerId).NotNull().GreaterThanOrEqualToAsync(nameof(GetSTSMetersByPropertyIdQuery.OwnerId).SplitPascalCase(), 1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetSTSMetersByPropertyIdQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
                {
                    var isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);

                    if (!isValid)
                    {
                        context.AddFailure(nameof(GetSTSMetersByPropertyIdQuery.CompanyId), string.Format(CommonConstants.NotExist, nameof(GetSTSMetersByPropertyIdQuery.CompanyId)));
                    }
                    isValid = await _userRepository.IsUserIdExist(model.OwnerId).ConfigureAwait(false);
                    if (!isValid)
                    {
                        context.AddFailure(nameof(GetSTSMetersByPropertyIdQuery.OwnerId), string.Format(CommonConstants.NotExist, nameof(GetSTSMetersByPropertyIdQuery.OwnerId)));
                    }


                });
        }
    }
}
