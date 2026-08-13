using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Domain.Requests.BulkUpload.Command
{
    public class BulkUploadRequestCommand:IRequest<BulkUploadResultDto>
    {
        public bool IsSingleUser {  get; set; }
        public IEnumerable<BulkUsers> UploadBulkUsers { get; set; }
    }

   
   
}
