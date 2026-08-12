using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class AddMetersFromMeterListQueryValidator : AbstractValidator<AddMetersFromMeterListQuery>
    {
     
        public AddMetersFromMeterListQueryValidator(IPropertyRepository _propertyRepository
                                                    ,IWorkContext workContext
                                                    ,IMeterRepository _meterRepository
                                                    , IMasterApiConnectService _masterApiConnectService
                                                    , MasterApiSetting _masterApiSetting)
        {
            RuleFor(x => x.PropertyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddMetersFromMeterListQuery.PropertyId).SplitPascalCase(), 1);
            
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isPropertyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);

                if (!isPropertyExist)
                    context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "Property id does not exist.");
                else
                {
                    var property = await _propertyRepository.GetPropertyById(model.PropertyId);
                    if (property != null && property.OwnerId != workContext.CurrentUserId)
                    {
                        context.AddFailure(nameof(workContext.CurrentUserId), "Only owner can add meter for respective property.");
                    }
                    foreach (var meter in model.Meter)
                    {

                        var isMeterTypeExist = await _meterRepository.IsMeterTypeIdExist(meter.MeterTypeId).ConfigureAwait(false);
                        if (!isMeterTypeExist)
                            context.AddFailure(nameof(meter.MeterTypeId), "Meter type id does not exist");

                        var UtilityTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);
                       
                        if (UtilityTypes != null)
                        {
                            var UtilityType = UtilityTypes.ToList().FirstOrDefault(t => t.id.Equals(meter.MeterTypeId));
                            if (UtilityType != null)
                            {
                                //if (meter.TargetConsumption < Convert.ToInt32(UtilityType.MinDailyTarget))
                                //{
                                //    context.AddFailure(nameof(meter.TargetConsumption), string.Format(CommonConstants.DailyTargetMinLimit + " for meter : " + meter.MeterNumber));
                                //}
                                //if (meter.TargetConsumption > Convert.ToInt32(UtilityType.MaxDailyTarget))
                                //{
                                //    context.AddFailure(nameof(meter.TargetConsumption), string.Format(CommonConstants.DailyTargetMaxLimit+ " for meter : " + meter.MeterNumber));
                                //}

                                if (meter.TargetConsumption < Convert.ToInt32(UtilityType.MinDailyTarget) || meter.TargetConsumption > Convert.ToInt32(UtilityType.MaxDailyTarget))
                                {
                                    context.AddFailure(nameof(AddUpdateMeterQuery.DailyTargetConsumption), "Daily target must be from " + UtilityType.MinDailyTarget + " " + UtilityType.UnitOfMeasure + " to " + UtilityType.MaxDailyTarget + " " + UtilityType.UnitOfMeasure + " for meter : " + meter.MeterNumber);
                                }
                            }
                        }

                    }

                    var existingMeterList = await _meterRepository.GetAllMetersByPropertyId(model.PropertyId).ConfigureAwait(false);
                    var firstMeter = existingMeterList.Select(a=>a.MeterNumber).FirstOrDefault();
                    var newMeters =model.Meter.ToList();
                    var firsNewtMeter = newMeters.Select(a=>a.MeterNumber).FirstOrDefault();
                    var existingmastercustomerAgreementId="";
                    var newmastercustomerAgreementId = "";
                    if (existingMeterList.ToList().Count> 0 && firstMeter!=null)
                    {
                        var existingmeterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + firstMeter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                        var existingmeterResult = await _masterApiConnectService.GetMeter(existingmeterUrl).ConfigureAwait(false);
                        if(existingmeterResult != null && existingmeterResult.Data.Count() > 0)
                        {
                             existingmastercustomerAgreementId = existingmeterResult.Data[0].CustomerAgreement.Id.ToString();
                        }
                        var newmeterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + firsNewtMeter.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                        var newmeterResult = await _masterApiConnectService.GetMeter(newmeterUrl).ConfigureAwait(false);
                        if (newmeterResult != null && newmeterResult.Data.Count() > 0)
                        {
                             newmastercustomerAgreementId = newmeterResult.Data[0].CustomerAgreement.Id.ToString();
                        }
                        var customerAgreementId =await _propertyRepository.GetCustomerAgreementId(model.PropertyId).ConfigureAwait(false);
                       
                        if ((existingmastercustomerAgreementId != null ||existingmastercustomerAgreementId !="")&& newmastercustomerAgreementId != null && customerAgreementId!="")
                        {
                            if (existingmastercustomerAgreementId != newmastercustomerAgreementId || (customerAgreementId!=newmastercustomerAgreementId))
                            {
                                context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                            }
                        }

                    }
                }

            });
        }
    }
}
