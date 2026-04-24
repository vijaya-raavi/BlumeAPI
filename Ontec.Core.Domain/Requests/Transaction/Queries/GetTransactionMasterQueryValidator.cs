using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Consumption.Queries;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class GetTransactionMasterQueryValidator : AbstractValidator<GetTransactionMasterQuery>
    {
        public GetTransactionMasterQueryValidator(IUserRepository _userRepository
                                                  , ICompanyRepository _companyRepository
                                                  , IWorkContext _workContext)
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(GetTransactionMasterQuery.UserId).SplitPascalCase(), 1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualToAsync(nameof(GetTransactionMasterQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);
                
                if (!isValid)
                {
                    context.AddFailure(nameof(GetTransactionMasterQuery.CompanyId), string.Format(CommonConstants.NotExist, nameof(GetTransactionMasterQuery.CompanyId)));
                }
                isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                {
                    context.AddFailure(nameof(GetTransactionMasterQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetTransactionMasterQuery.UserId)));
                }
                
                // if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Customer && _workContext.CurrentUserId != model.UserId)
                // {
                //     context.AddFailure(nameof(GetTransactionMasterQuery.UserId), string.Format(CommonConstants.Unauthorized));
                // }
            });
        }
    }
}
