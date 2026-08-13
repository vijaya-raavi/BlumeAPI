using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Infrastructure.Persistence.Repositories.BulkUpload;

namespace Ontec.Infrastructure.Services
{
    public class BulkUploadBackgroundService : BackgroundService
    {
        private const int ConcurrentBatchWorkers = 4; // mirrors old Hangfire WorkerCount
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(10); // treat a batch as orphaned if no heartbeat this long

        private readonly IBulkUploadQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BulkUploadBackgroundService> _logger;

        public BulkUploadBackgroundService(
            IBulkUploadQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<BulkUploadBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BulkUploadBackgroundService starting.");

            // On startup, immediately sweep for anything left over from a
            // previous process (crash, deploy, IIS recycle mid-batch).
            await SweepForOutstandingBatchesAsync(stoppingToken);

            var pollTask = PollLoopAsync(stoppingToken);
            var dispatchTask = ConsumeQueueAsync(stoppingToken);

            await Task.WhenAll(pollTask, dispatchTask);
        }

        // Immediate-dispatch path
        private async Task ConsumeQueueAsync(CancellationToken stoppingToken)
        {
            using var semaphore = new SemaphoreSlim(ConcurrentBatchWorkers);
            var running = new List<Task>();

            await foreach (var batchId in _queue.DequeueAllAsync(stoppingToken))
            {
                await semaphore.WaitAsync(stoppingToken);
                running.Add(ProcessOneAsync(batchId, semaphore, stoppingToken));
                running.RemoveAll(t => t.IsCompleted);
            }

            await Task.WhenAll(running);
        }

        // Fallback poll path — catches anything the fast path missed
        private async Task PollLoopAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                    await SweepForOutstandingBatchesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BulkUploadBackgroundService poll loop failed.");
                }
            }
        }

        private async Task SweepForOutstandingBatchesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();

            IEnumerable<Guid> batchIds;
            try
            {
                batchIds = await repo.GetBatchesNeedingWorkAsync(StaleAfter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query outstanding bulk upload batches.");
                return;
            }

            foreach (var batchId in batchIds)
                // Enqueue() itself dedupes against anything already in-flight
                // (queued or currently processing), so it's safe to call this
                // repeatedly for the same batchId every poll tick.
                _queue.Enqueue(batchId);
        }

        private async Task ProcessOneAsync(Guid batchId, SemaphoreSlim semaphore, CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IBulkUploadProcessor>();
                await processor.ProcessBatchAsync(batchId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing bulk upload batch {BatchId}", batchId);
            }
            finally
            {
                // ✅ release the in-flight lock in the queue so this batch can
                // be re-enqueued in the future if it ever goes stale again
                // (e.g. a later retry after a transient failure).
                _queue.Complete(batchId);
                semaphore.Release();
            }
        }
    }
}
