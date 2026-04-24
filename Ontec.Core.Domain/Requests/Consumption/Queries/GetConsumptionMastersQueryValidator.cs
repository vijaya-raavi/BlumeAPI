using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Consumption.Queries
{
    public class GetConsumptionMastersQueryValidator : AbstractValidator<GetConsumptionMastersQuery>
    {
        public GetConsumptionMastersQueryValidator(IUserRepository _userRepository
                                                   , ICompanyRepository _companyRepository
                                                   , IWorkContext _workContext)
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetConsumptionMastersQuery.UserId).SplitPascalCase(), 1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetConsumptionMastersQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetConsumptionMastersQuery.CompanyId), string.Format(CommonConstants.NotExist, nameof(GetConsumptionMastersQuery.CompanyId)));

                isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(GetConsumptionMastersQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetConsumptionMastersQuery.UserId)));

                if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Customer && _workContext.CurrentUserId != model.UserId)
                {
                    context.AddFailure(nameof(GetConsumptionMastersQuery.UserId), string.Format(CommonConstants.Unauthorized));
                }
            });
        }
    }
}