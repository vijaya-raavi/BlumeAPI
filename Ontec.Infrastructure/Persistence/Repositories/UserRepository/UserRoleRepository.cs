using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.UserRole;

namespace Ontec.Infrastructure.Persistence.Repositories.UserRepository
{
    public class UserRoleRepository : IUserRoleRepository
    {

        private readonly IGenericRepository _genericRepository;

        public UserRoleRepository(IGenericRepository genericRepository)
        {
            _genericRepository = genericRepository;
        }
        public async Task<IEnumerable<UserRoleDto>> GetUserRoles()
        {
            var sQuery = @"	SELECT Id, Name,Description,Status
                            ,to_char(created_at::date, 'dd-MM-yyyy') as CreatedAt
                            , to_char(Modified_at ::date, 'dd-MM-yyyy') as ModifiedAt 
                            FROM ohd_user_role_master
	                        ORDER BY id ASC  ";
            var result = await _genericRepository.GetAsync<UserRoleDto>(sQuery).ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<OntecSelectListItem>> GetRoleMasters()
        {
            var sQuery = @"	SELECT Id, Name
                            FROM ohd_user_role_master
                            Where status_id=@StatusId
	                        ORDER BY Name ASC  ";
            var parameter = new DynamicParameters();
            parameter.Add("@StatusId",(int)StatusEnum.Active);
            var result = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<OntecSelectListItem>> GetTitleMasters()
        {
            var sQuery = @"	SELECT id, name
	                        FROM public.ohd_enum_title
	                        where status_id=@StatusId
	                        ORDER BY Name ASC  ";
            var parameter = new DynamicParameters();
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var result = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
       
    }
}
