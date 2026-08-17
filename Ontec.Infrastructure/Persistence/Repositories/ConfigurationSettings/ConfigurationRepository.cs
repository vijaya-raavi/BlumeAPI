using System.Globalization;
using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Models.Dto.Configuration;
using Ontec.Core.Domain.Requests.Configuration.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Queries;
using Ontec.Core.Domain.Requests.Content.Command;

namespace Ontec.Infrastructure.Persistence.Repositories.Configuration
{
    public class ConfigurationRepository(IGenericRepository genericRepository, IWorkContext workContext) : IConfigurationRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;
        private readonly IWorkContext _workContext = workContext;



        public async Task UpdateConfiguration(AddOrUpdateConfigurationQuery request)
        {
            try
            {
                foreach (var configuration in request.Configurations)
                {
                    var sQuery = @"UPDATE public.ohd_configuration SET 
                         value=@Value                       
                        ,modified_at =@ModifiedAt               
                         ,modified_by=@ModifiedBy 
                        
                        WHERE id=@Id; 
                         SELECT id from ohd_configuration WHERE id=@Id;";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", configuration.Id);
                    parameters.Add("@Value", configuration.Value);
                    parameters.Add("@ModifiedAt", DateTime.UtcNow);
                    parameters.Add("@ModifiedBy", _workContext.CurrentUserId);

                    var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<IEnumerable<ConfigurationDto>> GetConfigurations()
        {
            try
            {
                var sQuery = @"SELECT   id as Id, 
                            name as Name, 
                            value  AS value, 
                            status_id AS StatusId
                            --created_at AS CreatedAt  
							--,created_by AS CreatedBy
							--,modified_at AS ModifiedAt 
							--,modified_by AS ModifiedBy
                            ,is_editable AS isEditable
                            ,display_name AS DisplayName
                            ,note AS Note
                            ,configuration_type AS ConfigurationType
                            FROM 
                            public.ohd_configuration
                            WHERE status_id=@Active AND is_editable = @IsEditable
                            Order by id asc;";

                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@IsEditable", true);
                var configurations = await _genericRepository.GetAsync<ConfigurationDto>(sQuery, parameters).ConfigureAwait(false);
                return configurations;
            }
            catch (Exception ex) { throw ex; }
        }
        public async Task<IEnumerable<BusinessHoursConfigurationsDto>> GetBusinessHoursConfigurations()
        {
            try
            {

                var sQuery = @"SELECT id AS Id, 
                        day AS Name,
                        enable As Enabled, 
                        ""from"" as From,
                        ""to"" as To,
                        customerapprovalnbh as AcceptUserNonBusiness, 
                        customerapprovalbh as AcceptUserBusiness, 
                        meterapprovalnbh as AcceptMeterNonBusiness, 
                        meterapprovalbh AcceptMeterBusiness
                        FROM public.ohd_business_hours_config
                        Order by id;";

                var businessHoursConfigurations = await _genericRepository.GetAsync<BusinessHoursConfigurationsDto>(sQuery).ConfigureAwait(false);
                return businessHoursConfigurations;
            }
            catch (Exception ex) { throw ex; }

        }

        public async Task DeleteConfigurationById(int Id)
        {
            var sQuery = @"UPDATE public.ohd_configuration
                                 SET status_id=@StatusId, modified_by = @last_modified_by,
                                 modified_at =@last_modified_at 
                                 WHERE id =@Id;                         
                                 SELECT COUNT(id) from ohd_configuration WHERE id=@Id;";


            var parameters = new DynamicParameters();
            parameters.Add("@Id", Id);
            parameters.Add("@last_modified_at", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@last_modified_by", _workContext.CurrentUserId);

            await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters);
        }
        public async Task<int> IsIdExist(int id)
        {
            var sQuery = @"SELECT count(id)
                            FROM ohd_configuration  
                          WHERE id=@Id and status_id != @StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdateAppBackGroundImage(string BackGroundUrl, int id)
        {
            var sQuery = @" UPDATE public.ohd_configuration
                            SET value=@BackGroundUrl
                               ,modified_at = @ModifiedAt
                                ,modified_by=@ModifiedBy    
                                WHERE id = @Id ;
                            Select Id From ohd_configuration
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@BackGroundUrl", BackGroundUrl);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@ModifiedBy", _workContext.CurrentUserId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsReasonExist(AddUpdateRejectionReasonQuery request)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_request_reject_reason  
                          WHERE reason=@Reason AND id!=@Id AND rejectionreasonfor=@For";
            var parameters = new DynamicParameters();
            parameters.Add("@Reason", request.RejectionReason);
            parameters.Add("@Id", request.Id);
            parameters.Add("@For", request.RejectionReasonFor);
            int result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsReasonIdExist(int Id)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_request_reject_reason";
            var parameters = new DynamicParameters();
            if (Id > 0)
            {
                sQuery += " WHERE id=@Id";
                parameters.Add("@Id", Id);
            }
            int result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> AddRejectionReason(AddUpdateRejectionReasonQuery request)
        {
            var sQuery = @" INSERT INTO ohd_request_reject_reason
                             (reason,
                              rejectionreasonfor,
                              status_id,
                              created_at)VALUES
                              (@Reason,
                                @For,
                                @StatusId,
                                @CreatedAt)
                               RETURNING lastval();";
            var parameters = new DynamicParameters();
            parameters.Add("@Reason", request.RejectionReason);
            parameters.Add("@for", request.RejectionReasonFor);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@CreatedAt", DateTime.UtcNow);


            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdateRejectionReason(AddUpdateRejectionReasonQuery request)
        {
            var sQuery = @" UPDATE ohd_request_reject_reason
                             SET reason=@Reason,
                              rejectionreasonfor =@For,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_request_reject_reason WHERE id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@Reason", request.RejectionReason);
            parameters.Add("@for", request.RejectionReasonFor);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", request.Id);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<RejectionReasonDto>> GetRejectionReason(GetRejectionReasonQuery request)
        {
            var sQuery = @" SELECT 
                             id,
                            reason
                           FROM public.ohd_request_reject_reason 
                            WHERE status_id=@StatusId";
            var parameter = new DynamicParameters();

            parameter.Add("@StatusId", (int)StatusEnum.Active);
            if (request.Id == 0 && request.RejectionReasonFor != 0)
            {
                sQuery += " AND RejectionReasonFor=@RejectionReasonFor";
                parameter.Add("@RejectionReasonFor", request.RejectionReasonFor);
            }
            if (request.Id > 0 && request.RejectionReasonFor != 0)
            {
                sQuery += " AND RejectionReasonFor=@RejectionReasonFor AND id=@Id";
                parameter.Add("@Id", request.Id);
                parameter.Add("@RejectionReasonFor", request.RejectionReasonFor);
            }
            if (request.RejectionReasonFor == 0 && request.Id > 0)
            {
                sQuery += " AND id=@Id";
                parameter.Add("@Id", request.Id);
            }


            var reasons = await _genericRepository.GetAsync<RejectionReasonDto>(sQuery, parameter).ConfigureAwait(false);
            return reasons;
        }
        public async Task<int> UpdateStatus(UpdateStatusQuery request)
        {
            var sQuery = "";
            if (request.UpdateTo == (int)UpdateStatusEnum.User)
            {
                sQuery = @" UPDATE ohd_user
                             SET status_id=@StatusId,
                                isverified=@IsVerified,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_user WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.Meter)
            {
                sQuery = @" UPDATE ohd_meter
                             SET status_id=@StatusId,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_meter WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.Reason)
            {
                sQuery = @" UPDATE ohd_request_reject_reason
                             SET status_id=@StatusId,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_request_reject_reason WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.Operator)
            {
                sQuery = @" UPDATE ohd_user
                             SET status_id=@StatusId,
                              modified_at=@ModifiedAt
                              WHERE id=@Id AND role_id=@RoleId;
                               SELECT id from ohd_user WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.TermConditionsVersion)
            {
                sQuery = @"UPDATE ohd_user
                             SET 
                                accepted_terms_conditions_version=@TermConditionsVersion,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_user WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.GroupCategory)
            {
                sQuery = @"UPDATE ohd_notificationgroups
                             SET 
                                status_id=@StatusId,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_notificationgroups WHERE id=@Id;";
            }
            if (request.UpdateTo == (int)UpdateStatusEnum.TopUp)
            {
                sQuery = @" UPDATE public.ohd_top_up_transactions
                             SET flag=@StatusId,
                              modified_at=@ModifiedAt
                              WHERE id=@Id;
                               SELECT id from ohd_top_up_transactions WHERE id=@Id;";
            }
            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", request.StatusId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", request.Id);
            parameters.Add("@RoleId", (int)RoleMasterEnum.Operator);
            parameters.Add("@IsVerified", request.IsVerified);
            parameters.Add("@TermConditionsVersion", request.TermConditionsVersion);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task UpdateBusinessConfiguration(UpdateBusinessHoursConfigurations request)
        {
            try
            {
                foreach (var configuration in request.businessConfigurations)
                {
                    var sQuery = @"UPDATE public.ohd_business_hours_config
	                                SET  
	                                day=@Day, 
	                                enable=@Enabled,
	                                ""from""=@From,
	                                ""to""=@To, 
	                                customerapprovalnbh=@AcceptUserNonBusiness, 
	                                customerapprovalbh=@AcceptUserBusiness, 
	                                meterapprovalnbh=@AcceptMeterNonBusiness, 
	                                meterapprovalbh=@AcceptMeterBusiness
	                                WHERE id=@Id; 
                                    SELECT id from ohd_business_hours_config WHERE id=@Id;";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", configuration.Id);
                    parameters.Add("@Day", configuration.Name);
                    parameters.Add("@ModifiedAt", DateTime.UtcNow);
                    parameters.Add("@ModifiedBy", _workContext.CurrentUserId);
                    parameters.Add("@Enabled", configuration.Enabled);

                    if (!string.IsNullOrEmpty(configuration.From) && configuration.From != "string")
                    {
                        DateTime parsedTime = DateTime.ParseExact(configuration.From, "HH:mm", CultureInfo.InvariantCulture);
                        string formattedTime = parsedTime.ToString("HH:mm");
                        parameters.Add("@From", parsedTime);
                    }
                    if (!string.IsNullOrEmpty(configuration.To) && configuration.To != "string")
                    {
                        DateTime parsedTime = DateTime.ParseExact(configuration.To, "HH:mm", CultureInfo.InvariantCulture);
                        string formattedTime = parsedTime.ToString("HH:mm");
                        parameters.Add("@To", parsedTime);
                    }

                    parameters.Add("@AcceptUserNonBusiness", configuration.AcceptUserNonBusiness);
                    parameters.Add("@AcceptUserBusiness", configuration.AcceptUserBusiness);
                    parameters.Add("@AcceptMeterNonBusiness", configuration.AcceptMeterNonBusiness);
                    parameters.Add("@acceptMeterBusiness", configuration.AcceptMeterBusiness);

                    var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<int> IsConfigIdExist(int id)
        {
            try
            {
                var sQuery = @"SELECT   id as Id
                            FROM 
                            public.ohd_configuration
                            WHERE status_id=@Active AND id = @Id
                            Order by id asc;";

                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Id", id);
                int result = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) { throw ex; }
        }
        public async Task<IEnumerable<ConfigurationDto>> GetNotEditableConfigurations()
        {
            try
            {
                var sQuery = @"SELECT   id as Id, 
                            name as Name, 
                            value  AS value, 
                            status_id AS StatusId
                            --created_at AS CreatedAt  
							--,created_by AS CreatedBy
							--,modified_at AS ModifiedAt 
							--,modified_by AS ModifiedBy
                            ,is_editable AS isEditable
                            ,display_name AS DisplayName
                            ,note AS Note
                            ,configuration_type AS ConfigurationType
                            FROM 
                            public.ohd_configuration
                            WHERE status_id=@Active AND is_editable = @IsEditable
                            Order by id asc;";

                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@IsEditable", false);
                var configurations = await _genericRepository.GetAsync<ConfigurationDto>(sQuery, parameters).ConfigureAwait(false);
                return configurations;
            }
            catch (Exception ex) { throw ex; }
        }
        public async Task UpdateMeterUtilityTypeDailyTarget(AddUpdateUtilityTypeDetailsCommandRequest request)
        {
            try
            {
                foreach (var metertype in request.UtilityDailyTarget)
                {
                    var sQuery = @"UPDATE public.ohd_enum_meter_type
	                                SET   modified_at=@ModifiedAt,
                                           mindailytarget=@MinDailyTarget,
                                           maxdailytarget=@MaxDailyTarget
	                                WHERE id=@Id; 
                                    SELECT id from ohd_enum_meter_type WHERE id=@Id;";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Id", metertype.Id);
                    parameters.Add("@ModifiedAt", DateTime.UtcNow);
                    parameters.Add("@MinDailyTarget", metertype.MinDailyTarget);
                    parameters.Add("@MaxDailyTarget", metertype.MaxDailyTarget);
                    var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
