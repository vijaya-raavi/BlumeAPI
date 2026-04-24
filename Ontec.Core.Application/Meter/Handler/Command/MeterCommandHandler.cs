using System.Drawing;
using iText.Layout.Element;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Notification.Queries;
using Org.BouncyCastle.Crypto;
using Scriban;

namespace Ontec.Core.Application.Meter.Handler.Command
{
    public class MeterCommandHandler : IRequestHandler<DeleteMeterById, string>
                                        , IRequestHandler<AddUpdateMeterQuery, AddUpdateResultDto>
                                        , IRequestHandler<GetApproveRejectMeterQuery, AddUpdateResultDto>
                                        , IRequestHandler<AddMetersFromMeterListQuery, AddUpdateResultDto>
    {
        private readonly IMeterRepository _meterRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMasterApiConnectService _masterApiConnectService;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly MasterApiSetting _masterApiSetting;
        private readonly IWorkContext _workContext;
        private readonly ICommonService _commonService;
        private readonly IPushNotification _pushNotification;
        private readonly IUserRepository _userRepository;
        private readonly IOtpService _otpService;
        private readonly ICompanyHelper _companyHelper;
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly INotificationRepository _notificationRepository;
        private IHostingEnvironment Environment;
        private readonly IAuditTrail _auditTrail;
        public MeterCommandHandler(IMeterRepository meterRepository
                                   , IPropertyRepository propertyRepository
                                   , IHostingEnvironment _environment
                                   , IDocumentRepository documentRepository
                                   , IMasterApiConnectService masterApiConnectService
                                   , MasterApiSetting masterApiSetting
                                   , IConfigurationRepository configurationRepository
                                   , ICommonService commonService
                                   , IWorkContext workContext
                                   , INotificationRepository notificationRepository
                                   , IPushNotification pushNotification
                                   , IUserRepository userRepository
                                   , IOtpService otpService
                                   , ICompanyHelper companyHelper
                                   , IEmailTemplateRepository emailTemplateRepository,
IAuditTrail auditTrail)
        {
            _meterRepository = meterRepository;
            _propertyRepository = propertyRepository;
            Environment = _environment;
            _documentRepository = documentRepository;
            _workContext = workContext;
            _masterApiConnectService = masterApiConnectService;
            _configurationRepository = configurationRepository;
            _masterApiSetting = masterApiSetting;
            _commonService = commonService;
            _notificationRepository = notificationRepository;
            _pushNotification = pushNotification;
            _userRepository = userRepository;
            _otpService = otpService;
            _companyHelper = companyHelper;
            _emailTemplateRepository = emailTemplateRepository;
            _auditTrail = auditTrail;
        }

        public async Task<AddUpdateResultDto> Handle(AddUpdateMeterQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddUpdateMeterQueryValidator(_propertyRepository, _meterRepository, _documentRepository, _workContext
                                                                   , _masterApiConnectService, _masterApiSetting
                                                                   , _configurationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var response = new AddUpdateResultDto();
            var eftNo = "";
            int eftNoCount = 0;
            if (request.ContractEndDate != null)
            {
                DateTime today = DateTime.UtcNow.Date;
                bool isValidDate = request.ContractEndDate.ParseDateTime() >= today;
                if (!isValidDate)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(AddUpdateMeterQuery.ContractEndDate),
                        ErrorMessage = "Invalid date format, date should be greater than today."
                    });
                }
            }
            if (request.ContractProofDocument != null)
            {
                DateTime today = DateTime.UtcNow.Date;
                bool isValidDate = request.ContractEndDate.ParseDateTime() >= today;
                if (!isValidDate)
                {
                    validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                    {
                        PropertyName = nameof(AddUpdateMeterQuery.ContractEndDate),
                        ErrorMessage = "Invalid date format, date should be greater than today."
                    });
                }
                else
                {
                    var date = DateTime.UtcNow.Date.Day;
                    request.ContractProofDocumentTypeId = (int)DocumentTypeEnum.ContractProof; // by default document should be Contract for Meter
                    var uploadDocDto = new UploadDocumentDto
                    {
                        UploadFile = request.ContractProofDocument,
                        FileName = request.PropertyId + "_" + request.Id.ToString() + "_" + date + Path.GetExtension(request.ContractProofDocument.FileName)
                    };
                    var documnetPath = await _documentRepository.SaveDocument(uploadDocDto, false).ConfigureAwait(false);
                    var doc = new DocumentDto
                    {
                        Id = 0,
                        DocumentTypeId = (int)DocumentTypeEnum.ContractProof,
                        extension = Path.GetExtension(request.ContractProofDocument.FileName),
                        StatusId = (int)StatusEnum.Active,
                        Title = request.MeterNumber,
                        Url = documnetPath
                    };

                    request.ContractProofDocumentId = await _documentRepository.AddDocument(doc).ConfigureAwait(false);
                }
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            double InputdailyTarget = request.DailyTargetConsumption;
            int result;
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;

