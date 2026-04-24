using Microsoft.AspNetCore.Http;

namespace Ontec.Core.Domain.Models.Dto.Document
{
    public class UploadDocumentDto
    {
        public required IFormFile UploadFile { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentNumber {  get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public int Id { get; set; }
    }
}
