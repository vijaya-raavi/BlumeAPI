using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.User.Commands;

public class BulkUploadRequestUsersCommandValidator : AbstractValidator<BulkUploadRequestUsersCommand>
{
    public BulkUploadRequestUsersCommandValidator(IUserRepository _userRepository, IPropertyRepository _propertyRepository, IMeterRepository _meterRepository,
        ICompanyRepository _companyRepository, IMasterApiConnectService _masterApiConnectService, MasterApiSetting _masterApiSetting,
        IConfigurationRepository _configurationRepository, IWorkContext workContext, IUserRoleRepository _userRoleRepository, ICommunicationRepository _communicationRepository,IDocumentRepository _documentRepository)
    {

        RuleFor(x => x.UploadBulkUsers)
            .NotNull().WithMessage("Upload bulk users list is required.")
            .Must(u => u != null && u.Any()).WithMessage("At least one user is required.");



        RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
        {
            var duplicateEmails = model.UploadBulkUsers
                                .GroupBy(x => x.Email.ToLower().Trim())
                                .Where(g => g.Select(x => x.Mobile.Trim()).Distinct().Count() > 1)
                                .Select(g => g.Key);

            if (duplicateEmails.Any())
                throw new ValidationException($"Same email with different mobiles: {string.Join(", ", duplicateEmails)}");

            var duplicateMobiles = model.UploadBulkUsers
                .GroupBy(x => x.Mobile.Trim())
                .Where(g => g.Select(x => x.Email.ToLower().Trim()).Distinct().Count() > 1)
                .Select(g => g.Key);

            if (duplicateMobiles.Any())
                context.AddFailure($"Same mobile with different emails: {string.Join(", ", duplicateMobiles)}");

            var duplicateMeters = model.UploadBulkUsers
                                .Where(x => x.UserProperties != null)
                                .SelectMany(x => x.UserProperties)
                                .Where(p => p.Meters != null)
                                .SelectMany(p => p.Meters)
                                .GroupBy(m => m.MeterNumber.Trim())
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key);

            if (duplicateMeters.Any())
                context.AddFailure($"Duplicate meter numbers found: {string.Join(", ", duplicateMeters)}");
        });
        RuleForEach(x => x.UploadBulkUsers).CustomAsync(async (user, context, cancellationToken) =>
        {
            // ── 1. User Validations ──────────────────────────────────────
            await ValidateUser(user, context, _userRepository, _userRoleRepository, _communicationRepository, _documentRepository);

            // ── 2. Property Validations ──────────────────────────────────
            if (user.UserProperties?.Any() == true)
            {
                foreach (var property in user.UserProperties)
                {
                    await ValidateProperty(property, user.CompanyId, context, _companyRepository, _propertyRepository, workContext, _configurationRepository);

                    // ── 3. Meter Validations ─────────────────────────────
                    if (property.Meters?.Any() == true)
                    {
                        foreach (var meter in property.Meters)
                        {
                            await ValidateMeter(meter, context, _meterRepository, _propertyRepository, _masterApiConnectService, _masterApiSetting, _configurationRepository, workContext);
                        }
                    }
                }
            }
        });
    }

    // ── User Validation ──────────────────────────────────────────────────
    private async Task ValidateUser(BulkUsers user, ValidationContext<BulkUploadRequestUsersCommand> context, IUserRepository userRepository, IUserRoleRepository _userRoleRepository, ICommunicationRepository _communicationRepository,IDocumentRepository documentRepository)
    {

        if (user.Title == 0)
        {
            context.AddFailure($"Title [{user.Email}]", "Title is required.");
        }
        int communicationIdCount = 0;
        if (user.CommunicationTypesIds != null)
        {
            communicationIdCount = user.CommunicationTypesIds.Count();
        }
        if (communicationIdCount == 0)
        {
            context.AddFailure(nameof(UpdateUserCommand.CommunicationTypesIds), "Communication type is required");
        }
        else
        {
            var communications = await _communicationRepository.GetCommunicationMasters().ConfigureAwait(false);
            var commIds = communications.Select(t => t.Id);

            var noDbRecords = user.CommunicationTypesIds.Where(m => !commIds.Contains(m));
            if (noDbRecords.Any())
            {
                context.AddFailure($"CommunicationTypesIds[{user.CommunicationTypesIds}]", "Invalid communication type ids.");

            }
        }
        if (string.IsNullOrEmpty(user.Email))
        {
            context.AddFailure($"Email[{user.Email}]", "Email is required.");
        }
        else
        {
            if (!Regex.IsMatch(user.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z-0-9._]+\.[a-zA-Z.]{2,}$"))
            {
                context.AddFailure($"Email[{user.Email}]", "Invalid email .");
            }

        }
        if (string.IsNullOrEmpty(user.Mobile))
        {
            context.AddFailure($"Mobile[{user.Mobile}]", "Mobile is required.");
        }
        else
        {
            if (!Regex.IsMatch(user.Mobile, "^[0-9]{10}$"))
            {
                context.AddFailure($"Mobile[{user.Mobile}]", "Invalid mobile number.");
            }
        }
        // Check email exists
        var id = await userRepository.IsEmailExist(user.Email, user.CompanyId, false).ConfigureAwait(false);
        if (id != 0)
            context.AddFailure($"Email[{user.Email}]", "Email already exists.");

        // Check mobile exists
        id = await userRepository.IsMobileExist(user.Mobile, user.CompanyId, false).ConfigureAwait(false);
        if (id != 0)
            context.AddFailure($"Mobile[{user.Mobile}]", "Mobile number already registered.");

        // Check temp user email/mobile mismatch
        var userIdByEmail = await userRepository.IsEmailInTempUserExist(user.Email, user.CompanyId).ConfigureAwait(false);
        var userIdByMobile = await userRepository.IsMobileInTempUserExist(user.Mobile, user.CompanyId).ConfigureAwait(false);

        if (userIdByEmail == 0 && userIdByMobile > 0)
            context.AddFailure($"Mobile[{user.Mobile}]", "Mobile already registered with other email id.");
        else if (userIdByEmail > 0 && userIdByMobile == 0)
            context.AddFailure($"Email[{user.Email}]", "Email already registered with other mobile number.");


        if (user.Title > 0)
        {
            var titleList = await _userRoleRepository.GetTitleMasters();
            if (!titleList.Any(t => t.Id.Equals(user.Title)))
            {
                context.AddFailure($"Title [{user.Email}]", "Invalid title");
            }
        }
        if (user.UserDocument != null)
        {
            var docTypes = await documentRepository.GetDocumentTypeMasters().ConfigureAwait(false);
            var docTypesName = docTypes.Select(s => s.Name).ToList();

            bool isExist = docTypesName.Contains(user.UserDocument.DocumentType);
            if (!isExist)
            {
                context.AddFailure(nameof(UpdateUserCommand.CommunicationTypesIds), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.CommunicationTypesIds).SplitPascalCase()));
            }
            int docTypesId = docTypes.ToList().Where(s => s.Name.Equals(user.UserDocument.DocumentType)).Select(t => t.Id).FirstOrDefault();

            if (docTypesId == (int)DocumentTypeEnum.SouthAfricanID)
            {

                if (!string.IsNullOrEmpty(user.UserDocument.DocNumber))
                {
                    string doc = user.UserDocument.DocNumber;
                    string mmdd = doc.Substring(2, 4); // Extract MMDD

                    if (user.Title == (int)Title.Mrs || user.Title == (int)Title.Ms)
                    {
                        if (!DateTime.TryParseExact(mmdd, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            context.AddFailure($"DocNumber [{user.Email}]", "Invalid document number");
                        }
                        if (!Regex.IsMatch(user.UserDocument.DocNumber, "^[0-9]{2}[0-9]{2}[0-9]{2}[0-4][0-9]{3}[0-1]{1}[8]{1}[0-9]{1}$"))
                        {
                            context.AddFailure($"DocNumber [{user.Email}]", "Invalid document number");
                        }
                    }
                    if (user.Title == (int)Title.Mr)
                    {

                        if (!DateTime.TryParseExact(mmdd, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        {
                            context.AddFailure($"DocNumber [{user.Email}]", "Invalid document number");
                        }
                        if (!Regex.IsMatch(user.UserDocument.DocNumber, "^[0-9]{2}[0-9]{2}[0-9]{2}[5-9][0-9]{3}[0-1]{1}[8]{1}[0-9]{1}$"))
                        {
                            context.AddFailure($"DocNumber [{user.Email}]", "Invalid document number");
                        }
                    }
                }
            }
        }
    }

    // ── Property Validation ──────────────────────────────────────────────
    private async Task ValidateProperty(
        PropertyRequestDto property,
        int companyId,
        ValidationContext<BulkUploadRequestUsersCommand> context,
        ICompanyRepository companyRepository,
        IPropertyRepository propertyRepository,
        IWorkContext workContext,
        IConfigurationRepository _configurationRepository)
    {
        bool isEstateEnable = false;
        // Company exists
        var isValid = await companyRepository.IsCompanyExist(companyId).ConfigureAwait(false);
        if (!isValid)
        {
            context.AddFailure($"Property - Company id", "Company id does not exist.");
            return;
        }
        var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
        if (configurations?.Any() == true)
        {
            var estateEnable = configurations.FirstOrDefault(t => t.Name.Equals("isestateenable"));
            if (estateEnable != null)
            {
                string value = (estateEnable.Value);
                if (value != null)
                {
                    if (value == "1")
                    {
                        isEstateEnable = true;
                    }
                }
            }
        }
        //Estate validation
        if (isEstateEnable && property.EstateId == 0)
            context.AddFailure($"Estate id", "Estate id does not exist.");
    }

    // ── Meter Validation ─────────────────────────────────────────────────
    private async Task ValidateMeter(
        PropertyMeterRequestDto meter,
        ValidationContext<BulkUploadRequestUsersCommand> context,
        IMeterRepository meterRepository,
        IPropertyRepository propertyRepository,
        IMasterApiConnectService masterApiConnectService,
        MasterApiSetting masterApiSetting,
        IConfigurationRepository configurationRepository,
        IWorkContext workContext)
    {


        // ── Master API Meter Validation ──────────────────────────────────
        var meterUrl = masterApiSetting.BaseUrl + masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
        var meterResult = await masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
        if (meterResult != null && meterResult.Data.Count() == 0)
        {
            context.AddFailure($"Meter Number[{meter.MeterNumber}]", "Meter number is not exist.");
        }
        var meterTypes = await meterRepository.GetMeterType().ConfigureAwait(false);
        if (meterResult != null && meterResult.Data.Count() > 0)
        {
            try
            {
                var meterdata = meterResult.Data;
                var mastercustomerAgreementId = "";
                var customerAgreementId = ""; // propertyId not available here — pass if needed

                foreach (var item in meterdata)
                {
                    if (item.CustomerAgreement != null)
                    {
                        if (!string.IsNullOrEmpty(item.CustomerAgreement.Id))
                            mastercustomerAgreementId = item.CustomerAgreement.Id.ToString();
                        else
                            context.AddFailure($"Meter [{meter.MeterNumber}] - MeterNumber", "Meter number is invalid.");

                        if (!string.IsNullOrEmpty(mastercustomerAgreementId) && !string.IsNullOrEmpty(customerAgreementId))
                        {
                            if (mastercustomerAgreementId != customerAgreementId)
                                context.AddFailure($"Meter - Property id [{meter.MeterNumber}]", "This meter number is not associated with this property.");
                        }


                    }
                    //string utiltyType = item.Meter.Model.ServiceResource;
                    //string utilityName = item.Meter.Model.Name;
                    //if (utilityName.Contains("SMART HOT_WATER"))
                    //{
                    //    utiltyType = "HOT WATER";
                    //}
                    //var utility = meterTypes.FirstOrDefault(t => t.name.ToUpper().Contains(utiltyType));
                    //if (utility != null && meter.MeterTypeId != utility.id)
                    //{
                    //    context.AddFailure($"Meter - Meter Number [{meter.MeterNumber}]", "Invalid utility type .");
                    //}
                }


               
            }
            catch (Exception)
            {
                context.AddFailure($"Meter - Meter Number", "Meter does not exist.");
            }
        }

        // ── Meter Type Exists ────────────────────────────────────────────
        var isMeterTypeExist = await meterRepository.IsMeterTypeIdExist(meter.MeterTypeId).ConfigureAwait(false);
        if (!isMeterTypeExist)
            context.AddFailure($"Meter - Utility type [{meter.MeterNumber}]", "Utility type  does not exist.");

        // ── Duplicate Meter Number ───────────────────────────────────────
        var existingMeterId = await meterRepository.IsActivePendingRejectedMeterNumberExist(meter.MeterNumber, 0).ConfigureAwait(false);
        if (existingMeterId > 0)
        {
            var existingMeter = await meterRepository.GetMeterById(existingMeterId).ConfigureAwait(false);
            var status = existingMeter.Status;
            context.AddFailure($"Meter  - MeterNumber[{meter.MeterNumber}]", status == "Active" ? "Meter number already registered." : $"Meter number is {status}.");
        }

        // ── Daily Target Consumption ─────────────────────────────────────

        if (meterTypes != null && meterTypes.Any(t => t.id.Equals(meter.MeterTypeId)))
        {
            var minDailyTarget = meterTypes.Where(t => t.id.Equals(meter.MeterTypeId)).Select(t => t.MinDailyTarget).FirstOrDefault();
            var maxDailyTarget = meterTypes.Where(t => t.id.Equals(meter.MeterTypeId)).Select(t => t.MaxDailyTarget).FirstOrDefault();
            var unitOfMeasure = meterTypes.Where(t => t.id.Equals(meter.MeterTypeId)).Select(t => t.UnitOfMeasure).FirstOrDefault();
            if (meter.TargetConsumption == 0)
            {
                context.AddFailure($"TargetConsumption[{meter.MeterNumber}{meter.TargetConsumption}]", "Daily target consumption is required.");
            }
            else if (!Regex.IsMatch(meter.TargetConsumption.ToString(), @"^\d+(\.\d+)?$"))
            {
                context.AddFailure($"TargetConsumption[{meter.MeterNumber}{meter.TargetConsumption}]", " No decimal numbers are allowed");
            }
            else if (Convert.ToDouble(minDailyTarget) != 0 && Convert.ToDouble(maxDailyTarget) != 0)
            {
                if (meter.TargetConsumption < Convert.ToDouble(minDailyTarget) || meter.TargetConsumption > Convert.ToDouble(maxDailyTarget))
                {
                    context.AddFailure($"Meter - DailyTargetConsumption[{meter.MeterNumber}{meter.TargetConsumption}]",
                        $"Daily target must be from {minDailyTarget} {unitOfMeasure} to {maxDailyTarget} {unitOfMeasure}.");
                }
            }
        }

        // ── Contract Document Validation ─────────────────────────────────
        if (meter.ContractDocumentUrl != null)
        {
            var configurations = await configurationRepository.GetConfigurations().ConfigureAwait(false);
            int defaultFileSize = 5;

            if (configurations?.Any() == true)
            {
                var documentSize = configurations.FirstOrDefault(t => t.Name.Equals("ContractDocumentSizeInMB"));
                if (documentSize != null)
                    defaultFileSize = int.Parse(documentSize.Value);
            }

            string[] supportedTypes = [".pdf"];
            var fileExt = Path.GetExtension(meter.ContractDocumentUrl.FileName);

            if (!supportedTypes.Contains(fileExt.ToLower()) || meter.ContractDocumentUrl.Length > (defaultFileSize * 1024 * 1024))
            {
                context.AddFailure($"Meter  - ContractDocument[{meter.MeterNumber}{meter.ContractDocumentUrl.FileName}]",
                    $"Allowed file formats (pdf) with {defaultFileSize}MB.");
            }

            // ── Contract End Date ────────────────────────────────────────
            if (!string.IsNullOrEmpty(meter.ContractEndDate))
            {
                if (DateTime.TryParseExact(meter.ContractEndDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime contractDate))
                {
                    if (contractDate < DateTime.UtcNow.Date)
                        context.AddFailure($"Meter- ContractEndDate[{meter.MeterNumber}]", "Date should be greater than today.");
                }
                else
                {
                    context.AddFailure($"Meter - ContractEndDate [{meter.MeterNumber}]", "Invalid date format, Please provide the date in yyyy-MM-dd format.");
                }
            }
        }
    }
}