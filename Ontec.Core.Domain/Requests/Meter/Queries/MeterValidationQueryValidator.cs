using FluentValidation;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class MeterValidationQueryValidator:AbstractValidator<MeterValidationQuery>
    {
        public MeterValidationQueryValidator()
        {
            RuleFor(x => x.MeterNumber).IsValidMeterNumber();   
        }
    }
}
