namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class MeterDto : BaseModel
    {
        public string MeterNumber { get; set; }
        public string MeterAlias { get; set; }
        public double Target { get; set; }
        public string MeterType { get; set; }
       public string MasterMeterType {  get; set; }
        public string Status { get; set; }
        public string MeterDocument { get; set; }
        public string UnitOfMeasure { get; set; }
        public string Comments { get; set; }
        public string ContractEndDate { get; set; }
        public bool IsSolar { get; set; }
        public bool IsVerified { get; set; }
        public string EFTNumber {  get; set; }
        public DocumentResultDto MeterDoc { get; set; }
    }
    public class DocumentResultDto
    {
        public string Type { get; set; }
        public string FileName { get; set; }
        public byte[] Document { get; set; }
    }
}
