using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Meter
{
    public class EditMeterMasters
    {
        public IEnumerable<OntecSelectListItem>? PropertiesList { get; set; }
        public IEnumerable<OntecSelectListItem>? MeterTypeList { get; set; }
        public IEnumerable<OntecSelectListItem>? DocumentTypeList { get; set; }
        public int ContractDocumentSizeInMb { get; set; }
    }
}
