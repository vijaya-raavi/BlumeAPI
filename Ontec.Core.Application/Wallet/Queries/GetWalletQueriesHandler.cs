using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.Wallet.Queries;

namespace Ontec.Core.Application.Wallet.Queries
{
    public class GetWalletQueriesHandler :  IRequestHandler<GetUserWalletQuery, UserWalletDto>
                                            ,IRequestHandler<GetUserWalletTransactionQuery, IEnumerable<UserWalletTransactionDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;

        public GetWalletQueriesHandler(IUserRepository userRepository, IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
        }
        public async Task<UserWalletDto> Handle(GetUserWalletQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetUserWalletQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _walletRepository.GetUserWalletById(request.UserId).ConfigureAwait(false);
        }
        public async Task<IEnumerable<UserWalletTransactionDto>> Handle(GetUserWalletTransactionQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetUserWalletTransactionQueryValidator(_userRepository,_walletRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _walletRepository.GetUserWalletTransaction(request).ConfigureAwait(false);
        }
    }
}
