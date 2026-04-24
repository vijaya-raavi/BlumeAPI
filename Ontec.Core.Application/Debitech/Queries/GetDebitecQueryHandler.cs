using MediatR;
using Microsoft.Extensions.Logging;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Requests.Debitech.Queries;
using System.Globalization;
using System.Text;

namespace Ontec.Core.Application.Debitech.Queries
{
    public class GetDebitecQueryHandler : IRequestHandler<GetNetCheckSumQuery, string>
    {
        private readonly ITopUpRepository _topUpRepository;
        private readonly ILogger<GetDebitecQueryHandler> _logger;
        public GetDebitecQueryHandler(ITopUpRepository topUpRepository, ILogger<GetDebitecQueryHandler> logger)
        {
            _topUpRepository = topUpRepository;
            _logger = logger;
        }

        public async Task<string> Handle(GetNetCheckSumQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("In getNetCheckSum handler");
            StringBuilder checksumStringBuilder = new StringBuilder();
            checksumStringBuilder.Append(request.TransactionValue.ToString("F2", CultureInfo.InvariantCulture));
            checksumStringBuilder.Append(request.IsCreditTransaction ? "c" : "d");
            checksumStringBuilder.Append(request.AccountNumber);
            checksumStringBuilder.Append(request.PayerReferenceNumber);

            var checkSumString = checksumStringBuilder.ToString().Normalize(NormalizationForm.FormC);
            try
            {
                _logger.LogInformation("Before save request");

                var checksum = ChecksumHelper.PopulateChecksum(checkSumString);
                var result = await _topUpRepository.SaveDebitechgetNetChecksumRequest(request, checksum).ConfigureAwait(false);
                _logger.LogInformation("after save request");
            }
            catch (Exception ex)
            {
                _logger.LogError("In catch of getnetchecksum handler : "+ ex.Message.ToString());

            }
            return ChecksumHelper.PopulateChecksum(checkSumString);
        }
    }
}
