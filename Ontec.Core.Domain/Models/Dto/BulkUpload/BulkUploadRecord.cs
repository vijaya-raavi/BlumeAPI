using Ontec.Core.Domain.Requests.BulkUpload.Command;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Domain.Models.Dto.BulkUpload
{
    public class BulkUploadRecord
    {
        public long RecordId { get; set; }
        public Guid BatchId { get; set; }
        public int RowNumber { get; set; }
        public string Payload { get; set; } = default!;   // JSONB
        public string Status { get; set; } = "Pending";
        public string? ErrorMessage { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
    public class StagedBulkUser
    {
        public BulkUsers User { get; set; } = default!;
        public int? DocumentId { get; set; }
    }
}
