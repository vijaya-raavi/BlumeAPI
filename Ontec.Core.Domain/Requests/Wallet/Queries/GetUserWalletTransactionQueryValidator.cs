using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Command;

namespace Ontec.Core.Domain.Requests.Wallet.Queries
{
    public  class GetUserWalletTransactionQueryValidator:AbstractValidator<GetUserWalletTransactionQuery>
    {
        public GetUserWalletTransactionQueryValidator(IUserRepository _userRepository,IWalletRepository _walletRepository) 
        {
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                if (model.UserId > 0)
                {
                    var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                    var iswalletExist = await _walletRepository.IsUserIdExistInWallet(model.UserId).ConfigureAwait(false);
                    if (!isExist)
                    {
                        context.AddFailure(nameof(GetUserWalletTransactionQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetUserWalletTransactionQuery.UserId)));
                    }
                    if (iswalletExist == 0)
                    {
                        context.AddFailure(nameof(GetUserWalletTransactionQuery.UserId), string.Format(CommonConstants.NotExist, nameof(GetUserWalletTransactionQuery.UserId)));
                    }
                }              

            });
        }
    }
}
