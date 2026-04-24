using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Wallet.Command;

namespace Ontec.Core.Application.Wallet.Command
{
    public  class WalletCommandHandler: IRequestHandler<UpdateWalletBalanceQuery, AddUpdateResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWorkContext _workContext;
        public WalletCommandHandler(IWorkContext workContext, IUserRepository userRepository, IWalletRepository walletRepository)
        {
            _workContext = workContext;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
        }
        public async Task<AddUpdateResultDto> Handle(UpdateWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new UpdateWalletBalanceQueryValidator(_userRepository, _workContext,_walletRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            int result;

            var id = await _walletRepository.IsUserIdExistInWallet(request.UserId).ConfigureAwait(false);
            double UpdatedBalance = 0;
            if (id > 0)
            {
                var userWallet = await _walletRepository.GetUserWalletById(request.UserId);
                try
                {
                    UpdatedBalance = userWallet.Balance + request.Amount;
                    AddUserWalletTransactionQuery walletTransaction = new AddUserWalletTransactionQuery
                    {
                        WalletId = userWallet.Id,
                        TransactionAmount = Convert.ToDouble(request.Amount),
                        UpdatedBalance = Convert.ToDouble(UpdatedBalance),
                        TransactionType = (int)TransactionType.Credit,
                        TransactionRemark = "Wallet credited with " + request.Amount + " amount"
                    };
                    //await _walletRepository.AddUserWalletTransaction(walletTransaction).ConfigureAwait(false);

                    result= await _walletRepository.AddUserWalletTransaction(walletTransaction).ConfigureAwait(false);                
                    if (result > 0)
                    {
                        result = await _walletRepository.UpdateUserWallet(request, UpdatedBalance).ConfigureAwait(false);
                        response.Id = result;
                        response.Message = "Transaction added successfully!";
                    }
                    else
                        response.Message = "Something went wrong!";
                }
                catch (Exception ex)
                {
                    response.Message = "Something went wrong!";
                }
               
            }


            return response;
        }

    }
}
