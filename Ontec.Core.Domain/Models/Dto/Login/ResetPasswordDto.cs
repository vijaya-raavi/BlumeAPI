namespace Ontec.Core.Domain.Models.Dto.Login
{
    public class ResetPasswordDto
    {
        public int CompanyId { get; set; }
        public string? EmailId { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
