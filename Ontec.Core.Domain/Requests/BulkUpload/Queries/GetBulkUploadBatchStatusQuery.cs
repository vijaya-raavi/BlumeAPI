using MediatR;
using Ontec.Core.Domain.Models.Dto.BulkUpload;

namespace Ontec.Core.Domain.Requests.BulkUpload.Queries
{
    public class GetBulkUploadBatchStatusQuery : IRequest<BulkUploadBatchDto>
    {
        public Guid BathcId { get; set; }
    }
}
