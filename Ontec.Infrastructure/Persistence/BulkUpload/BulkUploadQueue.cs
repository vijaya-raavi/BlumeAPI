using System.Collections.Concurrent;
using System.Threading.Channels;
using Ontec.Core.Domain.Interface.BulkUpload;

namespace Ontec.Infrastructure.Services
{
    /// <summary>
    /// In-memory queue for dispatching bulk upload batches to the background worker.
    ///
    /// Dedup is critical here: both the fast path (command handler enqueues right
    /// after insert) and the poll/sweep path (re-enqueues anything Pending/stale
    /// Processing) can try to enqueue the *same* batchId. Without tracking
    /// in-flight ids, a batch can get dispatched twice in parallel and processed
    /// concurrently by two workers — duplicate CreateUserAsync calls for the
    /// same users.
    ///
    /// _inFlight tracks any batchId that is currently queued OR being processed.
    /// It's only cleared when the consumer calls Complete(batchId).
    /// </summary>
    public class BulkUploadQueue : IBulkUploadQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

        private readonly ConcurrentDictionary<Guid, byte> _inFlight = new();

        public void Enqueue(Guid batchId)
        {
            // TryAdd returns false if already in-flight (queued or processing) —
            // this is what stops the double-dispatch race described above.
            if (_inFlight.TryAdd(batchId, 0))
            {
                // Channel is unbounded, TryWrite never fails in practice here.
                _channel.Writer.TryWrite(batchId);
            }
        }

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        public void Complete(Guid batchId)
        {
            _inFlight.TryRemove(batchId, out _);
        }
    }
}
