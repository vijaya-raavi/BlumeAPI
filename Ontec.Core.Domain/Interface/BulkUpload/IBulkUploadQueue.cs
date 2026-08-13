namespace Ontec.Core.Domain.Interface.BulkUpload
{
    public interface IBulkUploadQueue
    {
        /// <summary>
        /// Enqueue a batch for processing. Safe to call multiple times with the
        /// same batchId — duplicates are dropped while the batch is in-flight
        /// (queued or currently processing).
        /// </summary>
        void Enqueue(Guid batchId);

        /// <summary>
        /// Streams batch ids as they're enqueued. Consumer is responsible for
        /// calling Complete(batchId) once processing of that batch has finished
        /// (success or failure) so it can be re-enqueued later if it ever goes
        /// stale again.
        /// </summary>
        IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Marks a batch as no longer in-flight. Call this in a finally block
        /// after ProcessBatchAsync returns/throws.
        /// </summary>
        void Complete(Guid batchId);
    }
}
