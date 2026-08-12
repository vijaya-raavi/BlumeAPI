namespace Ontec.Core.Domain.Interface.BulkUpload
{
    public interface IBulkUploadProcessor
    {
       // Task ProcessBatchAsync(Guid batchId);
        Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken = default);

    }
}
