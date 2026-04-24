namespace Ontec.Core.Domain.Models.Dto.User
{
    public class DeviceTokensDto
    {
        public string Token { get; set; }
        public DateTime LastActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
