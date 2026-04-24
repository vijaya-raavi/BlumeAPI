using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Property.Command
{
    public class DeletePropertyById : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public bool IsPermanentDelete { get; set; }
        public bool IsRestore { get; set; }
    }
}
