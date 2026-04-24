namespace Ontec.Core.Domain.Models.Dto.PropertyUser
{
    public class AddEditPropertyUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TitleId { get; set; }
        public string? Title { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int PropertyId { get; set; }
        public string? Property { get; set; }
        public int PropertyUserTypeId { get; set; }
        public string? PropertyUserType { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
    }
}