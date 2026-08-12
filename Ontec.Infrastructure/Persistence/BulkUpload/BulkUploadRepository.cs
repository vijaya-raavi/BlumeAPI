using Dapper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.BulkUpload;

namespace Ontec.Core.Infrastructure.Repositories;

public class BulkUploadRepository : IBulkUploadRepository
{
    private readonly IGenericRepository _repo;

    public BulkUploadRepository(IGenericRepository repo)
    {
        _repo = repo;
    }
    public async Task<Guid> CreateBatchAsync(int totalRecords, int createdBy)
    {
        const string sql = @"
            INSERT INTO bulk_upload_batch (batch_id, status, total_records, created_by, created_at)
            VALUES (@BatchId, 'Pending', @TotalRecords, @CreatedBy, now())
            RETURNING batch_id;";

        var batchId = Guid.NewGuid();
        var parameters = new DynamicParameters();
        parameters.Add("@BatchId", batchId);
        parameters.Add("@TotalRecords", totalRecords);
        parameters.Add("@CreatedBy", createdBy);

        await _repo.ExecuteScalarAsync<Guid>(sql, parameters);
        return batchId;
    }

    public async Task InsertRecordsAsync(Guid batchId, List<BulkUploadRecord> records)
    {
        var queries = records.Select(r =>
        {
            var p = new DynamicParameters();
            p.Add("@BatchId", batchId);
            p.Add("@RowNumber", r.RowNumber);
            p.Add("@Payload", r.Payload);
            return new SqlQueryModel
            {
                SqlQuery = @"INSERT INTO bulk_upload_record (batch_id, row_number, payload, status)
                          VALUES (@BatchId, @RowNumber, @Payload::jsonb, 'Pending');",
                SqlParameters = p
            };
        }).ToList();

        await _repo.ExecuteMultipleCommandAsync(queries, steps: 200);
    }

    public async Task<IEnumerable<long>> GetPendingRecordIdsAsync(Guid batchId)
    {
        const string sql = @"SELECT record_id FROM bulk_upload_record
                              WHERE batch_id = @BatchId AND status = 'Pending';";
        var parameters = new DynamicParameters();
        parameters.Add("@BatchId", batchId);

        return await _repo.GetAsync<long>(sql, parameters);
    }
    
    public async Task UpdateRecordResultAsync(long recordId, string status, string? errorMessage)
    {
        const string sql = @"
            UPDATE bulk_upload_record
            SET status = @Status, error_message = @ErrorMessage, processed_at = now()
            WHERE record_id = @RecordId;";

        await _repo.ExecuteCommandAsync(sql, new { RecordId = recordId, Status = status, ErrorMessage = errorMessage });
    }

    public async Task UpdateBatchStatusAsync(Guid batchId, string status)
    {
        const string sql = @"UPDATE bulk_upload_batch SET status = @Status WHERE batch_id = @BatchId;";
        await _repo.ExecuteCommandAsync(sql, new { BatchId = batchId, Status = status });
    }

    public async Task FinalizeBatchAsync(Guid batchId)
    {
        const string sql = @"
            UPDATE bulk_upload_batch b
            SET success_count = c.success,
                failed_count = c.failed,
                processed_count = c.success + c.failed,
                status = CASE WHEN c.failed = 0 THEN 'Completed' ELSE 'CompletedWithErrors' END,
                completed_at = now()
            FROM (
                SELECT
                    COUNT(*) FILTER (WHERE status = 'Success') AS success,
                    COUNT(*) FILTER (WHERE status = 'Failed') AS failed
                FROM bulk_upload_record
                WHERE batch_id = @BatchId
            ) c
            WHERE b.batch_id = @BatchId;";

        await _repo.ExecuteCommandAsync(sql, new { BatchId = batchId });
    }

    public async Task<BulkUploadBatch?> GetBatchAsync(Guid batchId)
    {
        const string sql = @"
            SELECT batch_id AS BatchId, status AS Status, total_records AS TotalRecords,
                   processed_count AS ProcessedCount, success_count AS SuccessCount,
                   failed_count AS FailedCount, created_at AS CreatedAt, completed_at AS CompletedAt
            FROM bulk_upload_batch
            WHERE batch_id = @BatchId;";

        return await _repo.GetSignleOrDefaultAsync<BulkUploadBatch>(sql, new { BatchId = batchId });
    }

