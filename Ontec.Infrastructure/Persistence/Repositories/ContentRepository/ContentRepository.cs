using Dapper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Content;
using Ontec.Core.Domain.Models.Dto.Content;
using Ontec.Core.Domain.Requests.Content.Command;
using Ontec.Core.Domain.Requests.Content.Queries;

namespace Ontec.Infrastructure.Persistence.Repositories.ContentRepository
{
    public class ContentRepository : IContentRepository
    {
        private readonly IGenericRepository _genericRepository;
        public ContentRepository(IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository;
        }
        public async Task<ContentMaster> GetContent(GetContentQuery request)
        {
            var sQuery = @"SELECT id As Id,
                          content_value AS Value ,
                            version AS Version
                          FROM ohd_content 
                          WHERE content_name=@ContentName";

            var parameters = new DynamicParameters();
            parameters.Add("@ContentName", request.ContentName.ToLower());
             return await _genericRepository.GetFirstOrDefaultAsync<ContentMaster>(sQuery, parameters);
           
        }
        public async Task<bool> IsExist(GetContentQuery request)
        {
            var sQuery = @"SELECT count(id)
                          FROM ohd_content 
                          WHERE content_name=@ContentName";

            var parameters = new DynamicParameters();
            parameters.Add("@ContentName", request.ContentName.ToLower());
            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;

        }
        public async Task<bool> IsIdExist(AddContentQuery request)
        {
            var sQuery = @"SELECT count(id)
                          FROM ohd_content 
                          WHERE id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id);
            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;

        }
        public async Task<int> UpdateContnet(AddContentQuery request)
        {
            try
            {

                var sQuery = @"UPDATE public.ohd_content SET                         
                         content_value=@Value   
                        ,modified_at =@ModifiedAt               
                         ,version=@Version                         
                        WHERE id=@Id; 
                         SELECT id from ohd_content WHERE id=@Id;";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", request.Id);
                parameters.Add("@Value", request.Content);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@Version", request.Version);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
