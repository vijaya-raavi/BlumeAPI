using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class AddUpdateMeterQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string MeterNumber { get; set; }
        public string MeterAlias { get; set; }
        public double DailyTargetConsumption { get; set; }
        public string ContractEndDate { get; set; }
        public int MeterTypeId { get; set; }
        public int? StatusId { get; set; }
        public int? ContractProofDocumentId { get; set; }
        public int? ContractProofDocumentTypeId { get; set; }
        public IFormFile? ContractProofDocument { get; set; }

        public string Comments { get; set; }
    }
}
