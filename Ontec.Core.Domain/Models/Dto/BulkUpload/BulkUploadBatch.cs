namespace Ontec.Core.Domain.Models.Dto.BulkUpload
{
    public class BulkUploadBatch
    {
        public Guid BatchId { get; set; }
        public string Status { get; set; } = "Pending";
        public int TotalRecords { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
