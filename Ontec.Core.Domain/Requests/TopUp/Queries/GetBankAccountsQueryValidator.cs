using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Requests.TopUp.Command;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetBankAccountsQueryValidator:AbstractValidator<GetBankAccountsQuery>
    {
        public GetBankAccountsQueryValidator(ITopUpRepository _topUpRepository,IWorkContext _workContext ) 
        {
            RuleFor(m => m).CustomAsync(async (model, context, CancellationToken) =>
            {
                if (model.Id > 0)
                {
                    var isExist = await _topUpRepository.IsBankAccountIdExist(model.Id).ConfigureAwait(false);

                    if (!isExist)
                    {
                        context.AddFailure(nameof(GetBankAccountsQuery.Id), string.Format(CommonConstants.NotExist, nameof(GetBankAccountsQuery.Id)));
                    }
                  
                }
               
            });
        }
    }
}
