using Ontec.Core.Domain.Models.Dto.BulkUpload;

namespace Ontec.Core.Domain.Interface.BulkUpload
{
    public interface IBulkUploadRepository
    {
        Task<Guid> CreateBatchAsync(int totalRecords, int createdBy);
        Task InsertRecordsAsync(Guid batchId, List<BulkUploadRecord> records);
        Task<IEnumerable<long>> GetPendingRecordIdsAsync(Guid batchId);
        Task<IEnumerable<BulkUploadRecord>> GetRecordsByIdsAsync(long[] recordIds);
        Task UpdateRecordResultAsync(long recordId, string status, string? errorMessage);
        Task UpdateBatchStatusAsync(Guid batchId, string status);
        Task FinalizeBatchAsync(Guid batchId);
        Task<BulkUploadBatch?> GetBatchAsync(Guid batchId);
        Task<IEnumerable<BulkUploadRecord>> GetFailedRecordsAsync(Guid batchId);
        Task ResetFailedRecordsToPendingAsync(Guid batchId);
        Task<BulkUploadBatchDto> GetBatchStatus(Guid batchId);
        Task<IEnumerable<Guid>> GetBatchesNeedingWorkAsync(TimeSpan staleAfter);
        Task TouchHeartbeatAsync(Guid batchId);
        Task<IEnumerable<BulkUploadRecord>> GetSuccessfulUnnotifiedRecordsAsync(int take);
        Task MarkRecordsEmailSentAsync(IEnumerable<long> recordIds);
    }
}
