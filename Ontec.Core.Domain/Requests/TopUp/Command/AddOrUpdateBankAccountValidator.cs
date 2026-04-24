using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class AddOrUpdateBankAccountValidator : AbstractValidator<AddOrUpdateBankAccountQuery>
    {
        public AddOrUpdateBankAccountValidator(ITopUpRepository _topUpRepository,IWorkContext _workContext)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(m => m.BankName).NotNullAndEmptyAsync().IsValidFullName().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateBankAccountQuery.BankName).SplitPascalCase(), 20);
            RuleFor(m => m.AccountNumber).NotNullAndEmptyAsync().IsValidNumber().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateBankAccountQuery.AccountNumber).SplitPascalCase(), 19);
            RuleFor(m => m.AccountName).NotNullAndEmptyAsync().IsValidFullName().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateBankAccountQuery.AccountName).SplitPascalCase(), 100);
            RuleFor(m => m.BranchCode).NotNullAndEmptyAsync().IsValidNumber().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateBankAccountQuery.BranchCode).SplitPascalCase(), 6);
            RuleFor(m => m).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await _topUpRepository.IsBankAccountExist(model.AccountNumber,model.Id).ConfigureAwait(false);
               
                if (isExist > 0)
                {
                    context.AddFailure(nameof(AddOrUpdateBankAccountQuery.AccountNumber), "Account number already registered");
                }
                if (_workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && _workContext.CurrentRoleId!=(int)RoleMasterEnum.Operator)
                {
                    context.AddFailure(nameof(_workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(_workContext.CurrentRoleId)));
                }
            });
        }
            
    }
}
