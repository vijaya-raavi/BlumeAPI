using Dapper;
using iText.Layout.Properties;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.PropertyUser;
using Ontec.Core.Domain.Requests.PropertyUser.Command;

namespace Ontec.Infrastructure.Persistence.Repositories.PropertyUser
{
    public class PropertyUserRepository : IPropertyUserRepository
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IWorkContext _workContext;
        private readonly ICompanyHelper _companyHelper;
        public PropertyUserRepository(IGenericRepository genericRepository
                                      , IWorkContext workContext,
                                        ICompanyHelper companyHelper)
        {
            _genericRepository = genericRepository;
            _workContext = workContext;
            _companyHelper = companyHelper;
        }
        public async Task<EditPropertyUserMasters> GetEditPropertyUserMasters(int userId)
        {
            var result = new EditPropertyUserMasters();
            var sQuery = @"SELECT id, name  
                         FROM public.ohd_property
                         WHERE owner_id=@OwnerId AND Status_id=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@OwnerId", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            result.PropertiesList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            sQuery = @"SELECT id, name
	                   FROM public.ohd_enum_title
	                   where status_id=@StatusId";
            parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            result.TitleList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            sQuery = @"SELECT id, name
	                   FROM public.ohd_enum_user_relation
	                   where status_id=@StatusId";
            parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            result.UserTypeList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            return result;
        }
        public async Task<bool> IsPropertyUserIdExist(int propertyUserId)
        {
            var sQuery = @"SELECT Count(Id) from ohd_property_user_relation
                          WHERE Id=@PropertyUserId and status_id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyUserId", propertyUserId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }

        public async Task<AddEditPropertyUser> GetPropertyUserById(int propertyUserId)
        {
            var sQuery = @"SELECT pur.id
                           ,ur.title as TitleId
                           ,et.name as Title
                           ,ur.first_name as FirstName 
                           ,ur.last_name as LastName
                           ,ur.mobile
                           ,ur.email
                           ,pur.property_id as PropertyId
                           ,pr.name as Property
                           ,pur.user_relation_id as PropertyUserTypeId
                           ,eur.name  as PropertyUserType
                           ,pur.status_id as StatusId
                           ,S.Name as Status
                           ,ur.id as UserId  
	                       FROM public.ohd_property_user_relation as pur
	                       join public.ohd_user as Ur on pur.user_id=ur.id
	                       join public.ohd_enum_user_relation as eur on pur.user_relation_id=eur.id
	                       join public.ohd_enum_title as et on ur.title  =et.id 
	                       join public.ohd_property as pr on pur.property_id=pr.id
	                       join public.ohd_enum_status as s on pur.status_id = s.Id
	                       Where pur.Id=@PropertyUserId  and pur.status_Id!=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyUserId", propertyUserId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.GetFirstOrDefaultAsync<AddEditPropertyUser>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<AddEditPropertyUser> GetPropertyUserByPropertyId(int propertyId)
        {
            var sQuery = @"SELECT pur.id
                           ,ur.title as TitleId
                           ,et.name as Title
                           ,ur.first_name as FirstName 
                           ,ur.last_name as LastName
                           ,ur.mobile
                           ,ur.email
                           ,pur.property_id as PropertyId
                           ,pr.name as Property
                           ,pur.user_relation_id as PropertyUserTypeId
                           ,eur.name  as PropertyUserType
                           ,pur.status_id as StatusId
                           ,S.Name as Status
                           ,ur.id as UserId  
	                       FROM public.ohd_property_user_relation as pur
	                       join public.ohd_user as Ur on pur.user_id=ur.id
	                       join public.ohd_enum_user_relation as eur on pur.user_relation_id=eur.id
	                       join public.ohd_enum_title as et on ur.title  =et.id 
	                       join public.ohd_property as pr on pur.property_id=pr.id
	                       join public.ohd_enum_status as s on pur.status_id = s.Id
	                       Where pur.Id=@PropertyId  and pur.status_Id!=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.GetFirstOrDefaultAsync<AddEditPropertyUser>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task DeletePropertyUserById(int propertyUserId)
        {

            var parameters = new DynamicParameters();
            var sQuery = @" UPDATE public.ohd_property_user_relation
                         SET status_id=@StatusId, modified_at = @ModifiedAt
                         WHERE Id =@PropertyUserId";

            parameters.Add("@PropertyUserId", propertyUserId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);




        }
        public async Task DeleteAssociateUserSetting(int propertyUserId)
        {
            var deleteAssociateSettingsQuery = @" UPDATE public.ohd_associate_user_settings 
                                                 SET allow_top_up=@AllowTopup
                                                 WHERE property_user_id =@PropertyUserId";
            var parameters = new DynamicParameters();
            parameters.Add("@AllowTopup", 0);
            parameters.Add("@PropertyUserId", propertyUserId);
            await _genericRepository.ExecuteScalarAsync(deleteAssociateSettingsQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> AddPropertyUser(AddUpdatePropertyUser propertyUser, int propertyUserId, int serialNumber)
        {
            var sQuery = @"INSERT INTO public.ohd_property_user_relation(
	                       user_id
                         , property_id
                         , user_relation_id
                         , status_id
                         , created_at
                         ,serial_no)
	                      VALUES (@UserId
                         , @PropertyId
                         , @UserRelationId
                         , @StatusId
                         , @CreatedAt
                        ,@SerialNumber)
                     RETURNING lastval() ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", propertyUserId);
            parameters.Add("@PropertyId", propertyUser.PropertyId);
            parameters.Add("@UserRelationId", propertyUser.PropertyUserTypeId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@SerialNumber", serialNumber);
            try
            {
                var id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                if (id > 0 && propertyUser.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
                {
                    AddUpdateAssociateUserSettingsQuery userSettingsQuery = new()
                    {
                        PropertyUserId = id,
                        IsAllowTopUp = false,
                    };
                    await AddAssociateUserSettings(userSettingsQuery).ConfigureAwait(false);
                }
                return id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdatePropertyUser(AddUpdatePropertyUser propertyUser, int propertyUserId)
        {
            var sQuery = @" UPDATE public.ohd_property_user_relation
	                        SET  user_id=@UserId
                                , property_id=@PropertyId
                                , user_relation_id=@UserRelationId
                                , status_id=@StatusId
                                , modified_at=@ModifiedAt
	                        WHERE id=@Id; 
                            Select Id from public.ohd_property_user_relation
                            WHERE id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", propertyUserId);
            parameters.Add("@PropertyId", propertyUser.PropertyId);
            parameters.Add("@UserRelationId", propertyUser.PropertyUserTypeId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", propertyUser.Id);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<PropertyUserDto> GetPropertyUserLists(int propertyId)
        {
            var propertyUsersDto = new PropertyUserDto();
            var sQuery = @"Select pur.Id
                            ,per.name as PropertyName, U.profile_url as ProfileUrl,concat(U.first_name,' ',U.last_name) as UserName
                            ,U.email 
                            ,U.mobile
                            ,pur.created_at as createdOn
                            ,eur.name as roleName 
                            , CASE WHEN aus.allow_top_up is null then 0 else aus.allow_top_up END as TopUpAllow
                           ,pur.user_id as SystemUserId
                           FROM public.ohd_property as Per 
                           JOIN public.ohd_property_user_relation as pur on per.Id=pur.Property_Id
                           RIGHT Join public.ohd_user as U on pur.user_id=U.id
                           JOIN public.ohd_enum_user_relation as eur on pur.user_relation_id = eur.Id
                           LEFT JOIN public.ohd_associate_user_settings as aus on pur.id=aus.property_user_id
                           WHERE per.Id=@PropertyId and pur.status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@Active", (int)StatusEnum.Active);

            var ownersQuery = @"Select U.Id as Id
                               , U.profile_url as ProfileUrl,concat(U.first_name,' ',U.last_name) as UserName
                                ,U.email 
                                ,U.mobile
                                ,to_char(U.created_at::date, 'dd-MM-yyyy') as CreatedOn
                                ,'Owner' as roleName from public.ohd_property as Per 
                                Join public.ohd_user as U on per.owner_id=U.id
                                where per.Id=@PropertyId and per.status_id=@Active";

            var propertyQuery = @"Select per.name as PropertyName
                                FROM public.ohd_property as Per 
                                WHERE per.Id=@PropertyId and per.status_id=@Active";

            var cusrrentUserRole = @"SELECT distinct Case WHEN per.owner_id=@LoggedUserId THEN 'Owner' ELSE ur.name END FROM 
                                     public.ohd_property as per 
                                     LEFT JOIN public.ohd_property_user_relation as pur on per.id=pur.property_id
                                     LEFT JOIN public.ohd_enum_user_relation as ur on pur.user_relation_id=ur.Id
                                     WHERE per.Id=@ProprtyId and (per.owner_id=@LoggedUserId or pur.user_id=@LoggedUserId) 
                                      --AND per.status_id=@Active 
                                     --AND pur.status_id=@Active ";

            var currentUserParamerets = new DynamicParameters();
            currentUserParamerets.Add("@ProprtyId", propertyId);
            currentUserParamerets.Add("@LoggedUserId", _workContext.CurrentUserId);
            currentUserParamerets.Add("@Active", (int)StatusEnum.Active);

            var usersList = await _genericRepository.GetAsync<PropertyUserList>(sQuery, parameters).ConfigureAwait(false);
            var company = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);

            propertyUsersDto.Tenant = usersList.FirstOrDefault(t => t.RoleName.Equals(PropertyUserRelationEnum.Tenant.ToString(), StringComparison.Ordinal));
            propertyUsersDto.Associates = usersList.Where(t => t.RoleName.Equals(PropertyUserRelationEnum.Associate.ToString(), StringComparison.Ordinal)).ToList();

            propertyUsersDto.Owner = await _genericRepository.GetFirstOrDefaultAsync<PropertyUserList>(ownersQuery, parameters).ConfigureAwait(false);

            if (propertyUsersDto.Owner != null)
            {
                if (propertyUsersDto.Owner.ProfileUrl != null)
                {
                    propertyUsersDto.Owner.ProfileUrl = company.Domain + propertyUsersDto.Owner.ProfileUrl;
                    byte[] fileBytes = null;
                    fileBytes = await _genericRepository.GetDocumentAsBytesAsync(propertyUsersDto.Owner.ProfileUrl).ConfigureAwait(false);
                    if (fileBytes != null)
                    {
                        var profileImage = new DocumentResultDto
                        {
                            FileName = Path.GetFileName(propertyUsersDto.Owner.ProfileUrl),
                            Type = Path.GetExtension(propertyUsersDto.Owner.ProfileUrl),
                            Document = fileBytes
                        };

                        propertyUsersDto.Owner.Profile = profileImage;
                    }
                    propertyUsersDto.Owner.ProfileUrl = null;
                }
            }
            if (propertyUsersDto.Tenant != null)
            {
                if (propertyUsersDto.Tenant.ProfileUrl != null)
                {
                    propertyUsersDto.Tenant.ProfileUrl = company.Domain + propertyUsersDto.Tenant.ProfileUrl;
                    byte[] fileBytes = null;
                    fileBytes = await _genericRepository.GetDocumentAsBytesAsync(propertyUsersDto.Tenant.ProfileUrl).ConfigureAwait(false);
                    if (fileBytes != null)
                    {
                        var profileImage = new DocumentResultDto
                        {
                            FileName = Path.GetFileName(propertyUsersDto.Tenant.ProfileUrl),
                            Type = Path.GetExtension(propertyUsersDto.Tenant.ProfileUrl),
                            Document = fileBytes
                        };

                        propertyUsersDto.Tenant.Profile = profileImage;
                    }
                    propertyUsersDto.Tenant.ProfileUrl = null;
                }
            }
            if (propertyUsersDto.Associates != null)
            {
                foreach (var a in propertyUsersDto.Associates)
                {
                    if (a.ProfileUrl != null)
                    {
                        a.ProfileUrl = company.Domain + a.ProfileUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(a.ProfileUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var profileImage = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(a.ProfileUrl),
                                Type = Path.GetExtension(a.ProfileUrl),
                                Document = fileBytes
                            };

                            a.Profile = profileImage;
                        }
                        a.ProfileUrl = null;
                    }
                }
            }
            propertyUsersDto.PropertyName = await _genericRepository.GetFirstOrDefaultAsync<string>(propertyQuery, parameters).ConfigureAwait(false);
            propertyUsersDto.RoleForProperty = await _genericRepository.ExecuteScalarAsync<string>(cusrrentUserRole, currentUserParamerets).ConfigureAwait(false);
            propertyUsersDto.Owner.TopUpAllow = true;
            return propertyUsersDto;
        }

        #region PropertyAssociateUserDelete
        public async Task DeletePropertyAssociateUserById(int propertyId)
        {
            var sQuery = @"UPDATE ohd_property_user_relation SET status_id=@StatusId ,modified_at=@ModifiedAt
                           WHERE property_id = @PropertyId AND user_relation_id=@UserRelationId";

            var parameters = new DynamicParameters();

            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@UserRelationId", (int)PropertyUserRelationEnum.Associate);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            //parameters.Add("@AllowTopup", 0);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<bool> IsPropertyIdExist(int propertyId)
        {
            var sQuery = @"SELECT Count(Id) from ohd_property_user_relation 
                           WHERE  property_id = @PropertyId 
                           AND user_relation_id=@UserRelationId 
                           AND status_id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@UserRelationId", (int)PropertyUserRelationEnum.Associate);
            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<bool> IsTenantValid(int propertyId, int currentUser)
        {
            var sQuery = @"SELECT count(*) FROM public.ohd_property_user_relation 
				           WHERE user_id=@UserId AND user_relation_Id=@UserRelationId and property_id=@PropertyId
                           AND status_id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@UserRelationId", (int)PropertyUserRelationEnum.Tenant);
            parameters.Add("@UserId", currentUser);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

            return count > 0;
        }
        public async Task<bool> IsPropertyUserExist(int propertyId, int userId)
        {
            var sQuery = @"select count (ID) from ohd_Property_User_Relation 
                        where property_id=@PropertyId and user_id= @UserId and status_id=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@UserId", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            var count = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> IsPropertyUserInActive(int propertyId, int userId)
        {
            var sQuery = @"select id from ohd_Property_User_Relation 
                        where property_id=@PropertyId and user_id= @UserId and status_id=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@UserId", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var count = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }
        public async Task<PropertyOwnerDto> GetPropertyOwnerIdByPropertyUserId(int propertyUserId)
        {
            var sQuery = @"select per.owner_id as OwnerId,per.Id as PropertyId from ohd_property AS per
                        LEFT JOIN ohd_property_user_relation AS  puer ON puer.property_id=per.id
                        where puer.Id=@PropertyUserId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyUserId", propertyUserId);

            return await _genericRepository.GetFirstOrDefaultAsync<PropertyOwnerDto>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<IEnumerable<int>> GetPropertyUsersByPropertyId(int propertyId)
        {
            var sQuery = @"Select user_relation_id from public.ohd_property_user_relation
                         where property_Id= @PropertyId";

            var parameter = new DynamicParameters();
            parameter.Add("@PropertyId", propertyId);

            return await _genericRepository.GetAsync<int>(sQuery, parameter).ConfigureAwait(false);
        }
        #endregion

        #region AssociateUserSettings
        public async Task<int> IsAssociateUserSettingsExist(int propertyAssociateUserId)
        {
            var sQuery = @"SELECT id FROM ohd_associate_user_settings 
                          WHERE property_user_id = @PropertyUserId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyUserId", propertyAssociateUserId);
            var count = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }

        public async Task<int> AddAssociateUserSettings(AddUpdateAssociateUserSettingsQuery request)
        {
            var sQuery = @"INSERT INTO public.ohd_associate_user_settings(
	                       property_user_id 
                         , allow_top_up
                         , created_at)
	                      VALUES (@PropertyUserId 
                         ,@AllowTopUp   
                         , @CreatedAt)
                     RETURNING lastval() ";

            var parameters = new DynamicParameters();
            parameters.Add("@PropertyUserId", request.PropertyUserId);
            parameters.Add("@AllowTopUp", request.IsAllowTopUp ? 1 : 0);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            try
            {
                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateAssociateUserSettings(AddUpdateAssociateUserSettingsQuery request)
        {
            var sQuery = @"UPDATE public.ohd_associate_user_settings 
                          SET  allow_top_up=@AllowTopUp
                            ,modified_at=@ModifiedAt
	                       WHERE property_user_id =@PropertyUserId;
                           SELECT id FROM ohd_associate_user_settings 
                           WHERE property_user_id =@PropertyUserId;";

            var parameters = new DynamicParameters();

            parameters.Add("@PropertyUserId", request.PropertyUserId);
            parameters.Add("@AllowTopUp", request.IsAllowTopUp ? 1 : 0);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<GetPropertyOwnerDeatilsDto> GetPropertyOwnerDetails(int propertyId, int UserId)
        {
            try
            {
                var sQuery = @"SELECT distinct   concat(first_name,' ',last_name)AS PropertyOwner,
                            per.name AS Property,per.unit_number AS UnitNumber  
                            FROM ohd_user AS ur 
                            LEFT outer JOIN  ohd_property AS per ON per.owner_id=ur.id
                            WHERE  per.id=@PropertyId AND ur.id=@LoggedUserId";
                var parameters = new DynamicParameters();

                parameters.Add("@PropertyId", propertyId);
                parameters.Add("@LoggedUserId", UserId);

                var result = await _genericRepository.GetFirstOrDefaultAsync<GetPropertyOwnerDeatilsDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<GetPropertyOwnerDeatilsDto> GetPropertyTenantDetails(int propertyId, int UserId)
        {
            try
            {
                var sQuery = @"SELECT distinct   concat(first_name,' ',last_name)AS PropertyOwner,
                            per.name AS Property,per.unit_number AS UnitNumber  
                            FROM ohd_user AS ur 
                            LEFT JOIN public.ohd_property_user_relation as pur ON ur.id=pur.user_id
                           LEFT outer JOIN  ohd_property AS per ON pur.property_id=per.id
                            WHERE  per.id=@PropertyId AND ur.id=@LoggedUserId";
                var parameters = new DynamicParameters();

                parameters.Add("@PropertyId", propertyId);
                parameters.Add("@LoggedUserId", UserId);

                var result = await _genericRepository.GetFirstOrDefaultAsync<GetPropertyOwnerDeatilsDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion

        public async Task<int> GetSerialNumber(int propertyId)
        {
            var sQuery = @"SELECT  CASE 
                            WHEN serial_no < 10 THEN LPAD(serial_no::text, 2, '0')
                            ELSE serial_no::text
                             END AS serial_no FROM public.ohd_property_user_relation 
                            WHERE property_id=@PropertyId order by id desc";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<int> GetSerialNumberByPropertyUserId(int propertyId, int userId)
        {
            var sQuery = @"SELECT  CASE 
                            WHEN serial_no < 10 THEN LPAD(serial_no::text, 2, '0')
                            ELSE serial_no::text
                             END AS serial_no FROM public.ohd_property_user_relation 
                            WHERE property_id=@PropertyId and user_id=@UserId order by id desc";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@UserId", userId);
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<int> getUserIdBySerialNumberPropertyId(int serialNo,int propertyId)
        {
            var sQuery = @"SELECT  user_id
                           FROM public.ohd_property_user_relation 
                            WHERE serial_no=@SerialNo AND property_id=@PropertyId";
            var parameters = new DynamicParameters();
            parameters.Add("@SerialNo", serialNo);
            parameters.Add("@PropertyId", propertyId);
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

    }
}

