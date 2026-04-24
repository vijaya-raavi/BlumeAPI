using FluentValidation;
using MediatR;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class UpdateNotifyTopUpTransactionQueryValidator : AbstractValidator<PayFastModel>
    {
        public UpdateNotifyTopUpTransactionQueryValidator(ITopUpRepository _topUpRepository)
        {
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                int id = await _topUpRepository.IsTransactionNoExist(model.m_payment_id).ConfigureAwait(false);
                if (id == 0)
                {
                    context.AddFailure(nameof(PayFastModel.m_payment_id), string.Format(CommonConstants.NotExist, nameof(PayFastModel.m_payment_id)));
                }
            });
        }
    }
}
