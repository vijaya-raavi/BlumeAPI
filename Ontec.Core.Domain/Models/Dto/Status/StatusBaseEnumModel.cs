namespace Ontec.Core.Domain.Models.Dto.Status
{
    public class StatusBaseEnumModel:BaseEnumModel
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string StatusDisplayName { get; set; }
    }
}
