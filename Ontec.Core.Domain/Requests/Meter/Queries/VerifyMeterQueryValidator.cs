using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class VerifyMeterQueryValidator : AbstractValidator<VerifyMeterQuery>
    {
        public VerifyMeterQueryValidator(IMeterRepository _meterRepository
                                        , IPropertyRepository _propertyRepository
                                        , IMasterApiConnectService _masterApiConnectService
                                    , MasterApiSetting _masterApiSetting)
        {
            RuleFor(x => x.MeterNumber).NotNullAndEmptyAsync().IsValidMeterNumber();
            RuleFor(x => x.PropertyId).NotNull().GreaterThanOrEqualToAsync(nameof(VerifyMeterQuery.PropertyId).SplitPascalCase(), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var meterId = await _meterRepository.IsActivePendingRejectedMeterNumberExist(model.MeterNumber, 0).ConfigureAwait(false);

                var status = "";
                if (meterId > 0)
                {
                    var meter = await _meterRepository.GetMeterById(meterId).ConfigureAwait(false);
                    status = meter.Status;
                    if(status=="Active")
                    context.AddFailure(nameof(VerifyMeterQuery.MeterNumber), "Meter number already registered.");
                    else
                     context.AddFailure(nameof(VerifyMeterQuery.MeterNumber), "Meter number is " + status);
                    
                }
                

                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(VerifyMeterQuery.PropertyId), "Property id does not exist.");

                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + model.MeterNumber.ToUpper() + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                if (meterResult == null)
                {
                    context.AddFailure(nameof(VerifyMeterQuery.MeterNumber), CommonConstants.MeterNotExist);
                }
                else
                {
                    try
                    {
                        var meterDataId = meterResult.Data[0].Meter.Id;
                       // var notificationEmail = meterResult.Data[0].CustomerAccount.NotificationEmail;
                       // var notificationPhone = meterResult.Data[0].CustomerAccount.NotificationPhone;
                       // var propertyOnwer = await _propertyRepository.GetOwnerByPropertyId(model.PropertyId).ConfigureAwait(false);
                       // if (propertyOnwer.Email != notificationEmail)
                       // {
                       //     context.AddFailure(nameof(VerifyMeterQuery.PropertyId), "Invalid meter number for this property.");
                       // }
                       // if (propertyOnwer.Mobile != notificationPhone)
                       // {
                       //     context.AddFailure(nameof(VerifyMeterQuery.PropertyId), "Invalid meter number for this property.");
                       // }
                    }
                    catch (Exception ex)
                    {
                        context.AddFailure(nameof(VerifyMeterQuery.MeterNumber), CommonConstants.MeterNotExist);
                    }
                }
            });
        }
    }
}
