namespace Ontec.Core.Domain.Models.Dto.BulkUpload
{
    public class UserProvisioningResult
    {
        public bool Success { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
