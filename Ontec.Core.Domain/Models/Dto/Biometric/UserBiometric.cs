namespace Ontec.Core.Domain.Models.Dto.Biometric
{
    public class UserBiometric
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string DeviceId { get; set; }

        public string PublicKey { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
