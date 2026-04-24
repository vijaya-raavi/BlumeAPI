using MediatR;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Requests.Meter.Queries
{
    public class FetchMeterFromCustAggmentCommandRequest : IRequest<MasterPropertyAndMeterDetails>
    {
        public string? UnitNumber { get; set; }
        public string? EstateName { get; set; }
        public int? PropertyId { get; set; }
        public string? MeterNumber { get; set; }
        public string? CustomerAgreementNo { get; set; }
        public bool? IsConfirmed{get;set;}
    }
}
