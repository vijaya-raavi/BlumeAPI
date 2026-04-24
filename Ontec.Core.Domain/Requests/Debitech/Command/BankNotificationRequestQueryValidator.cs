using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.Debitech.Command
{
    public class BankNotificationRequestQueryValidator : AbstractValidator<BankNotificationRequest>
    {
        public BankNotificationRequestQueryValidator(ITopUpRepository topUpRepository)
        {
            //RuleFor(m => m.NetUpTransactionGuid).NotNull();
            //RuleFor(m => m.TransactionValue).NotNull();
            //RuleFor(m => m.IsCreditTransaction).NotNull();
            //RuleFor(m => m.AccountNumber).NotNull();
            //RuleFor(m => m.PayerReferenceNumber).NotNull();
            //RuleFor(m => m.NetUpChecksum).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                 var count = await topUpRepository.IsNetUpTransactionGuidExist(model.NetUpTransactionGuid).ConfigureAwait(false);
                 if (count > 0)
                 {
                     context.AddFailure(nameof(BankNotificationRequest.NetUpTransactionGuid), string.Format(CommonConstants.AlreadyExist, nameof(BankNotificationRequest.NetUpTransactionGuid)));
                 }
            });
        }
    }
}
