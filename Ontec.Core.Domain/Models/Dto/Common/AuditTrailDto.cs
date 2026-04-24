namespace Ontec.Core.Domain.Models.Dto.Common
{
    public class AuditTrailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Action { get; set; }
        public string? ActionTable { get; set; }
        public DateTime AddedOn { get; set; }
        public string? Module { get; set; }
        public string EntityName { get; set; }


    }
}
