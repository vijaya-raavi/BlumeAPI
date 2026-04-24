namespace Ontec.Core.Domain.Models.Dto.Document
{
    public class DocumentDto : BaseModel
    {
        public int DocumentTypeId { get; set; }
        public string Documentnumber {  get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string extension { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
