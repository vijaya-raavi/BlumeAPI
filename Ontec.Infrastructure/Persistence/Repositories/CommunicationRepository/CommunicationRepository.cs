using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.CommunicationSetting;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Ontec.Infrastructure.Persistence.Repositories.CommunicationRepository
{
    public class CommunicationRepository(IGenericRepository genericRepository) : ICommunicationRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;

        public async Task<IEnumerable<CommunicationType>> GetCommunicationsByUserId(int userId)
        {

            var sQuery = @"SELECT distinct ec.Id ,ec.name ,ec.display_name as DisplayName, urs.status_id as Status ,
                           CASE WHEN urs.status_id=@StatusId then true ELSE false END as IsActive
	                       FROM public.ohd_user_communication_setting as urs
	                       Join public.ohd_enum_communications as ec on urs.communication_id =ec.id
	                       where urs.user_id=@userId and ec.status_id=@StatusId and urs.status_id=@StatusId";
            var parameter = new DynamicParameters();
            parameter.Add("@userId", userId);
            parameter.Add("@StatusId", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<CommunicationType>(sQuery, parameter);
        }
        public async Task InsertUserCommunications(IEnumerable<int> communicationsIds, int userId)
        {
            var parameters = new DynamicParameters();
            foreach (var comId in communicationsIds)
            {
                var sQuery = @"INSERT INTO public.ohd_user_communication_setting(
	                       user_id, communication_id, status_id, created_at)
	                      VALUES (
                            @UserId
                            ,@CommunicationId
                            ,@StatusId
                            ,@CreatedAt
                            )
                        RETURNING lastval()";
                 parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CommunicationId", comId);
                parameters.Add("@StatusId", (Int32)StatusEnum.Active);
                parameters.Add("@CreatedAt", DateTime.UtcNow);
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
           
        }

        public async Task InsertUserSettings(int communicationId, int userId,int statusId)
        {
            var parameters = new DynamicParameters();
           
                var sQuery = @"INSERT INTO public.ohd_user_communication_setting(
	                       user_id, communication_id, status_id, created_at)
	                      VALUES (
                            @UserId
                            ,@CommunicationId
                            ,@StatusId
                            ,@CreatedAt
                            )
                        RETURNING lastval()";
                parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@CommunicationId", communicationId);
                parameters.Add("@StatusId", statusId);
                parameters.Add("@CreatedAt", DateTime.UtcNow);
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            

        }
        
        public async Task DeleteUserSettings(int userId)
        {
            var parameters = new DynamicParameters();

            var sQuery = @"DELETE from  public.ohd_user_communication_setting
                            Where user_id=@UserId";
            parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
           
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);


        }
        public async Task UpdateUserCommunications(IEnumerable<int> communicationsIds, int userId, IEnumerable<int> ExistcommIds)
        {
            try
            {
                var parameters = new DynamicParameters();
                if (ExistcommIds.Count()>0)
                {
                    foreach (var existcomId in ExistcommIds)
                    {
                        var sQuery = @"UPDATE public.ohd_user_communication_setting
                           SET status_id=@StatusId 
                               ,modified_at=@ModifiedAt
                           WHERE user_id=@UserId AND communication_id=@CommunicationId;";

                        parameters.Add("@UserId", userId);
                        parameters.Add("@CommunicationId", existcomId);
                        parameters.Add("@StatusId", (Int32)StatusEnum.Inactive);
                        parameters.Add("@ModifiedAt", DateTime.UtcNow);
                        await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
                    }
                }
                //if (communicationsIds.Count() > 0)
                //{
                    
                //    foreach (var comId in communicationsIds)
                //    {
                //        var sQuery = @"UPDATE public.ohd_user_communication_setting
	               //       SET status_id=@StatusId 
                //            ,modified_at=@ModifiedAt
	               //       WHERE communication_id=@ComId and user_id=@UserId;";
                //        parameters = new DynamicParameters();
                //        parameters.Add("@ComId", comId);
                //        parameters.Add("@ModifiedAt", DateTime.UtcNow);
                //        parameters.Add("@StatusId", (Int32)StatusEnum.Active);
                //        parameters.Add("@UserId", userId);
                //        var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                //    }
                //}
                 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<CommunicationEnumModel>> GetCommunicationEnumModel()
        {
            var sQuery = @"SELECT ec.Id,ec.Name, ec.display_name as displayname,s.Id as StatusId,s.name as Status, s.display_value as StatusDisplayName
	                       FROM public.ohd_enum_communications as  ec
	                       join public.ohd_enum_status as s on ec.status_id = s.id
	                       where S.name=@ActiveStatus;";
            var parameters = new DynamicParameters();
            parameters.Add("@ActiveStatus", CommunicationTypeEnum.Mobile.ToString());

            return await _genericRepository.GetAsync<CommunicationEnumModel>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<IEnumerable<OntecSelectListItem>> GetCommunicationMasters()
        {
            var sQuery = @"SELECT Id,name 
	                       FROM public.ohd_enum_communications 
	                       where status_id=@Status";
            var parameters = new DynamicParameters();
            parameters.Add("@Status", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);
        }
    }
}
