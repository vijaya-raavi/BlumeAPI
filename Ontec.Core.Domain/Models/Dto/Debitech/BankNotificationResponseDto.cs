using Microsoft.Extensions.Hosting;

namespace Ontec.Core.Domain.Models.Dto.Debitech
{
    public class BankNotificationResponseDto
    {
        public string BankTransactionId { get; set; }
        public string NetUpTransactionGuid { get; set; } 
        public string Message { get; set; }
        public int Status { get; set; }
    }
}
