using Ontec.Core.Domain.Models.Dto.Content;
using Ontec.Core.Domain.Requests.Content.Command;
using Ontec.Core.Domain.Requests.Content.Queries;

namespace Ontec.Core.Domain.Interface.Content
{
    public interface IContentRepository
    {
        Task<ContentMaster> GetContent(GetContentQuery request);
        Task<bool> IsExist(GetContentQuery request);
        Task<bool> IsIdExist(AddContentQuery request);
        Task<int> UpdateContnet(AddContentQuery request);
    }
}
