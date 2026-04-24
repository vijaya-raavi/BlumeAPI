using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class GetTopUpTransactionsQueryValidator :AbstractValidator<GetTopUpTransactionsQuery>
    {
        public GetTopUpTransactionsQueryValidator(ITopUpRepository topUpRepository) 
        {
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
           {
               if (model.TransactionId != null && model.TransactionId !="string" && model.TransactionId !="")
               {
                   var isExist = await topUpRepository.IsTransactionNoExist(model.TransactionId).ConfigureAwait(false);
                   if (isExist == 0)
                   {
                       context.AddFailure(nameof(GetTopUpTransactionsQuery.TransactionId), string.Format(CommonConstants.NotExist, nameof(GetTopUpTransactionsQuery.TransactionId)));
                   }
                  
               }

           });
        }
    }
}
