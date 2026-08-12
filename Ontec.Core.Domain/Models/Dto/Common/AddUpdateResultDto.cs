namespace Ontec.Core.Domain.Models.Dto.Common
{
    public class AddUpdateResultDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string UtilityType {  get; set; }
        public int UtilityId { get; set; }
        public int StatusId {  get; set; }
        public string Status { get; set; }
    }
    public class BulkUploadResultDto
    {
        public int UserId { get; set; }
        public Guid BatchId { get; set; }
        public string Message { get; set; }
    }
    public class VerifyOTPDto
    {
        public string Message { get; set; }
        public string VerifiedKey { get; set; }
    }
    public class MeterAgreementEntry
    {
        public string UnitNumber { get; set; }
        public string MeterNumber { get; set; }
        public string CustomerAgreementId { get; set; }
    }
}
