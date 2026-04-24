using System.Globalization;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;


namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class AddUpdateMeterQueryValidator : AbstractValidator<AddUpdateMeterQuery>
    {

        public AddUpdateMeterQueryValidator(IPropertyRepository _propertyRepository
                                            , IMeterRepository _meterRepository
                                            , IDocumentRepository _documentRepository
                                            , IWorkContext workContext
                                            , IMasterApiConnectService _masterApiConnectService
                                            , MasterApiSetting _masterApiSetting
                                            , IConfigurationRepository _configurationRepository)
        {

            RuleFor(x => x.PropertyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddUpdateMeterQuery.PropertyId).SplitPascalCase(), 1);
            RuleFor(x => x.MeterTypeId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddUpdateMeterQuery.MeterTypeId).SplitPascalCase(), 1);
            RuleFor(x => x.MeterNumber).IsValidMeterNumber().NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsyncMeterNumber(nameof(AddUpdateMeterQuery.MeterNumber).SplitPascalCase(), 15);
            RuleFor(x => x.DailyTargetConsumption).NotNull().IsValidTargetConsumption();
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                
                    var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + model.MeterNumber.ToUpper()+ "&paging=(limit)(5)(offset)(0)";
                    var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                   
                    if (meterResult == null)
                    {
                        //context.AddFailure(nameof(AddUpdateMeterQuery.MeterNumber), CommonConstants.MeterNotExist);
                    }
                    else
                    {
                        try
                        {
                             var meterDataId = meterResult.Data[0].Meter.Id;
                            
                                var customerAgreementId = "";
                                var mastercustomerAgreementId = "";
                                var meterdata = meterResult.Data;
                                customerAgreementId = await _propertyRepository.GetCustomerAgreementId(model.PropertyId).ConfigureAwait(false);
                                foreach (var item in meterdata)
                                {
                                    // Check if the current element contains the customer agreement
                                    if (item.CustomerAgreement != null)
                                    {
                                        if (item.CustomerAgreement.Id != "")
                                        {
                                            mastercustomerAgreementId = item.CustomerAgreement.Id.ToString();
                                        }
                                        else
                                        {
                                            context.AddFailure(nameof(AddUpdateMeterQuery.MeterNumber), "Meter number is invalid");
                                        }

                                        if (mastercustomerAgreementId != "" && customerAgreementId != "" && customerAgreementId != "")
                                        {
                                            if (mastercustomerAgreementId != customerAgreementId)
                                            {
                                                context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                                            }
                                        }
                                    }
                                }
                        }
                        catch (Exception ex)
                        {
                            context.AddFailure(nameof(AddUpdateMeterQuery.MeterNumber), CommonConstants.MeterNotExist);
                        }
                    }
                

                var isPropertyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);

                if (!isPropertyExist)
                    context.AddFailure(nameof(AddUpdateMeterQuery.PropertyId), "Property id does not exist");
                else
                {
                    var property = await _propertyRepository.GetPropertyById(model.PropertyId);
                    if (property != null && property.OwnerId != workContext.CurrentUserId)
                    {
                        context.AddFailure(nameof(AddUpdateMeterQuery.MeterTypeId), "Only owner can add meter for respective property.");
                    }
                }
                var isMeterTypeExist = await _meterRepository.IsMeterTypeIdExist(model.MeterTypeId).ConfigureAwait(false);
                if (!isMeterTypeExist)
                    context.AddFailure(nameof(AddUpdateMeterQuery.MeterTypeId), "Meter type id does not exist");

                //var meterId = await _meterRepository.IsMeterNumberExist(model.MeterNumber, model.Id).ConfigureAwait(false);
                var meterId=await _meterRepository.IsActivePendingRejectedMeterNumberExist(model.MeterNumber, model.Id).ConfigureAwait(false);
                var status = "";
                if (meterId > 0)
                {
                    var meter = await _meterRepository.GetMeterById(meterId).ConfigureAwait(false);
                    status = meter.Status;
                    if (status == "Active")
                        context.AddFailure(nameof(AddUpdateMeterQuery.MeterNumber), "Meter number already registered");
                    else
                        context.AddFailure(nameof(AddUpdateMeterQuery.MeterNumber), "Meter number is " + status);

                }


                double dailyTarget =(model.DailyTargetConsumption);
                var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);
                var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                if (meterTypes != null && meterTypes.Any(t => t.id.Equals(model.MeterTypeId)))
                {
                    var minDailyTarget =meterTypes.Where(t => t.id.Equals(model.MeterTypeId)).Select(t=>t.MinDailyTarget).FirstOrDefault();
                    var maxDailyTarget = meterTypes.Where(t => t.id.Equals(model.MeterTypeId)).Select(t => t.MaxDailyTarget).FirstOrDefault();
                    var unitOfMeasure = meterTypes.Where(t => t.id.Equals(model.MeterTypeId)).Select(t => t.UnitOfMeasure).FirstOrDefault();
                    if (Convert.ToDouble(minDailyTarget) != 0 && Convert.ToDouble(maxDailyTarget) != 0)
                    {
                        if (dailyTarget < Convert.ToDouble(minDailyTarget) || dailyTarget > Convert.ToDouble(maxDailyTarget))
                        {
                            context.AddFailure(nameof(AddUpdateMeterQuery.DailyTargetConsumption),"Daily target must be from "+ minDailyTarget + " " + unitOfMeasure + " to " + maxDailyTarget +" "+unitOfMeasure);
                        }

                        //if (dailyTarget > Convert.ToDouble(maxDailyTarget))
                        //{
                        //    context.AddFailure(nameof(AddUpdateMeterQuery.DailyTargetConsumption), string.Format(CommonConstants.DailyTargetMaxLimit + ". Maximum limit is " + maxDailyTarget+ " "+ unitOfMeasure+"."));
                        //}
                    }
                }


                //if (model.Id == 0 && model.ContractProofDocument == null)
                //{
                //    context.AddFailure(nameof(AddUpdateMeterQuery.ContractProofDocument), string.Format(CommonConstants.IsRequired, nameof(AddUpdateMeterQuery.ContractProofDocument)));
                //}
                if (model.ContractProofDocument != null)
                {
                    int defaultFileSize = 5;
                    if (configurations != null)
                    {
                        if (configurations.Any())
                        {
                            var documentSize = configurations.FirstOrDefault(t => t.Name.Equals("ContractDocumentSizeInMB"));
                            if (documentSize != null)
                                defaultFileSize = int.Parse(documentSize.Value);
                        }
                    }
                    var isValid = true;
                    string[] supportedTypes = [".pdf"];
                    var fileExt = System.IO.Path.GetExtension(model.ContractProofDocument.FileName);
                    if (!supportedTypes.Contains(fileExt.ToLower()))
                        isValid = false;
                    if (model.ContractProofDocument.Length > (defaultFileSize * 1024 * 1024))//50 MB 
                        isValid = false;
                    if (!isValid)
                        context.AddFailure(nameof(AddUpdateMeterQuery.ContractProofDocument), "Allowed file formats (pdf) with " + defaultFileSize + "MB");

                    if (!string.IsNullOrEmpty(model.ContractEndDate))
                    {
                        if (DateTime.TryParseExact(model.ContractEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime contractDate))
                        {
                            DateTime today = DateTime.UtcNow.Date;
                            bool isValidDate = contractDate >= today;
                            if (!isValidDate)
                                context.AddFailure(nameof(AddUpdateMeterQuery.ContractEndDate), "Invalid date format, date should be greater than today.");
                        }
                        else
                        {
                            context.AddFailure(nameof(AddUpdateMeterQuery.ContractEndDate), "Invalid date format, Please provide the date in yyyy-MM-dd format.");
                        }
                    }
                }
            });
        }
    }
}
