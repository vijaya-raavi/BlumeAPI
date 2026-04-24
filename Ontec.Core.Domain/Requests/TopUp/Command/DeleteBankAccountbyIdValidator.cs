using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.TopUp;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public  class DeleteBankAccountbyIdValidator : AbstractValidator<DeleteBankAccountbyId>
    {
        public DeleteBankAccountbyIdValidator(ITopUpRepository _topUpRepository, IWorkContext workContext)
        {

            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await _topUpRepository.IsBankAccountIdExist(model.Id).ConfigureAwait(false);
                if (!isExist)
                    context.AddFailure(nameof(DeleteBankAccountbyId.Id), string.Format(CommonConstants.NotExist, nameof(DeleteBankAccountbyId.Id)));
            });
        }
    }
}