    public async Task<IEnumerable<BulkUploadRecord>> GetFailedRecordsAsync(Guid batchId)
    {
        const string sql = @"
            SELECT record_id AS RecordId, row_number AS RowNumber, payload::text AS Payload,
                   error_message AS ErrorMessage
            FROM bulk_upload_record
            WHERE batch_id = @BatchId AND status = 'Failed';";

        return await _repo.GetAsync<BulkUploadRecord>(sql, new { BatchId = batchId });
    }

    public async Task ResetFailedRecordsToPendingAsync(Guid batchId)
    {
        const string sql = @"
            UPDATE bulk_upload_record
            SET status = 'Pending', error_message = NULL
            WHERE batch_id = @BatchId AND status = 'Failed';";

        await _repo.ExecuteCommandAsync(sql, new { BatchId = batchId });
    }
    public async Task<IEnumerable<BulkUploadRecord>> GetRecordsByIdsAsync(long[] recordIds)
    {
        const string sql = @"
        SELECT record_id   AS RecordId,
               batch_id  AS BatchId,
               row_number AS RowNumber,
               payload::text AS Payload,
               status  AS Status,
               error_message AS ErrorMessage
        FROM bulk_upload_record
        WHERE record_id = ANY(@RecordIds);";

        var parameters = new DynamicParameters();
        parameters.Add("@RecordIds", recordIds);

        return await _repo.GetAsync<BulkUploadRecord>(sql, parameters);
    }
    public async Task<BulkUploadBatchDto> GetBatchStatus(Guid batchId)
    {
        var sQuery = @"SELECT batch_id AS BatchId ,
                        status AS Status ,
                        total_records AS TotalRecords , 
                        processed_count AS ProcessedCount ,
                        success_count AS SuccessCount ,
                        failed_count AS FailedCount ,
                        created_by AS CreatedBy , 
                        created_at AS CreatedAt , 
                        completed_at AS CompletedAt 
                        FROM public.bulk_upload_batch
                        WHERE batch_id=@BatchId ;";
        var parameters = new DynamicParameters();
        parameters.Add("@BatchId", batchId);
        return await _repo.GetFirstOrDefaultAsync<BulkUploadBatchDto>(sQuery, parameters);
    }
    public async Task<IEnumerable<Guid>> GetBatchesNeedingWorkAsync(TimeSpan staleAfter)
    {
        // Picks up:
        //   - batches never dispatched yet ('Pending')
        //   - batches that were 'Processing' but whose worker died
        //     (no heartbeat update within `staleAfter`)
        const string sql = @"
        SELECT batch_id
        FROM bulk_upload_batch
        WHERE status = 'Pending'
           OR (status = 'Processing'
               AND (last_heartbeat_at IS NULL OR last_heartbeat_at < now() - @StaleAfter));";

        var parameters = new DynamicParameters();
        parameters.Add("@StaleAfter", staleAfter);

        return await _repo.GetAsync<Guid>(sql, parameters);
    }

    public async Task TouchHeartbeatAsync(Guid batchId)
    {
        const string sql = @"UPDATE bulk_upload_batch SET last_heartbeat_at = now() WHERE batch_id = @BatchId;";
        await _repo.ExecuteCommandAsync(sql, new { BatchId = batchId });
    }
    public async Task<IEnumerable<BulkUploadRecord>> GetSuccessfulUnnotifiedRecordsAsync(int take)
    {
        const string sql = @"
        SELECT record_id AS RecordId, batch_id AS BatchId, row_number AS RowNumber,
               payload::text AS Payload, status AS Status, error_message AS ErrorMessage
        FROM bulk_upload_record
        WHERE 
           status = 'Success'
          AND email_sent = false 
          ORDER BY record_id
        LIMIT @Take;";

        return await _repo.GetAsync<BulkUploadRecord>(sql, new { Take = take });
    }

    public async Task MarkRecordsEmailSentAsync(IEnumerable<long> recordIds)
    {
        const string sql = @"
        UPDATE bulk_upload_record
        SET email_sent = true
        WHERE record_id = ANY(@RecordIds);";

        var parameters = new DynamicParameters();
        parameters.Add("@RecordIds", recordIds.ToArray());

        await _repo.ExecuteCommandAsync(sql, parameters);
    }
}