using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class GetTransactionSummaryQueryValidator : AbstractValidator<GetTransactionSummaryQuery>
    {
        public GetTransactionSummaryQueryValidator(IPropertyRepository _propertyRepository)
        {
                RuleFor(m => m.PropertyId).NotNull();
                RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
                {
                    var isPropertyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                    if (!isPropertyExist)
                    {
                        context.AddFailure(nameof(GetTransactionSummaryQuery.PropertyId), string.Format(CommonConstants.NotExist, nameof(GetTransactionSummaryQuery.PropertyId)));

                    }

                    //var isMeterIdExist = await _meterRepository.IsMeterIdExist(model.MeterId).ConfigureAwait(false);
                    //if (!isMeterIdExist)
                    //{
                    //    context.AddFailure(nameof(GetTransactionSummaryQuery.MeterId), string.Format(CommonConstants.NotExist, nameof(GetTransactionSummaryQuery.MeterId)));
                    //}
                    //var property = await _propertyRepository.GetPropertyById(model.PropertyId).ConfigureAwait(false);
                    //if (property != null)
                    //{
                    //    if (property.StatusId == (int)StatusEnum.Inactive)
                    //    {
                    //        context.AddFailure(nameof(GetTransactionSummaryQuery.PropertyId), string.Format(CommonConstants.NotActive, nameof(GetTransactionSummaryQuery.PropertyId)));
                    //    }
                    //}

                    //var isMeterActive = await _meterRepository.IsMeterNumberActive(model.MeterNumber).ConfigureAwait(false);
                    //if (!isMeterActive)
                    //{
                    //    context.AddFailure(nameof(GetTransactionSummaryQuery.MeterNumber), string.Format(CommonConstants.NotActive, nameof(GetTransactionSummaryQuery.MeterNumber)));
                    //}

                    if (model.TransactionCycleId == (int)TransactionPeriod.Custom)
                    {
                        DateTime fromdate = model.FromDate.Date;
                        DateTime todate = model.Todate.Date;
                        TimeSpan objTimeSpan = fromdate - todate;
                        double Days = Convert.ToDouble(objTimeSpan.TotalDays);

                        if (fromdate > todate)
                        {
                            context.AddFailure(nameof(GetTransactionSummaryQuery.FromDate), string.Format(CommonConstants.InValidFromDate, nameof(GetTransactionSummaryQuery.FromDate)));
                        }
                        if (todate < fromdate)
                        {
                            context.AddFailure(nameof(GetTransactionSummaryQuery.Todate), string.Format(CommonConstants.InValidToDate, nameof(GetTransactionSummaryQuery.Todate)));
                        }
                        if (Days > 90)
                        {
                            context.AddFailure(nameof(GetTransactionSummaryQuery.FromDate), string.Format(CommonConstants.InValidPeriod, nameof(GetTransactionSummaryQuery.FromDate)));
                            context.AddFailure(nameof(GetTransactionSummaryQuery.Todate), string.Format(CommonConstants.InValidPeriod, nameof(GetTransactionSummaryQuery.Todate)));

                        }
                    }
                });
            }
    }
}
