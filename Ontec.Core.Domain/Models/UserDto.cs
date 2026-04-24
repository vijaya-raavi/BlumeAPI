namespace Ontec.Core.Domain.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int CompanyId { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public int StatusId { get; set; }

        public string Password {  get; set; }
        public string FirstName {  get; set; }

        public string TaxNumber {  get; set; }
       // public int PropertyId { get; set; }
        public int MeterId {  get; set; }
        public string DeviceToken {  get; set; }
        public string AcceptedTermConditionVersion { get; set; }

        public bool IsBusiness {  get; set; }
    }
}
