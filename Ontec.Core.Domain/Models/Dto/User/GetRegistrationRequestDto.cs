using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.User
{
    public class GetRegistrationRequestDto
    {
        public int Id { get; set; }
        public string ProfileUrl {  get; set; }
        public DocumentResultDto ProfileImage { get; set; }
        public string Consumer   { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; } 
        public string Date {  get; set; }
        public string Document {  get; set; }
        public DocumentResultDto DocumnetProof { get; set; }
    }
}
