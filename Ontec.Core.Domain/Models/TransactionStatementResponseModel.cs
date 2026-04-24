using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models
{
    public class TransactionStatementResponseModel
    {
            public string ResponseMsg { get; set; }
           public string DocumentUrl {  get; set; }
        public DocumentResultDto TransactionStatement { get; set; }

    }
}
