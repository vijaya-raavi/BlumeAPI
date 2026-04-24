using MediatR;
using Microsoft.AspNetCore.Hosting;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Requests.TopUp.Queries;

namespace Ontec.Core.Application.TopUp.Queries
{
    public class GetBankAccountQueryHandler : IRequestHandler<GetBankAccountsQuery, IEnumerable<BankAccountDto>>
                                             , IRequestHandler<GetPaymentMethodsQuery, IEnumerable<PaymentMethodsDto>>
                                            , IRequestHandler<GetTopUpTransactionsQuery, DatatableModel<GetTopUpTransaction>>
                                            , IRequestHandler<GetUserPayamenstQuery, DatatableModel<UserPaymentsDto>>
                                            , IRequestHandler<DownloadPurchaceRecieptPdfQuery, PurchaceReceiptResponseModel>


    {
        private IHostingEnvironment Environment;
        private readonly IDocumentRepository _documentRepository;

        private readonly IUserRepository _userRepository;
        private readonly ITopUpRepository _topupRepository;
        private readonly IWorkContext _workContext;
        private readonly IOtpService _otpService;
        private readonly IGenericRepository _genericRepository;

        public GetBankAccountQueryHandler(IUserRepository userRepository,
                                    ITopUpRepository topupRepository,
                                    IWorkContext workContext
                                    , IDocumentRepository documentRepository
                                    , IHostingEnvironment _environment
                                    , IOtpService otpService
            ,IGenericRepository genericRepository)
        {
            _documentRepository = documentRepository;
            Environment = _environment;
            _userRepository = userRepository;
            _topupRepository = topupRepository;
            _workContext = workContext;
            _otpService = otpService;
            _genericRepository = genericRepository;
        }
        public async Task<IEnumerable<BankAccountDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetBankAccountsQueryValidator(_topupRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _topupRepository.GetBankAccounts(request).ConfigureAwait(false);
        }
        public async Task<IEnumerable<PaymentMethodsDto>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetPaymentMethodsQueryValidator(_userRepository, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _topupRepository.GetPaymentMethods(request).ConfigureAwait(false);
        }

        public async Task<DatatableModel<GetTopUpTransaction>> Handle(GetTopUpTransactionsQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetTopUpTransactionsQueryValidator(_topupRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _topupRepository.GetTopupTransactions(request).ConfigureAwait(false);
        }

        public async Task<DatatableModel<UserPaymentsDto>> Handle(GetUserPayamenstQuery request, CancellationToken cancellationToken)
        {
            return await _topupRepository.GetUserPayments(request).ConfigureAwait(false);
        }

        public async Task<PurchaceReceiptResponseModel> Handle(DownloadPurchaceRecieptPdfQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var res = new PurchaceReceiptResponseModel();
            var commonValidator = new DownloadPurchaceRecieptPdfQueryValidator(_topupRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            res = await _topupRepository.DownloadPurchaceRecieptPdfQuery(request,_workContext.CurrentUserId).ConfigureAwait(false);
            if (res.PurchaceReceiptUrl != null)
            {
                byte[] fileBytes = null;
                fileBytes = await _genericRepository.GetDocumentAsBytesAsync(res.PurchaceReceiptUrl).ConfigureAwait(false);
                if (fileBytes != null)
                {
                    var Doc = new DocumentResultDto
                    {
                        FileName = Path.GetFileName(res.PurchaceReceiptUrl),
                        Type = Path.GetExtension(res.PurchaceReceiptUrl),
                        Document = fileBytes
                    };

                    res.Receipt = Doc;
                    res.PurchaceReceiptUrl = null;
                }
            }
            return res;
        }



    }
}
