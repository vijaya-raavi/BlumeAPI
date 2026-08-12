using System.Text.Json;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Models.Dto.BulkUpload;

namespace Ontec.Infrastructure.Persistence.Repositories.BulkUpload
{
    public class BulkUploadProcessor : IBulkUploadProcessor
    {
        private const int ChunkSize = 100;

        private readonly IBulkUploadRepository _bulkUploadRepo;
        private readonly IUserProvisioningService _userService;

        public BulkUploadProcessor(
            IBulkUploadRepository bulkUploadRepo,
            IUserProvisioningService userService)
        {
            _bulkUploadRepo = bulkUploadRepo;
            _userService = userService;
        }

        public async Task ProcessBatchAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            var batch = await _bulkUploadRepo.GetBatchAsync(batchId);
            if (batch is null) return;

            await _bulkUploadRepo.UpdateBatchStatusAsync(batchId, "Processing");
            // Touch immediately so the sweep's StaleAfter window starts counting
            // from "we actually picked this up", not from batch creation time.
            await _bulkUploadRepo.TouchHeartbeatAsync(batchId);

            var pendingIds = (await _bulkUploadRepo.GetPendingRecordIdsAsync(batchId)).ToArray();

            // ✅ sequential chunk processing — safe with IGenericRepository's
            //    transaction-per-call model (TransactionOpen/Close inside CreateUserAsync
            //    is not safe to run in parallel on a shared repository instance)
            foreach (var chunk in pendingIds.Chunk(ChunkSize))
            {
                await ProcessChunkAsync(chunk);

                // ✅ heartbeat after every chunk — without this, any batch that
                // takes longer than the sweeper's StaleAfter window looks
                // "orphaned" mid-run and gets re-enqueued while still in
                // progress, causing duplicate processing.
                await _bulkUploadRepo.TouchHeartbeatAsync(batchId);
            }

            await _bulkUploadRepo.FinalizeBatchAsync(batchId);
        }

        private async Task ProcessChunkAsync(long[] recordIds)
        {
            var records = await _bulkUploadRepo.GetRecordsByIdsAsync(recordIds);

            foreach (var record in records)
            {
                string status;
                string? errorMessage = null;

                try
                {
                    var staged = JsonSerializer.Deserialize<StagedBulkUser>(record.Payload)!;
                    var result = await _userService.CreateUserAsync(staged.User, staged.DocumentId);

                    status = result.Success ? "Success" : "Failed";
                    errorMessage = result.ErrorMessage;
                }
                catch (Exception ex)
                {
                    status = "Failed";
                    errorMessage = ex.Message;
                }

                await _bulkUploadRepo.UpdateRecordResultAsync(record.RecordId, status, errorMessage);
            }
        }
    }
}
