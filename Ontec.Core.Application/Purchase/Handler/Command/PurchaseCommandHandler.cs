using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Requests.Purchase.Queries;
using Ontec.Core.Domain.Requests.TopUp.Queries;
using Ontec.Core.Domain.Requests.VendRequest.Commands;

namespace Ontec.Core.Application.Purchase.Handler.Command
{
    public class PurchaseCommandHandler : IRequestHandler<GetSTSPurchaseDetails, GetSTSTopUpTransactions>
    // IRequestHandler<SendVendSTSRequestCommand, IEnumerable<STSVendRequestResponse>>
    {
        private readonly IMeterRepository _meterRepository;
        private readonly IVendRequestHelper _vendRequestHelper;
        private readonly ITopUpRepository _topUpRepository;
        private readonly IPropertyRepository _propertyRepository;
        public PurchaseCommandHandler(IMeterRepository meterRepository,
                                      IVendRequestHelper vendRequestHelper,
                                      ITopUpRepository topUpRepository,
                                      IPropertyRepository propertyRepository)
        {
            _meterRepository = meterRepository;
            _vendRequestHelper = vendRequestHelper;
            _topUpRepository = topUpRepository;
            _propertyRepository = propertyRepository;
        }

        //public async Task<IEnumerable<STSVendRequestResponse>> Handle(SendVendSTSRequestCommand request, CancellationToken cancellationToken)
        //{
        //    var response = new List<STSVendRequestResponse>();
        //    response = (List<STSVendRequestResponse>)await _vendRequestHelper.ProcessSTSRequest(request).ConfigureAwait(true);
        //    List<string> rctNumbers = new List<string>();
        //    if (response.Any())
        //    {
        //        foreach (var item in response)
        //        {
        //            if (item.StatusCode == 200)
        //            {
        //                var isReceiptExist = await _topUpRepository.ISRctNumExist(item.ReceiptNumber).ConfigureAwait(false);
        //                if (isReceiptExist == 0)
        //                {
        //                    var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
        //                    var meter = await _meterRepository.GetMeterByMeterNumber(request.Meter).ConfigureAwait(false);
        //                    var property = await _propertyRepository.GetPropertyById(meter.PropertyId).ConfigureAwait(false);
        //                    string stdToken = "";
        //                    string bssttoken = "";
        //                    string keychangetoken = "";
        //                    double debt = 0;
        //                    string mrktMsg = "";
        //                    string cutMsg = "";
        //                    int userId = 0;
        //                    int meterId = 0;
        //                    string RctNum = "";
        //                    if (string.IsNullOrEmpty(item.Token))
        //                    {
        //                        stdToken = item.Token;
        //                    }
        //                    if (string.IsNullOrEmpty(item.bsstToken))
        //                    {
        //                        bssttoken = item.bsstToken;
        //                    }
        //                    if (string.IsNullOrEmpty(item.keyChangeToken))
        //                    {
        //                        keychangetoken = item.keyChangeToken;
        //                    }

        //                    if (string.IsNullOrEmpty(item.DebtAmount.ToString()))
        //                    {
        //                        debt = item.DebtAmount;
        //                    }
        //                    if (string.IsNullOrEmpty(item.customerMsg))
        //                    {
        //                        cutMsg = item.customerMsg;
        //                    }
        //                    if (string.IsNullOrEmpty(item.mrktMsg))
        //                    {
        //                        mrktMsg = item.mrktMsg;
        //                    }
        //                    if (property != null)
        //                    {
        //                        userId = property.OwnerId;
        //                    }
        //                    if (meter != null)
        //                    {
        //                        meterId = meter.Id;
        //                    }
        //                    if (item.ReceiptNumber != null)
        //                    {
        //                        RctNum = item.ReceiptNumber;
        //                    }
        //                    var obj = new AddSTSTopUpHelper()
        //                    {
        //                        TransactionId = strTransactionNumber,
        //                        UserId = userId,
        //                        MeterId = meterId,
        //                        StdToken = stdToken,
        //                        BsstToken = bssttoken,
        //                        KeyChangeToken = keychangetoken,
        //                        DebtAmount = debt,
        //                        RCTNumber = RctNum,
        //                        CustomerMsg = cutMsg,
        //                        MrktMsg = mrktMsg,
        //                        TransactionDate = item.TransactionDate

        //                    };
        //                    await _topUpRepository.AddTopupTransactionsFromSTSResponse(obj).ConfigureAwait(false);
        //                    var requstdf = new DownloadPurchaceRecieptPdfQuery()
        //                    {
        //                        TransactionId = strTransactionNumber,
        //                    };
        //                    rctNumbers.Add(item.ReceiptNumber);
        //                    await _topUpRepository.DownloadPurchaceRecieptPdfQuery(requstdf, userId).ConfigureAwait(false);

        //                }
        //            }
        //        }
        //    }
        //    return response;
        //}
        public async Task<GetSTSTopUpTransactions> Handle(GetSTSPurchaseDetails request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetSTSPurchaseDetailsValidator(_meterRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var responses = new List<VendRequestResponse>();


            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = DateTime.UtcNow.Date;
            switch (request.StsTransactionPeriod)
            {
                case 1://this month
                    startDate = request.FromDate;
                    endDate = request.ToDate.Date.AddDays(1);
                    break;

                case 2://last month
                    startDate = request.FromDate;
                    endDate = request.ToDate.Date.AddDays(1);
                    break;

                case 3://custom
                    startDate = request.FromDate;
                    endDate = request.ToDate.Date.AddDays(1);
                    break;

                default:
                    startDate = request.FromDate.Date;
                    endDate = request.ToDate.Date.AddDays(1);
                    break;
            }
            var requestSts = new SendVendSTSRequestCommand()
            {
                Meter = request.Meter,
                FromDate = startDate,
                ToDate = endDate,
            };
            responses = (List<VendRequestResponse>)await _vendRequestHelper.ProcessSTSRequest(requestSts).ConfigureAwait(true);
            List<string> rctNumbers = new List<string>();
            if (responses.Any())
            {
                foreach (var item in responses)
                {
                    if (item.Response != null)
                    {
                        var isRctNoExist = await _topUpRepository.ISRCTNoExist(item.ReceiptNumber).ConfigureAwait(false);
                        if (isRctNoExist == 0)
                        {
                            var strTransactionNumber = ChecksumHelper.GenerateTransactionNumber();
                            var meter = await _meterRepository.GetMeterByMeterNumber(request.Meter).ConfigureAwait(false);
                            var property = await _propertyRepository.GetPropertyById(meter.PropertyId).ConfigureAwait(false);
                            int userId = 0;
                            int meterId = 0;
                            if (property != null)
                            {
                                userId = property.OwnerId;
                            }
                            if (meter != null)
                            {
                                meterId = meter.Id;
                            }

                            var obj = new AddSTSTopUpHelper()
                            {
                                TransactionId = strTransactionNumber,
                                UserId = userId,
                                MeterId = meterId,
                                vendResponse = item.Response,
                                StdToken = item.Token,
                                BsstToken = item.bsstToken,
                                KeyChangeToken = item.keyChangeToken,
                                CustomerMsg = item.customerMsg,
                                MrktMsg = item.mrktMsg,
                                RCTNumber = item.ReceiptNumber,
                                Message = item.Message,
                                TxnDate=item.TxnDatetime,
                                TarrifUnits=item.Tarrif,

                            };
                            
                            await _topUpRepository.AddTopupTransactionsFromSTSResponse(obj).ConfigureAwait(false);
                            var requstdf = new DownloadPurchaceRecieptPdfQuery()
                            {
                                TransactionId = strTransactionNumber,
                            };

                            if (item.ReceiptNumber != "" && !string.IsNullOrEmpty(item.ReceiptNumber))
                            {
                                rctNumbers.Add(item.ReceiptNumber);
                            }
                            //await _topUpRepository.DownloadPurchaceRecieptPdfQuery(requstdf, userId).ConfigureAwait(false);
                            
                        }
                        else
                        {
                            rctNumbers.Add(item.ReceiptNumber);
                        }
                    }
                }
            }
            return await _topUpRepository.GetSTSTopUps(rctNumbers,startDate.ToUniversalTime(),endDate.ToUniversalTime()).ConfigureAwait(false);
        }
    }
}
