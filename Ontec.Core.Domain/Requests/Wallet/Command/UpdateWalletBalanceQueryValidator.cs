using System.Reflection;
using FluentValidation;
using MediatR;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;

namespace Ontec.Core.Domain.Requests.Wallet.Command
{
    public class UpdateWalletBalanceQueryValidator : AbstractValidator<UpdateWalletBalanceQuery>
    {
        public UpdateWalletBalanceQueryValidator(IUserRepository _userRepository, IWorkContext _workContext,IWalletRepository _walletRepository)
        {
            RuleFor(x => x.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(UpdateWalletBalanceQuery.UserId), 1);
            RuleFor(x => x.Amount).IsValidTopUpAmount();
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                var iswalletExist = await _walletRepository.IsUserIdExistInWallet(model.UserId).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(UpdateWalletBalanceQuery.UserId), string.Format(CommonConstants.NotExist, nameof(UpdateWalletBalanceQuery.UserId)));
                }
                if (iswalletExist == 0)
                {
                    context.AddFailure(nameof(UpdateWalletBalanceQuery.UserId), string.Format(CommonConstants.NotExist, nameof(UpdateWalletBalanceQuery.UserId)));
                }
                else
                {
                    if (_workContext.CurrentUserId != model.UserId)
                    {
                        context.AddFailure(nameof(UpdateWalletBalanceQuery.UserId), string.Format(CommonConstants.Unauthorized, nameof(UpdateWalletBalanceQuery.UserId)));
                    }
                }

            });
        }
    }
}
