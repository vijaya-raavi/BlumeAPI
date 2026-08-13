namespace Ontec.Core.Domain.Models.Dto.BulkUpload
{
    public class BulkUploadBatchDto
    {
        public Guid BatchId { get; set; }
        public string Status { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public IEnumerable<BulkUploadRecord> FailedRecords { get; set; }
    }
}
