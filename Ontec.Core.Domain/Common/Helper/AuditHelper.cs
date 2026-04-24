namespace Ontec.Core.Domain.Common.Helper
{
    public class AuditHelper
    {
        public int AddedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public string Action { get; set; }
        public string ActionTable { get; set; }
        public string ModuleName { get; set; }
        public int? StatusId {  get; set; }
        public int UpdatedId {  get; set; }
        public string EntityName {  get; set; }
    }
}
