using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Requests.Transaction.Queries;
using Ontec.Core.Domain.Requests.VendRequest.Commands;

namespace Ontec.Core.Domain.Requests.Purchase.Queries
{
    public class GetSTSPurchaseDetailsValidator : AbstractValidator<GetSTSPurchaseDetails>
    {
        public GetSTSPurchaseDetailsValidator(IMeterRepository meterRepository)
        {
            RuleFor(m => m.Meter).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isMeterExist = await meterRepository.IsMeterNumberExist(model.Meter).ConfigureAwait(false);
                if (isMeterExist == 0)
                {
                    context.AddFailure(nameof(GetSTSPurchaseDetails.Meter), string.Format(CommonConstants.NotExist, nameof(GetSTSPurchaseDetails.Meter).SplitPascalCase()));

                }


                if (string.IsNullOrEmpty(model.FromDate.ToString())&&string.IsNullOrEmpty(model.ToDate.ToString())&& model.FromDate.ToString()!= "string" && model.ToDate.ToString() !="string")
                {
                    DateTime fromdate =model.FromDate.Date;
                    DateTime todate = model.ToDate.Date;
                    TimeSpan objTimeSpan = fromdate - todate;
                    double Days = Convert.ToDouble(objTimeSpan.TotalDays);

                    if (fromdate > todate)
                    {
                        context.AddFailure(nameof(GetSTSPurchaseDetails.FromDate), string.Format(CommonConstants.InValidFromDate, nameof(GetSTSPurchaseDetails.FromDate)));
                    }
                    if (todate < fromdate)
                    {
                        context.AddFailure(nameof(GetSTSPurchaseDetails.ToDate), string.Format(CommonConstants.InValidToDate, nameof(GetSTSPurchaseDetails.ToDate)));
                    }
                    if (Days > 90)
                    {
                        context.AddFailure(nameof(GetSTSPurchaseDetails.FromDate), string.Format(CommonConstants.InValidPeriod, nameof(GetSTSPurchaseDetails.FromDate)));
                        context.AddFailure(nameof(GetSTSPurchaseDetails.ToDate), string.Format(CommonConstants.InValidPeriod, nameof(GetSTSPurchaseDetails.ToDate)));

                    }
                }
            });

        }
    }
}
