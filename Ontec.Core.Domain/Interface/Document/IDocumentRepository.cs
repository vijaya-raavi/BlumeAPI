using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.Transaction;

namespace Ontec.Core.Domain.Interface.Document
{
    public interface IDocumentRepository
    {
        public Task<int> AddDocument(DocumentDto request);
        public Task<int> UpdateDocument(DocumentDto request);
        public Task<bool> IsDocumentTypeValid(int id);
        Task<IEnumerable<OntecSelectListItem>> GetDocumentTypeMasters();
        Task<int> UploadDocument(UploadDocumentDto request);
        Task<string> SaveDocument(UploadDocumentDto request, bool isProfilePic);
        Task<string> SaveLogo(UploadDocumentDto request);

        Task<int> AddApplicationLogger(ApplicationLogger request);
    }
}
