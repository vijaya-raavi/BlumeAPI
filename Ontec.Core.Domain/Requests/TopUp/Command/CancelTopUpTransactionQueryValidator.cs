using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public  class CancelTopUpTransactionQueryValidator: AbstractValidator<CancelTopUpTransactionQuery>
    {
        public CancelTopUpTransactionQueryValidator(ITopUpRepository _topUpRepository)
        {
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                int id = await _topUpRepository.IsTransactionNoExist(model.TransactionId).ConfigureAwait(false);
                if (id == 0)
                {
                    context.AddFailure(nameof(CancelTopUpTransactionQuery.TransactionId), string.Format(CommonConstants.NotExist, nameof(CancelTopUpTransactionQuery.TransactionId)));
                }
            });

        }
    }
}
