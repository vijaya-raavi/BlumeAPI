using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;

namespace Ontec.Core.Domain.Requests.Transaction.Queries
{
    public class DownloadStatementPdfQueryValidator : AbstractValidator<DownloadStatementPdfQuery>
    {
        public DownloadStatementPdfQueryValidator(IPropertyRepository _propertyRepository)
        {
            RuleFor(m => m.PropertyId).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                //var isExist = await _meterRepository.IsMeterIdExist(model.MeterId).ConfigureAwait(false);
                //if (!isExist)
                //{
                //    context.AddFailure(nameof(DownloadStatementPdfQuery.MeterId), string.Format(CommonConstants.NotExist, nameof(DownloadStatementPdfQuery.MeterId)));

                //}

                var isPropertyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isPropertyExist)
                {
                    context.AddFailure(nameof(GetTransactionSummaryQuery.PropertyId), string.Format(CommonConstants.NotExist, nameof(GetTransactionSummaryQuery.PropertyId)));

                }
                if (model.TransactionCycleId == (int)TransactionPeriod.Custom)
                {
                    DateTime fromdate = model.FromDate.Date;
                    DateTime todate = model.Todate.Date;
                    TimeSpan objTimeSpan = fromdate - todate;
                    double Days = Convert.ToDouble(objTimeSpan.TotalDays);

                    if (fromdate > todate)
                    {
                        context.AddFailure(nameof(DownloadStatementPdfQuery.FromDate), string.Format(CommonConstants.InValidFromDate, nameof(DownloadStatementPdfQuery.FromDate)));
                    }
                    if (todate < fromdate)
                    {
                        context.AddFailure(nameof(DownloadStatementPdfQuery.Todate), string.Format(CommonConstants.InValidToDate, nameof(DownloadStatementPdfQuery.Todate)));
                    }
                    if (Days > 90)
                    {
                        context.AddFailure(nameof(DownloadStatementPdfQuery.FromDate), string.Format(CommonConstants.InValidPeriod, nameof(DownloadStatementPdfQuery.FromDate)));
                        context.AddFailure(nameof(DownloadStatementPdfQuery.Todate), string.Format(CommonConstants.InValidPeriod, nameof(DownloadStatementPdfQuery.Todate)));

                    }
                }
            });
        }
    }
}