            string timeZoneId = "South Africa Standard Time"; // Windows ID
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime harareTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            formattedTime = formattedTime.Replace('.', ':');
            bool isVerified = false;
            bool isBusinessHours = false;
            TimeSpan fromBusinessHours = TimeSpan.Zero;
            TimeSpan toBusinessHours = TimeSpan.Zero;
            bool isEnableNonBusinessHours = false;
            bool isEnableBusinessHours = false;
            bool enabled = false;
            var businessConfigurations = await _configurationRepository.GetBusinessHoursConfigurations().ConfigureAwait(false);


            var day = System.DateTime.Now.ToString("dddd");
            if (businessConfigurations != null && businessConfigurations.Any(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)))
            {

                fromBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).From;

                toBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).To;

                isEnableBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptMeterBusiness;

                enabled = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).Enabled;

                isEnableNonBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptMeterNonBusiness;


            }
            DateTime currentdate = Convert.ToDateTime(harareTime).ToUniversalTime();
            TimeSpan currentTime = harareTime.TimeOfDay;
            if (request.Id == 0)
            {


                if (currentTime >= fromBusinessHours && currentTime <= toBusinessHours)
                {
                    isBusinessHours = true;
                }
                else
                {

                    isBusinessHours = false;
                }
                if (enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (!enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "";
                    isVerified = false;
                }

                if (enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {

                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }


                if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {

                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }

                if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }

                if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Pending;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }

                if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "Meter send to admin for approval.";
                    isVerified = false;
                }
                if (!enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }
                if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                {
                    request.StatusId = (int)StatusEnum.Active;
                    request.Comments = "";
                    isVerified = false;
                }

                var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("meterapproval")))
                {
                    var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("meterapproval"));
                    if (approvalConfig != null && !string.IsNullOrEmpty(approvalConfig.Value) && approvalConfig.Value == "1")
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter sent for admin approval";
                    }
                    else
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                    }

                }
            }


            if (request.Id > 0)
            {
                var meter = await _meterRepository.GetMeterById(request.Id).ConfigureAwait(false);

                var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("meterapproval")))
                {
                    var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("meterapproval"));
                    if (approvalConfig != null && !string.IsNullOrEmpty(approvalConfig.Value) && approvalConfig.Value == "1")
                    {
                        if (meter.StatusId == (int)StatusEnum.Rejected)
                            request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter sent for admin approval";
                    }
                    else
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";

                    }
                }

                if (currentTime >= fromBusinessHours && currentTime <= toBusinessHours)
                {
                    isBusinessHours = true;
                }
                else
                {
                    isBusinessHours = false;
                }
                if (request.StatusId == (int)StatusEnum.Rejected)
                {
                    if (enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }

                    if (!enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "";
                        isVerified = false;
                    }

                    if (enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {

                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }




                    if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {

                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }

                    if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }


                    if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Pending;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }

                    if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        request.StatusId = (int)StatusEnum.Active;
                        request.Comments = "";
                        isVerified = false;
                    }

                }

                meter.ContractEndDate = request.ContractEndDate;
                if (request.ContractProofDocument != null)
                {
                    meter.ContractProofDocumentId = request.ContractProofDocumentId.Value;
                }


                meter.DailyTargetConsumption = request.DailyTargetConsumption;
                meter.PropertyId = request.PropertyId;
                meter.MeterTypeId = request.MeterTypeId;
                meter.Comments = request.Comments;
                if (request.StatusId == null)
                    request.StatusId = meter.StatusId;

                if (request.StatusId != null)
                {
                    meter.StatusId = request.StatusId.Value;
                }
                meter.MeterAlias = request.MeterAlias;
                result = await _meterRepository.UpdateMeter(meter).ConfigureAwait(false);
                try
                {
                    if (string.IsNullOrEmpty(meter.EFTNumber))
                    {
                        meter.EFTNumber = await GenerateEFTNumber(result);
                        eftNo = await GenerateEFTNumber(result).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(eftNo))
                            eftNoCount = await _meterRepository.IsEftNoExist(eftNo).ConfigureAwait(false);
                        if (eftNoCount > 0)
                        {
                            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                            {
                                PropertyName = nameof(eftNo),
                                ErrorMessage = "EFTNumber already Exist ,it should be unique."
                            });
                        }
                        else
                        {
                            await _meterRepository.UpdateEFTNumberByMeterId(result, eftNo).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex) { }
                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "Meter updated",
                    Description = "Meter : " + request.MeterNumber + "  updated successfully",
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Register

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }
            else
            {

                int metereMasterTypeId = 0;





                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + request.MeterNumber + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);

                result = await _meterRepository.AddMeter(request, metereMasterTypeId, eftNo, isVerified).ConfigureAwait(false);
                try
                {
                    eftNo = await GenerateEFTNumber(result).ConfigureAwait(false);

                    if (eftNo != null)
                    {
                        eftNoCount = await _meterRepository.IsEftNoExist(eftNo).ConfigureAwait(false);
                        if (eftNoCount > 0)
                        {
                            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                            {
                                PropertyName = nameof(eftNo),
                                ErrorMessage = "EFTNumber already Exist ,it should be unique."
                            });
                        }
                        else
                        {
                            await _meterRepository.UpdateEFTNumberByMeterId(result, eftNo).ConfigureAwait(false);
                        }
                    }

                }
                catch (Exception ex) { }

                if (meterResult != null)
                {
                    var customerAgreementId = "";
                    var mastercustomerAgreementId = "";
                    customerAgreementId = await _propertyRepository.GetCustomerAgreementId(request.PropertyId).ConfigureAwait(false);
                    var meterdata = meterResult.Data;

                    foreach (var item in meterdata)
                    {
                        if (item.CustomerAgreement != null)
                        {
                            if (item.CustomerAgreement.Id != "")
                            {
                                mastercustomerAgreementId = item.CustomerAgreement.Id.ToString();
                                if ((customerAgreementId == null || customerAgreementId == "") && mastercustomerAgreementId != "")
                                {
                                    await _propertyRepository.UpdatePropertyCustomerAgreementId(request.PropertyId, mastercustomerAgreementId);

                                }
                            }
                        }
                    }

                }

                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "New meter registered",
                    Description = "New meter : " + request.MeterNumber + "  registered successfully",
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Register

                };
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Records created successfully!";
                else
                    response.Message = "Records updated successfully!";
            }
            return response;
        }

        public async Task<string> Handle(DeleteMeterById request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();
            request.TrimAllStrings();
            var commonValidator = new DeleteMeterByIdValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var meter = await _meterRepository.GetMeterById(request.Id).ConfigureAwait(false);
            var propertyId = meter.PropertyId;
            int propId = await _propertyRepository.UpdateCustomerAgreementId(propertyId).ConfigureAwait(false);

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Update Property CustomerAgreementId(";
                objAudit.ActionTable = "ohd_property";
                objAudit.ModuleName = "Property";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.UpdatedId = propId;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            await _meterRepository.DeleteMeterById(request.Id).ConfigureAwait(false);

            AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
            {
                UserID = _workContext.CurrentUserId,
                Title = "Meter removed",
                Description = meter.MeterNumber + " :  " + "removed",
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Notification

            };

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Delete Meter";
                objAudit.ActionTable = "ohd_meter";
                objAudit.ModuleName = "Meter";
                objAudit.StatusId = (int)StatusEnum.Inactive;
                objAudit.UpdatedId = request.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

            return "Meter deleted successfully!";
        }
        #region ApproveRejectMeterRequest
        public async Task<AddUpdateResultDto> Handle(GetApproveRejectMeterQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new GetApproveRejectMeterQueryValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);

            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            int result;
            var meter = await _meterRepository.GetMeterById(request.MeterID).ConfigureAwait(false);

            result = await _meterRepository.ApproveRejectMeterById(request).ConfigureAwait(false);

            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {

                objAudit.AddedBy = _workContext.CurrentUserId;
                if (request.IsApproved)
                {
                    objAudit.Action = "Approved Meter";
                    objAudit.StatusId = (int)StatusEnum.Active;
                }
                if (request.IsChecked && !request.IsApproved)
                {
                    objAudit.Action = "Deactivated Meter";
                    objAudit.StatusId = (int)StatusEnum.Deactive;
                }
                if (!request.IsApproved)
                {
                    objAudit.Action = "Rejected Meter";
                    objAudit.StatusId = (int)StatusEnum.Rejected;
                }
                objAudit.ActionTable = "ohd_meter";
                objAudit.ModuleName = "Meter";

                objAudit.UpdatedId = request.MeterID;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }

            AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
            {
                //UserID = 1408,
                UserID = _workContext.CurrentUserId,
                Title = "Meter request " + (request.IsApproved ? "Approved" : "Rejected"),
                Description = meter.MeterNumber + " :  " + (request.IsApproved ? "Approved" : "Rejected"),
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Notification

            };


            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            string reason = "";
            string mailBody = "";
            string approvalStatus = "";
            var user = await _userRepository.GetUserByMeterId(request.MeterID).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            var emailTemplates = await _emailTemplateRepository.GetEmailTemplates().ConfigureAwait(false);
            var welcomeEmail = new EmailTemplateDto();
            if (result > 0)
            {
                response.Id = result;
                if (result == 0)
                    response.Message = "Something Went wrong!";
                else
                {
                    var meterResult = await _meterRepository.GetOwnerIdByMeterId(request.MeterID).ConfigureAwait(false);
                    var status = request.IsApproved ? "approved" : "rejected";
                    var sendNotification = new SendUserNotificationQuery
                    {
                        UserId = meterResult.Id,
                        Message = "Your meter " + meterResult.MeterNumber + " is " + status + " by admin, kindly check."
                    };
                    await _commonService.SendUserNotificationQuery(sendNotification).ConfigureAwait(false);

                    if (user != null && !string.IsNullOrEmpty(user.DeviceToken) && user.DeviceToken != "string")
                    {
                        await _pushNotification.SendMessage(newNotification.Title, newNotification.Description, user.DeviceToken, user.Id).ConfigureAwait(false);

                    }
                    if (!request.IsApproved)
                    {
                        approvalStatus = "rejected";
                        welcomeEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Approve Meter"));
                        if (user != null)
                        {
                            if (!string.IsNullOrEmpty(welcomeEmail.Html))
                            {
                                var model = new PropertyUserWelcomeEmailDto
                                {
                                    CompanyName = companyDetails.Name,
                                    FirstName = user.FirstName,
                                    Email = user.Email,
                                    Mobile = user.Mobile,
                                    companyEmail = companyDetails.Email,
                                    Domain = companyDetails.Domain,
                                    companyLogo = companyDetails.RelativeUrl,
                                    MeterNumber = meter.MeterNumber,
                                    ApprovalStatus = approvalStatus,
                                    RejectReason = request.Comments,
                                };
                                var template = Template.Parse(welcomeEmail.Html);
                                welcomeEmail.Html = template.Render(model, memberRenamer: member => member.Name);
                            }
                            mailBody = welcomeEmail.Html;
                        }

                        reason = "Rejection reason: " + request.Comments;
                        //mailBody = "Your meter  mumber " + meter.MeterNumber + " is " + approvalStatus + "/" + reason;

                    }
                    if (request.IsApproved)
                    {
                        approvalStatus = "approved";
                        welcomeEmail = emailTemplates.FirstOrDefault(g => g.Name.Equals("Approve Meter"));
                        if (user != null)
                        {
                            if (!string.IsNullOrEmpty(welcomeEmail.Html))
                            {
                                var model = new PropertyUserWelcomeEmailDto
                                {
                                    CompanyName = companyDetails.Name,
                                    FirstName = user.FirstName,
                                    Email = user.Email,
                                    Mobile = user.Mobile,
                                    companyEmail = companyDetails.Email,
                                    Domain = companyDetails.Domain,
                                    companyLogo = companyDetails.RelativeUrl,
                                    MeterNumber = meter.MeterNumber,
                                    ApprovalStatus = approvalStatus,
                                };
                                var template = Template.Parse(welcomeEmail.Html);
                                welcomeEmail.Html = template.Render(model, memberRenamer: member => member.Name);
                            }
                            mailBody = welcomeEmail.Html;
                            //mailBody = "Your meter  mumber  " + meter.MeterNumber + " is " + approvalStatus;
                        }
                    }
                    if (user != null)
                    {
                        EmailModelClass obj = new()
                        {

                            title = "Meter request " + approvalStatus,
                            email = user.Email,
                            forEvent = "MeterStatus",
                            subtitle = "",
                            mobile = user.Mobile,
                            propertyUser = user.UserName,
                            body = mailBody,
                            documentPath = "",
                            companyId = user.CompanyId
                        };
                        await _otpService.SendEventMail(obj).ConfigureAwait(false);
                    }
                }

            }
            response.Message = "Meter status updated successfully!";

            return response;
        }
        #endregion

        public async Task<AddUpdateResultDto> Handle(AddMetersFromMeterListQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddMetersFromMeterListQueryValidator(_propertyRepository, _workContext, _meterRepository, _masterApiConnectService, _masterApiSetting);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();
            if (request.Meter.Count > 0)
            {
                var existingMeterList = await _meterRepository.GetAllMetersByPropertyId(request.PropertyId).ConfigureAwait(false);
                var existingMeters = existingMeterList
                          .Where(t => t.Status.Equals("Active") || t.Status.Equals("Pending") || t.Status.Equals("Rejected") || t.Status.Equals("De active"))
                          .Select(t => t.MeterNumber)
                          .ToList();


                var newMeters = request.Meter;
                var filteredNewMeters = newMeters
                                        .Where(newMeter => !existingMeters.Contains(newMeter.MeterNumber))
                                        .ToList();

                var AllMetersList = await _meterRepository.GetAllMeters().ConfigureAwait(false);
                var AllExistingMeters = AllMetersList.Select(t => t.MeterNumber).ToList();
                var filteredFromAllMeters = filteredNewMeters
                                           .Where(filteredNewMeters => !AllExistingMeters.Contains(filteredNewMeters.MeterNumber))
                                           .ToList();


                request.Meter = filteredFromAllMeters;
                var firstMeter = filteredFromAllMeters.Select(a => a.MeterNumber).FirstOrDefault();
                AddMetersFromListHelperClass obj = new AddMetersFromListHelperClass();
                int eftNocount = 0;
                var eftNo = "";
                int result;
                string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
                string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;

                formattedTime = formattedTime.Replace('.', ':');
                bool isVerified = false;
                bool isBusinessHours = false;



                string timeZoneId = "South Africa Standard Time"; // Windows ID
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                DateTime harareTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

                if (request.Meter.Count > 0)
                {
                    //var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                    //if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("meterapproval")))
                    //{
                    //    var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("meterapproval"));
                    //    if (approvalConfig != null && !string.IsNullOrEmpty(approvalConfig.Value) && approvalConfig.Value == "1")
                    //    {
                    //        obj.StatusId = (int)StatusEnum.Pending;
                    //    }
                    //    else
                    //    {
                    //        obj.StatusId = (int)StatusEnum.Active;
                    //    }
                    //}


                    TimeSpan fromBusinessHours = TimeSpan.Zero;
                    TimeSpan toBusinessHours = TimeSpan.Zero;
                    bool isEnableNonBusinessHours = false;
                    bool isEnableBusinessHours = false;
                    bool enabled = false;
                    var businessConfigurations = await _configurationRepository.GetBusinessHoursConfigurations().ConfigureAwait(false);


                    var day = System.DateTime.Now.ToString("dddd");

                    if (businessConfigurations != null && businessConfigurations.Any(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)))
                    {

                        fromBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).From;

                        toBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).To;

                        isEnableBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptMeterBusiness;

                        enabled = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).Enabled;

                        isEnableNonBusinessHours = businessConfigurations.FirstOrDefault(t => t.Name.Equals(day, StringComparison.OrdinalIgnoreCase)).AcceptMeterNonBusiness;


                    }
                    DateTime currentdate = Convert.ToDateTime(harareTime).ToUniversalTime();
                    TimeSpan currentTime = harareTime.TimeOfDay;

                    if (currentTime >= fromBusinessHours && currentTime <= toBusinessHours)
                    {
                        isBusinessHours = true;
                    }
                    else
                    {
                        isBusinessHours = false;
                    }

                    if (enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }



                    if (enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {

                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }

                    if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {

                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }

                    if (!enabled && !isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (!enabled && !isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }

                    //if (!enabled && isBusinessHours && !isEnableBusinessHours && !isEnableNonBusinessHours)
                    //{
                    //    obj.StatusId = (int)StatusEnum.Pending;
                    //    obj.Comment = "Meter send to admin for approval.";
                    //    isVerified = false;
                    //}

                    if (enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && isEnableBusinessHours && !isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }

                    if (enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "Meter send to admin for approval.";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && !isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Active;
                        obj.Comment = "";
                        isVerified = false;
                    }
                    if (!enabled && isBusinessHours && isEnableBusinessHours && isEnableNonBusinessHours)
                    {
                        obj.StatusId = (int)StatusEnum.Pending;
                        obj.Comment = "";
                        isVerified = false;
                    }

                    foreach (var meter in request.Meter)
                    {


                        obj.EFTNo = eftNo;
                        obj.MeterAlias = await GenerateMeterAlias(request.PropertyId, meter.MeterTypeId).ConfigureAwait(false);
                        obj.MeterNumber = meter.MeterNumber;
                        obj.MeterTypeId = meter.MeterTypeId;
                        obj.PropertyId = request.PropertyId;
                        obj.TargetConsumption = meter.TargetConsumption;
                        int id = await _meterRepository.AddMetersFromMeterList(obj).ConfigureAwait(false);

                        eftNo = await GenerateEFTNumber(id).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(eftNo))
                        {
                            eftNocount = await _meterRepository.IsEftNoExist(eftNo).ConfigureAwait(false);
                        }

                        if (eftNocount > 0)
                        {
                            validatorResult.Errors.Add(new FluentValidation.Results.ValidationFailure
                            {
                                PropertyName = nameof(eftNo),
                                ErrorMessage = "EFTNumber already Exist ,it should be unique."
                            });
                        }
                        else
                        {
                            await _meterRepository.UpdateEFTNumberByMeterId(id, eftNo).ConfigureAwait(false);
                        }
                        response.Id = id;
                        response.Message = "Meters added successfully!";

                    }

                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + firstMeter + "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);

                    if (meterResult != null)
                    {
                        var customerAgreementId = "";
                        var mastercustomerAgreementId = "";
                        customerAgreementId = await _propertyRepository.GetCustomerAgreementId(request.PropertyId).ConfigureAwait(false);
                        var meterdata = meterResult.Data;

                        foreach (var item in meterdata)
                        {
                            // Check if the current element contains the customer agreement
                            if (item.CustomerAgreement != null)
                            {
                                if (item.CustomerAgreement.Id != "")
                                {
                                    mastercustomerAgreementId = item.CustomerAgreement.Id.ToString();
                                    if (string.IsNullOrEmpty(customerAgreementId) && !string.IsNullOrEmpty(mastercustomerAgreementId))
                                    {
                                        await _propertyRepository.UpdatePropertyCustomerAgreementId(request.PropertyId, mastercustomerAgreementId);

                                    }
                                }
                            }
                        }

                    }

                }


                if (request.Meter.Count > 0)
                {
                    foreach (var meter in request.Meter)
                    {
                        AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                        {

                            UserID = _workContext.CurrentUserId,
                            Title = "New meter registered",

                            Description = "New meter : " + meter.MeterNumber + "  registered successfully",
                            IsRead = (int)StatusEnum.Sent,
                            NotificationType = (int)NotificationType.Register
                        };
                        await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                    }
                }
                else
                {
                    response.Id = 0;
                    response.Message = "Meters already Exists!";
                }
                if (!validatorResult.IsValid)
                    throw new ValidationException(validatorResult.Errors);
            }
            else
            {
                response.Id = 0;
                response.Message = "Meters already Exists!";
            }
            return response;
        }

        private async Task<string> GenerateEFTNumber(int meterId)
        {

            string strEFTNo = "";
            string strPrecharacterEft = "";
            int eftRandomDigitNumberLength = 0;
            var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
            if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("eftprecharacters")))
            {

                var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("eftprecharacters"));
                if (approvalConfig != null)
                {
                    strPrecharacterEft = approvalConfig.Value;
                }
                var eftRandomDigitNumber = configurations.FirstOrDefault(t => t.Name.Equals("eftReferenceRandomDigitNumberLength"));
                if (eftRandomDigitNumber != null)
                {
                    eftRandomDigitNumberLength = Convert.ToInt32(eftRandomDigitNumber.Value);
                }
            }

            DateTime currentDate = DateTime.Now;
            string monthNumber = currentDate.ToString("MM");
            string yearNumber = currentDate.ToString("yyyy");

            strEFTNo = strPrecharacterEft.ToLower() + "00" + meterId;
            return strEFTNo;
        }


        private async Task<string> GenerateMeterAlias(int propertyId, int meterType)
        {

            string strMeterAlias = "";

            var property = await _propertyRepository.GetPropertyById(propertyId);
            string propertyName = property.Name;
            var UtilityTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);

            var UtilityType = UtilityTypes.ToList().FirstOrDefault(t => t.id.Equals(meterType));
            if (UtilityType != null)
            {
                strMeterAlias = propertyName + " " + UtilityType.name + " Meter";
            }

            return strMeterAlias;
        }

    }
}
