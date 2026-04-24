using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Queries
{
    public class DownloadPurchaceRecieptPdfQueryValidator : AbstractValidator<DownloadPurchaceRecieptPdfQuery>
    {

        public DownloadPurchaceRecieptPdfQueryValidator(ITopUpRepository topUpRepository)
        {
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                if (model.TransactionId != null && model.TransactionId != "string" && model.TransactionId != "")
                {
                    var isExist = await topUpRepository.IsTransactionNoExist(model.TransactionId).ConfigureAwait(false);
                    if (isExist == 0)
                    {
                        context.AddFailure(nameof(GetTopUpTransactionsQuery.TransactionId), string.Format(CommonConstants.NotExist, nameof(GetTopUpTransactionsQuery.TransactionId)));
                    }
                    else
                    {
                        var Transaction = await topUpRepository.GetTopupTransactionDetails(model.TransactionId).ConfigureAwait(false);
                        if ((Transaction.TopupStatus != "VendSuccess" && Transaction.TopupStatus != "Recharge done successfully!" )|| Transaction.VendResponse == null  )
                        {
                            context.AddFailure(nameof(GetTopUpTransactionsQuery.TransactionId), "Transaction is failed.");

                        } 
                    }
                } 
            });
        }
    }
}
