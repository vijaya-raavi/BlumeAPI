using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class AddOrUpdateBankAccountQuery : IRequest<AddUpdateResultDto>
    {
        public int Id {  get; set; }

        public string BankName {  get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCode {  get; set; }
        public bool Status { get; set; }
    }
}
