namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class UpdateMeterDto : BaseModel
    {
        public int PropertyId { get; set; }
        public string MeterNumber { get; set; }
        public string? MeterAlias { get; set; }
        public double DailyTargetConsumption { get; set; }
        public string? ContractEndDate { get; set; }
        public string Status { get; set; } 
        public int StatusId { get; set; } 
        public int? MeterTypeId { get; set; }
        public string? MeterType { get; set; }
        public int? ContractProofDocumentId { get; set; }
        public string? MeterDocument { get; set; }
        public string? Unitofmeasure { get; set; }
        public string Comments { get; set; }
        public DocumentResultDto MeterDoc { get; set; }
        public string EFTNumber {  get; set; }
    }
}
