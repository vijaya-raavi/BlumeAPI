using System.Data;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.BulkUpload;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Application.Common.Helper
{
    public interface IUserProvisioningService
    {
        Task<UserProvisioningResult> CreateUserAsync(BulkUsers dto, int? preResolvedDocId);
    }
    public class UserProvisioningService : IUserProvisioningService
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IUserRepository _userRepository;              // has InsertUser, InsertUserDocument, IsBulkUserEmailExist, IsBulkUserMobileExist
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IOtpService _emailService;
        private readonly ILogger<UserProvisioningService> _logger;

        public UserProvisioningService(
            IGenericRepository genericRepository,
            IUserRepository userRepository,
            ICommunicationRepository communicationRepository,
            IPropertyRepository propertyRepository,
            IWalletRepository walletRepository,
            IOtpService emailService,
            ILogger<UserProvisioningService> logger)
        {
            _genericRepository = genericRepository;
            _userRepository = userRepository;
            _communicationRepository = communicationRepository;
            _propertyRepository = propertyRepository;
            _walletRepository = walletRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UserProvisioningResult> CreateUserAsync(BulkUsers user, int? preResolvedDocId)
        {
            IDbTransaction transaction = null;
            try
            {
                // ── 1. Business validation ──────────────────────────
                var emailExists = await _userRepository.IsBulkUserEmailExist(user.Email, user.CompanyId);
                if (emailExists)
                    return new UserProvisioningResult { Success = false, ErrorMessage = $"Email '{user.Email}' already exists." };

                var mobileExists = await _userRepository.IsBulkUserMobileExist(user.Mobile, user.CompanyId);
                if (mobileExists)
                    return new UserProvisioningResult { Success = false, ErrorMessage = $"Mobile '{user.Mobile}' already exists." };

                // ── 2. Transaction scope ────────────────────────────
                transaction = _genericRepository.TransactionOpen();

                var userId = await _userRepository.InsertUser(user, preResolvedDocId ?? 0);

                var tasks = new List<Task>();

                if (user.CommunicationTypesIds?.Any() == true)
                    tasks.Add(_communicationRepository.InsertUserCommunications(user.CommunicationTypesIds, userId));

                if (user.UserProperties?.Any() == true)
                    tasks.Add(_userRepository.InsertPropertiesParallel(userId, user.UserProperties, user.CompanyId));

                await Task.WhenAll(tasks);

                int walletCreated = await _walletRepository.AddUserWallet(userId);
                if (walletCreated == 0)
                    throw new Exception($"Wallet creation failed for user {userId}.");

                transaction.Commit();

                return new UserProvisioningResult
                {
                    Success = true,
                    UserId = userId,

                };
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                _logger.LogError(ex, "Failed to provision user {Email}", user.Email);
                return new UserProvisioningResult { Success = false, ErrorMessage = ex.Message };
            }
            finally
            {
                _genericRepository.TransactionClose();
            }
        }
    }

}
