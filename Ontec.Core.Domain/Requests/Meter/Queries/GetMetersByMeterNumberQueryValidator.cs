using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Requests.Meter.Command;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class GetMetersByMeterNumberQueryValidator : AbstractValidator<GetMetersByMeterNumberQuery>
    {
        public GetMetersByMeterNumberQueryValidator(IMasterApiConnectService _masterApiConnectService
                                    , MasterApiSetting _masterApiSetting, IMeterRepository meterRepository, IPropertyRepository _propertyRepository)
        {
            RuleFor(x => x.MeterNumber).NotNullAndEmptyAsync().IsValidMeterNumber();
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                int existCount = await meterRepository.IsMeterNumberExist(model.MeterNumber).ConfigureAwait(false);

                var idDeactivate = await meterRepository.IsMeterNumberDeactivated(model.MeterNumber).ConfigureAwait(false);
                if (idDeactivate)
                {
                    context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "Meter number is De activate.");

                }
                var isPropertyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);

                if (!isPropertyExist)
                    context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "Property id does not exist.");

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + model.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                var custAgreeID = "";
                if (meterResult == null)
                {
                    context.AddFailure(nameof(VerifyMeterQuery.MeterNumber), "Meter number not exists");
                }
                if (meterResult != null)
                {
                    custAgreeID = meterResult.Data[0].CustomerAgreement.Id.ToString();
                    var customerAgreementId = await _propertyRepository.GetCustomerAgreementId(model.PropertyId).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(customerAgreementId) && customerAgreementId != custAgreeID)
                    {
                        context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                    }

                }
                if (existCount > 0)
                {
                    var meter = await meterRepository.GetMeterDetails(model.MeterNumber).ConfigureAwait(false);
                    if (meter != null && meter.PropertyId != model.PropertyId)
                    {
                        context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "Meter is already registered for another property, you can't import meters for this property");
                    }
                    if (meterResult != null && meter != null)
                    {
                        custAgreeID = meterResult.Data[0].CustomerAgreement.Id.ToString();
                        var propertyCust = await _propertyRepository.IsCustomerAgreementIdExist(custAgreeID).ConfigureAwait(false);
                        var currentPropertycustomerAgreementId = await _propertyRepository.GetCustomerAgreementId(model.PropertyId).ConfigureAwait(false);
                        if (propertyCust != null && propertyCust.CustomerAgreementIdCount > 0)
                        {
                            if (propertyCust.PropertyId != model.PropertyId)
                            {
                                context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                            }
                        }
                        if (!string.IsNullOrEmpty(currentPropertycustomerAgreementId) && custAgreeID != null && custAgreeID != currentPropertycustomerAgreementId)
                        {
                            context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                        }
                    }
                }
                else
                {
                    var existingMeterList = await meterRepository.GetAllMetersByPropertyId(model.PropertyId).ConfigureAwait(false);
                    var firstMeter = existingMeterList.FirstOrDefault();
                    var newMeters = model.MeterNumber.ToList();
                    var existingmastercustomerAgreementId = "";
                    var newmastercustomerAgreementId = "";
                    if (existingMeterList.ToList().Count > 0 && firstMeter != null)
                    {
                        var existingmeterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + firstMeter.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                        var existingmeterResult = await _masterApiConnectService.GetMeter(existingmeterUrl).ConfigureAwait(false);
                        if (existingmeterResult != null)
                        {
                            existingmastercustomerAgreementId = existingmeterResult.Data[0].CustomerAgreement.Id.ToString();
                        }
                        var newmeterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + model.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                        var newmeterResult = await _masterApiConnectService.GetMeter(newmeterUrl).ConfigureAwait(false);
                        if (newmeterResult != null)
                        {
                            newmastercustomerAgreementId = newmeterResult.Data[0].CustomerAgreement.Id.ToString();
                        }
                        var customerAgreementId = await _propertyRepository.GetCustomerAgreementId(model.PropertyId).ConfigureAwait(false);

                        if (newmastercustomerAgreementId != null && existingmastercustomerAgreementId != "")
                        {
                            if (existingmastercustomerAgreementId != newmastercustomerAgreementId)
                            {
                                context.AddFailure(nameof(AddMetersFromMeterListQuery.PropertyId), "This meter number is not associated with this property.");
                            }
                            if (!string.IsNullOrEmpty(customerAgreementId) && customerAgreementId != newmastercustomerAgreementId)
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
