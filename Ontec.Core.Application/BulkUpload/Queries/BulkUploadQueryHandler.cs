using MediatR;
using Ontec.Core.Domain.Interface.BulkUpload;
using Ontec.Core.Domain.Models.Dto.BulkUpload;
using Ontec.Core.Domain.Requests.BulkUpload.Queries;

namespace Ontec.Core.Application.BulkUpload.Queries
{
    public class BulkUploadQueryHandler :IRequestHandler<GetBulkUploadBatchStatusQuery,BulkUploadBatchDto>
    {
        private readonly IBulkUploadRepository _bulkUploadRepository;
        public BulkUploadQueryHandler(IBulkUploadRepository bulkUploadRepository) 
        {
            _bulkUploadRepository = bulkUploadRepository;   
        }
        public async Task<BulkUploadBatchDto> Handle(GetBulkUploadBatchStatusQuery request, CancellationToken cancellationToken)
        {

            var dto = new BulkUploadBatchDto();

             var result= await _bulkUploadRepository.GetBatchStatus(request.BathcId).ConfigureAwait(false);
            var failedRecords = await _bulkUploadRepository.GetFailedRecordsAsync(request.BathcId).ConfigureAwait(false);
            dto.FailedRecords = failedRecords.ToList();
            dto.TotalRecords = result.TotalRecords;
            dto.SuccessCount = result.SuccessCount;
            dto.CompletedAt = result.CompletedAt;
            dto.CreatedAt = result.CreatedAt;
            dto.BatchId = result.BatchId;
            dto.Status = result.Status;
            dto.ProcessedCount = result.ProcessedCount;
            dto.FailedCount = result.FailedCount;
            dto.CreatedBy = result.CreatedBy;
            return dto;
        }
    }
}
