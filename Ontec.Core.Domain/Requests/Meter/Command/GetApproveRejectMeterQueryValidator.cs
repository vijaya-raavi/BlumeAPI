using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class GetApproveRejectMeterQueryValidator: AbstractValidator<GetApproveRejectMeterQuery>
    {
        public GetApproveRejectMeterQueryValidator(IMeterRepository _meterRepository)
        {
            RuleFor(x => x.MeterID).NotNull().GreaterThanOrEqualTo(1);
            
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
               var isValid=await _meterRepository.IsMeterIdExist(model.MeterID);
                
                if(!isValid)
                {
                    context.AddFailure(nameof(GetApproveRejectMeterQuery.MeterID),string.Format(CommonConstants.NotExist,nameof(GetApproveRejectMeterQuery.MeterID)));
                }
                var isExist = await _meterRepository.IsPendingMeterIdExist(model.MeterID).ConfigureAwait(false);
                if (isValid && !isExist)
                {
                    context.AddFailure(nameof(GetApproveRejectMeterQuery.MeterID), string.Format(CommonConstants.MeterIsNotPending, nameof(GetApproveRejectMeterQuery.MeterID)));
                }
                if (!model.IsApproved && string.IsNullOrEmpty(model.Comments))
                {
                    context.AddFailure(nameof(GetApproveRejectMeterQuery.Comments), string.Format(CommonConstants.AreRequired, nameof(GetApproveRejectMeterQuery.Comments)));
                }
            });

        }
    }
}
