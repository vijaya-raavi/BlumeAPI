using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.User.Handler.Command;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Otp;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.Wallet;
using Ontec.Core.Domain.Models.Dto.BulkUpload;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Wallet;
using Ontec.Core.Domain.Requests.BulkUpload.Command;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Application.BulkUpload.Command
{
    public class BulkUploadCommandHandler : IRequestHandler<BulkUploadRequestCommand, BulkUploadResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IWorkContext _workContext;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IEncryptionandDecryption _encryptionandDecryption;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IOtpService _otpService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ILogger<UserCommandHandller> _logger;
        private readonly IMeterRepository _meterRepository;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IAuditTrail _auditTrail;
        private readonly IBulkUploadRepository _bulkUploadRepository;
        //private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBulkUploadQueue _bulkUploadQueue;
        private readonly IWalletRepository _walletRepository;
        public BulkUploadCommandHandler(IUserRepository userRepository
                                   , ICommunicationRepository communicationRepository
                                   , IDocumentRepository documentRepository
                                   , IWorkContext workContext
                                   , IUserRoleRepository userRoleRepository
                                   , IEncryptionandDecryption encryptionandDecryption
                                   , IConfigurationRepository configurationRepository
                                    , IOtpService otpService
                                   , ICompanyRepository companyRepository
                                   , INotificationRepository notificationRepository
                                    , IOtpRepository otpRepository
                                    , ILogger<UserCommandHandller> logger
                                    , IPropertyRepository propertyRepository
                                    , IMeterRepository meterRepository
                                    , IMasterApiConnectService masterApiConnectService
                                   , MasterApiSetting masterApiSetting
                                  , IAuditTrail auditTrail
                                , IBulkUploadRepository bulkUploadRepository
                               , IServiceScopeFactory serviceScopeFactory
                                , IBulkUploadQueue bulkUploadQueue
                                , IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _communicationRepository = communicationRepository;
            _documentRepository = documentRepository;
            _workContext = workContext;
            _userRoleRepository = userRoleRepository;
            _encryptionandDecryption = encryptionandDecryption;
            _configurationRepository = configurationRepository;
            _companyRepository = companyRepository;
            _otpService = otpService;
            _notificationRepository = notificationRepository;
            _otpRepository = otpRepository;
            _logger = logger;
            _propertyRepository = propertyRepository;
            _meterRepository = meterRepository;
            _masterApiSetting = masterApiSetting;
            _masterApiConnectService = masterApiConnectService;
            _auditTrail = auditTrail;
            _bulkUploadRepository = bulkUploadRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _bulkUploadQueue = bulkUploadQueue;
            _walletRepository = walletRepository;
        }

        public async Task<BulkUploadResultDto> Handle(BulkUploadRequestCommand request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new BulkUploadRequestCommandValidator(_userRepository, _propertyRepository, _meterRepository, _companyRepository,
                                                _masterApiConnectService, _masterApiSetting, _configurationRepository, _workContext, _userRoleRepository, _communicationRepository, _documentRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            // ✅ dedupe/merge — unchanged
            var users = request.UploadBulkUsers.GroupBy(x => new
            {
                Email = x.Email.ToLower().Trim(),
                Mobile = x.Mobile.Trim()
            })
            .Select(g =>
            {
                var first = g.First();
                first.TaxNumber ??= string.Empty;
                if (string.IsNullOrWhiteSpace(first.TermConditionsVersion) || first.TermConditionsVersion == "string")
                    first.TermConditionsVersion = string.Empty;
                first.UserProperties = g.Where(x => x.UserProperties != null)
                                         .SelectMany(x => x.UserProperties)
                                         .ToList();
                return first;
            })
            .ToList();


            if (!request.IsSingleUser)
            {
                // ✅ real user, not new Guid()
                var batchId = await _bulkUploadRepository.CreateBatchAsync(users.Count, _workContext.CurrentUserId);

                // ✅ resolve document uploads NOW (per user), before staging — IFormFile can't be deferred
                var records = new List<BulkUploadRecord>();
                int rowNum = 1;

                foreach (var user in users)
                {
                    int? docId = 0;
                    if (user.UserDocument != null && user.UserDocument.FileDoc != null)
                        docId = await _userRepository.InsertUserDocument(0, user.UserDocument);

                    var staged = new StagedBulkUser
                    {
                        User = user,
                        DocumentId = docId
                    };

                    records.Add(new BulkUploadRecord
                    {
                        RowNumber = rowNum++,
                        Payload = JsonSerializer.Serialize(staged)
                    });
                }

                await _bulkUploadRepository.InsertRecordsAsync(batchId, records);


                _bulkUploadQueue.Enqueue(batchId);

                return new BulkUploadResultDto
                {
                    BatchId = batchId,
                    Message = $"Batch accepted for processing. BatchId: {batchId}, Total: {users.Count}"
                };
            }
            else
            {

                int userId = 0;
                int walletId = 0;
                try
                {
                    userId = await _userRepository.BulkInsertUsers(request.UploadBulkUsers).ConfigureAwait(false);

                    if (userId > 0)
                    {
                        walletId = await _walletRepository.AddUserWallet(userId).ConfigureAwait(false);
                    }
                    var user =await _userRepository.GetUserById(userId).ConfigureAwait(false);
                    var emailModel = new EmailModelClass
                    {
                        companyId = user.CompanyId,
                        email = user.Email,
                        mobile = user.Mobile,
                        propertyUser = string.IsNullOrWhiteSpace(user.FirstName) ? user.Email : user.FirstName,
                        forEvent = "newbulkUser",
                        body = string.Empty,
                        title = string.Empty,
                        subtitle = string.Empty
                    };

                    await _otpService.SendEventMail(emailModel);


                    return new BulkUploadResultDto
                    {
                        UserId = userId,
                        BatchId = new Guid(),
                        Message = $"Single user added successfully"
                    };
                }
                catch (Exception ex)
                {
                    return new BulkUploadResultDto
                    {
                        UserId = 0,
                        BatchId = new Guid(),
                        Message = $"Something went wrong"
                    };
                }
            }

        }
    }
}
