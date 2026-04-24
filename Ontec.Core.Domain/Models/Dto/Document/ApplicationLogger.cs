namespace Ontec.Core.Domain.Models.Dto.Document
{
    public class ApplicationLogger
    {
        public int Id { get; set; }
        public string Request { get; set; }
        public string Error { get; set; }
        public string Method { get; set; }
    }
}
