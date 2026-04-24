using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Meter.Command
{
    public class GetApproveRejectMeterQuery : IRequest<AddUpdateResultDto>
    {
        public int MeterID { get; set; }
        public bool IsApproved {  get; set; }
        public string? Comments { get; set; }
        public bool IsChecked {  get; set; }
    }
}
    