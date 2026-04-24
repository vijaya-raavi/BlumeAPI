using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Models.Dto.Transaction
{
    public class TransactionMasterDto
    {
        public IEnumerable<TransactionPropertyList>? PropertyList { get; set; }
        public IEnumerable<OntecSelectListItem>?  TransactionPeriod { get; set; }
    }
    public class TransactionPropertyList
    {
        public int MeterId { get; set; }
        public string MeterNumber { get; set; }
        public int MeterTypeId { get; set; }
        public string MeterType {  get; set; }
        public string PropertyName { get; set; }
        public string PropName {  get; set; }
        public string EFTNo {  get; set; }
        public int PropertyId {  get; set; }
        public bool? AllowTopUp { get; set; }
        public int? PropertyRelationId { get; set; }
    }
}