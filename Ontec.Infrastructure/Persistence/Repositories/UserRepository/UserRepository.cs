using Dapper;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Biometric;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.BiometricVerification.Command;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Operator.Command;
using Ontec.Core.Domain.Requests.Operator.Queries;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.User.Commands;
using Ontec.Core.Domain.Requests.User.Queries;
using Ontec.Infrastructure.Persistence.Repositories.Document;
using System.Data;

namespace Ontec.Infrastructure.Persistence.Repositories.UserRepository
{
    public class UserRepository(IGenericRepository genericRepository
                                , ICommunicationRepository communicationRepository,
                                ICompanyHelper companyHelper,
                                IWorkContext workContext, IEncryptionandDecryption encryptionandDecryption,
                                IMasterApiConnectService masterApiConnectService,
                                IPropertyRepository propertyRepository,
                                IDocumentRepository documentRepository,
                                IMeterRepository meterRepository,
                            MasterApiSetting masterApiSetting,
                            IConfigurationRepository configurationRepository) : IUserRepository
    {
        private readonly IGenericRepository _genericRepository = genericRepository;
        private readonly ICommunicationRepository _communicationRepository = communicationRepository;
        private readonly ICompanyHelper _companyHelper = companyHelper;
        private readonly IWorkContext _workContext = workContext;
        private readonly IEncryptionandDecryption _encryptionandDecryption = encryptionandDecryption;
        private readonly IMasterApiConnectService _masterApiConnectService = masterApiConnectService;
        private readonly MasterApiSetting _masterApiSetting = masterApiSetting;
        private readonly IDocumentRepository _documentRepository = documentRepository;
        private readonly IPropertyRepository _propertyRepository = propertyRepository;
        private readonly IMeterRepository _meterRepository = meterRepository;
        private readonly IConfigurationRepository _configurationRepository = configurationRepository;

        public async Task<int> RegisterUser(RegisterUserCommand request)
        {
            var sQuery = @" INSERT INTO public.ohd_user(
	                      mobile_country_code,
                                company_id, 
                                mobile, 
                                email,
                                password, 
                                role_id, 
                                status_id, 
                                created_at, 
                                modified_at,
                                isbusiness ,
                                isverified)                          
                                VALUES (@countryCodeId,
                                @companyId,
                                @mobile,
                                @email,
                                @password,
                                @role ,
                                @statusId,
                                @created_at,
                                @modified_at,
                                @IsBusiness,
                                @IsVerified)
                          RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@countryCodeId", request.CountryCodeId);
            parameters.Add("@companyId", request.CompanyId);
            parameters.Add("@mobile", request.MobileNumber);
            parameters.Add("@email", request.EmailId.ToLower());
            parameters.Add("@password", request.Password);
            parameters.Add("@role", (int)RoleMasterEnum.Customer);//as owner
            parameters.Add("@statusId", (int)StatusEnum.InProcess); //pending
            parameters.Add("@created_at", DateTime.UtcNow);
            parameters.Add("@modified_at", DateTime.UtcNow);
            parameters.Add("@IsBusiness", request.Isbusiness);
            parameters.Add("@IsVerified", false);


            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<UserDto> GetUserByEmailComapnyId(GetUserByEmailComapnyId request)
        {
            var sQuery = @"SELECT u.Id
                            , u.email
                            ,u.mobile 
                            , u.first_name as FirstName
                            ,concat(u.first_name,' ',u.last_name) as UserName
                            ,u.password
                            , u.company_id as CompanyId
                            , u.Email,u.role_id as RoleId
                             ,ur.Name as RoleName 
                             ,ur.status_id as StatusId 
                              ,  u.isbusiness As IsBusiness 
                             ,CASE WHEN u.tax_number =null THEN u.tax_number ELSE '' END as TaxNumber
                            ,accepted_terms_conditions_version AS AcceptedTermConditionVersion
                            FROM ohd_user as u
                          JOIN ohd_user_role_master as ur on u.role_id =ur.id 
                          WHERE (lower(u.email)=@email or u.mobile=@email) AND u.company_id=@companyId AND u.status_id!=@Inactive";
            var parameters = new DynamicParameters();
            parameters.Add("@email", request.Email.ToLower());
            parameters.Add("@companyId", request.CompanyId);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);

            return await _genericRepository.GetFirstOrDefaultAsync<UserDto>(sQuery, parameters);
        }
        public async Task<UserProfileDto> GetUserById(int id)
        {
            try
            {
                var sQuery = @"SELECT distinct u.Id
                                ,u.Title as titleId
                                , concat(u.first_name,' ',u.last_name) as UserName
                                , u.first_name as firstname
                                , u.last_name as lastname
                                , u.company_id as CompanyId
                                , u.Email
                                , ur.Name as Role
                                , u.role_id as RoleId 
                                , title.name as title 
                                , mc.code as CountryCode
                                , u.mobile
                                , u.address_lattitude as AddressLattitude
                                , u.address_longitude as AddressLongitude
                                , u.address_line_1 as AddressLine1
                                , u.city
                                , u.state
                                , u.country
                                , u.status_id as statusId
                                , s.display_value as status
                                , u.profile_url as ProfileUrl
                                , u.proof_document_id as ProofDocumentId
                                ,doc.document_type as ProofDocumentTypeId
								,doc.url as ProofDocumentUrl
								,doctype.name AS ProofDocumentType
								,to_char(u.created_at,'dd-MM-yyyy') As AddedOn
                                ,u.last_login_date As LastLoginDate
								,CASE WHEN u.status_id=6 THEN u.comments ELSE null END as Comments
                                ,CASE WHEN u.tax_number IS null THEN ''  ELSE u.tax_number END as TaxNumber
								,CASE WHEN aus.allow_top_up is not  NULL THEN  allow_top_up ELSE 0  END AS AllowTopUp
                                ,u.comments as Comments
                                ,u.isbusiness As IsBusiness 
                                ,u.isverified AS IsVerified
                                ,u.is_forced_password AS IsForcedPasswordChange
                                ,doc.doc_number As DocumentNumber
                                ,u.accepted_terms_conditions_version AS AcceptedTermConditionVersion
                                ,con.version AS TermsConditionsCurrentVersion
                                ,config.value as IsWallet
                                ,conf.value as IsEstateEnable
                                ,auxconf.value As auxaccountdetails
								FROM ohd_user as u
                          JOIN ohd_user_role_master as ur on u.role_id =ur.id
						  LEFT JOIN public.ohd_mobile_country_code as mc  on u.mobile_country_code=mc.id
						  JOIN public.ohd_enum_status as s on u.status_id=s.id
						  LEFT JOIN public.ohd_enum_title as title on u.title=title.id
                          LEFT JOIN public.ohd_document as doc on u.proof_document_id=doc.id
						  LEFT JOIN public.ohd_document_type AS doctype ON doc.document_type=doctype.id
                          LEFT JOIN public.ohd_property_user_relation AS pur on pur.user_id=u.id
						  LEFT JOIN public.ohd_associate_user_settings as aus ON aus.property_user_id=pur.id
                          LEFT JOIN public.ohd_content AS con ON  u.company_id=con.company_id  
						  LEFT JOIN public.ohd_configuration AS config ON u.company_id=config.company_id
                        LEFT JOIN public.ohd_configuration AS conf ON u.company_id=con.company_id
                         LEFT JOIN public.ohd_configuration AS auxconf ON u.company_id=con.company_id
                          WHERE u.id=@userId and con.content_name='terms_conditions' 
						  AND
						  config.name  ='iswallet'  AND conf.name='isestateenable' 
                            AND auxconf.name='auxaccountdetails'  ";
                var parameters = new DynamicParameters();
                parameters.Add("@userId", id);
                parameters.Add("@Rejected", (int)StatusEnum.Rejected);
                var result = await _genericRepository.GetFirstOrDefaultAsync<UserProfileDto>(sQuery, parameters);
                var company = await _companyHelper.GetCompany(result.CompanyId);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.ProfileUrl))
                    {
                        result.ProfileUrl = company.Domain + result.ProfileUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(result.ProfileUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var DocProfile = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(result.ProfileUrl),
                                Type = Path.GetExtension(result.ProfileUrl),
                                Document = fileBytes
                            };

                            result.ProfileImage = DocProfile;

                        }
                    }
                    else
                    {
                        result.ProfileUrl = null;
                    }

                    if (!string.IsNullOrEmpty(result.ProofDocumentUrl))
                    {
                        result.ProofDocumentUrl = company.Domain + result.ProofDocumentUrl;
                    }
                    if (!string.IsNullOrEmpty(result.ProofDocumentUrl))
                    {
                        result.ProofDocumentUrl = result.ProofDocumentUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(result.ProofDocumentUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var Doc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(result.ProofDocumentUrl),
                                Type = Path.GetExtension(result.ProofDocumentUrl),
                                Document = fileBytes
                            };

                            result.DocProof = Doc;
                            result.ProofDocumentUrl = null;
                        }
                        else
                        {
                            result.ProofDocumentUrl = null;
                        }
                    }
                    result.ProfileUrl = null;

                    result.ProofDocumentUrl = null;
                    result.CommunicationTypes = await _communicationRepository.GetCommunicationsByUserId(id);
                    result.CommunicationTypesIds = result.CommunicationTypes.Any() ? result.CommunicationTypes.Select(t => t.Id) : new List<int>();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<UserProfileDto> GetNewRegisterUserById(int id)
        {
            try
            {
                var sQuery = @"SELECT u.Id
                                ,u.Title as titleId
                                , concat(u.first_name,' ',u.last_name) as UserName
                                , u.first_name as FirstName
                                , u.first_name as firstname
                                , u.last_name as lastname
                                , u.company_id as CompanyId
                                , u.Email
                                ,u.mobile
                                , ur.Name as Role
                                , u.role_id as RoleId  
								,to_char(u.created_at,'dd-MM-yyyy') As AddedOn
                                ,u.last_login_date As LastLoginDate
                                ,u.isbusiness As IsBusiness                           
                          FROM ohd_user as u
                          JOIN ohd_user_role_master as ur on u.role_id =ur.id
                          WHERE u.id=@userId";
                var parameters = new DynamicParameters();
                parameters.Add("@userId", id);
                var result = await _genericRepository.GetFirstOrDefaultAsync<UserProfileDto>(sQuery, parameters);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<bool> IsUserIdExist(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user  
                          WHERE id=@Id and status_id != @StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        public async Task<bool> IsUserIdNotInprocess(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user  
                            WHERE id=@Id and status_id != @InActive
                            AND status_id != @InProcess";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@InActive", (int)StatusEnum.Inactive);
            parameters.Add("@InProcess", (int)StatusEnum.InProcess);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        public async Task<int> IsUserExist(int Userid)
        {
            var sQuery = @"SELECT count(id)
                            FROM ohd_user  
                          WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", Userid);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<bool> IsUserByEmailMobileActive(string emialMobile)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user  
                          WHERE (lower(email)=@Email or mobile =@Email)  and status_id != @StatusId and role_id!=@Temporary";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", emialMobile.ToLower());
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        public async Task<int> IsUserTemporary(string emialMobile)
        {
            var sQuery = @"SELECT COALESCE(
                            (
                                SELECT id
                                FROM ohd_user  
                                WHERE (lower(email)=@Email or mobile =@Email)AND role_id = @Temporary
                                LIMIT 1
                            ),
                            0
                        ) AS id";
            var parameters = new DynamicParameters(); 
            parameters.Add("@Email", emialMobile.ToLower());
            parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdateTemporaryUser(RegisterUserCommand request)
        {
            var sQuery = @" Update ohd_user  
                            Set role_id=@CustomerRoleId
                           ,password=@Password
                           ,mobile_country_code=@CountryCodeId  
                           ,status_id=@InProcess
                          WHERE (lower(email)=@Email or mobile =@Email) ;
                          Select Id from ohd_user 
                            WHERE (lower(email)=@Email or mobile =@Email) ";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", request.EmailId.ToLower());
            parameters.Add("@Password", request.Password);
            parameters.Add("@CountryCodeId", request.CountryCodeId);
            parameters.Add("@CustomerRoleId", (int)RoleMasterEnum.Customer);
            parameters.Add("@InProcess", (int)StatusEnum.InProcess);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<LoginResult> IsUserExist(GetUserByEmailQuery model)
        {

            var sQuery = @"SELECT ur.id as UserId
                            ,ur.Mobile 
                            ,Case when ur.id>0 then 1 else 0 end as IsUserValid
                            ,ur.first_name as firstname
                            ,ur.last_name as lastname
                            ,ur.Email as EmailId
                            ,r.Name as Role
                             --,ur.lockoutenabled
                             -- ,ur.accessfailedcount  
                            ,last_login_date,
                             failed_to_validate as FailedCountAttempted
                           ,failedtovalidate_modified_at as LastAttempted
                           ,isblocked as IsBlocked
                           ,es.name as Status
						   ,ur.comments As Comments 
                            ,ur.isbusiness AS IsBusiness
                            ,ur.accepted_terms_conditions_version AS AcceptedTermConditionVersion
                            ,ur.is_forced_password AS IsForcedPasswordChange
                            --,config.value as IsEstateEnable
                           FROM ohd_user as Ur
                           Join public.ohd_user_role_master as r on Ur.role_id = r.Id
                           JOIN public.ohd_enum_status as es ON ur.status_id=es.id
                             --JOIN public.ohd_configuration AS config ON ur.company_id=config.company_id
                          WHERE (lower(ur.email)=@email or ur.mobile=@email) and ur.password=@password  and ur.company_id=@companyId and Ur.status_id!=@Inactive";
            //--AND config.name='isestateenable' ";

            var parameters = new DynamicParameters();
            if (model.IsAdmin)
            {
                sQuery += @" AND (Ur.role_id=@AdminRoleId OR Ur.Role_id=@OperatorRoleId)";
                parameters.Add("@AdminRoleId", (int)RoleMasterEnum.Admin);
                parameters.Add("@OperatorRoleId", (int)RoleMasterEnum.Operator);
            }
            parameters.Add("@email", model.Email.ToLower());
            parameters.Add("@password", model.Password);
            parameters.Add("@companyId", model.CompanyId);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            try
            {
                return await _genericRepository.GetFirstOrDefaultAsync<LoginResult>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new LoginResult();
            }
        }
        public async Task<bool> IsInActiveUserExist(GetUserByEmailQuery model)
        {

            var sQuery = @"SELECT ur.id  
                            FROM ohd_user as Ur 
                          WHERE (lower(ur.email)=@email OR ur.mobile=@email) AND ur.password=@password 
                          AND ur.company_id=@companyId AND Ur.status_id!=@Inactive
                          AND ur.failed_to_validate!=5";

            var parameters = new DynamicParameters();
            if (model.IsAdmin)
            {
                sQuery += @" AND (Ur.role_id=@role_id OR Ur.Role_id=@ORoleId)";
                parameters.Add("@role_id", (int)RoleMasterEnum.Admin);
                parameters.Add("@ORoleId", (int)RoleMasterEnum.Operator);
            }
            else
            {
                sQuery += @" AND Ur.role_id=@role_id";
                parameters.Add("@role_id", (int)RoleMasterEnum.Customer);
            }
            parameters.Add("@email", model.Email.ToLower());
            parameters.Add("@password", model.Password);
            parameters.Add("@companyId", model.CompanyId);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            try
            {
                var userId = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return userId > 0;
            }
            catch (Exception ex)
            {
                var data = ex.Message;
                return false;
            }
        }
        public async Task<bool> IsInActiveUserEmailExist(GetUserByEmailQuery model)
        {

            var sQuery = @"SELECT ur.id  
                            FROM ohd_user as Ur 
                          WHERE (lower(ur.email)=@email OR ur.mobile=@email) 
                          --AND ur.password=@password 
                          AND ur.company_id=@companyId AND Ur.status_id!=@Inactive";

            var parameters = new DynamicParameters();
            if (model.IsAdmin)
            {
                sQuery += @" AND (Ur.role_id=@role_id OR Ur.Role_id=@ORoleId)";
                parameters.Add("@role_id", (int)RoleMasterEnum.Admin);
                parameters.Add("@ORoleId", (int)RoleMasterEnum.Operator);
            }
            else
            {
                sQuery += @" AND Ur.role_id=@role_id";
                parameters.Add("@role_id", (int)RoleMasterEnum.Customer);
            }
            parameters.Add("@email", model.Email.ToLower());
            parameters.Add("@password", model.Password);
            parameters.Add("@companyId", model.CompanyId);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            try
            {
                var userId = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return userId > 0;
            }
            catch (Exception ex)
            {
                var data = ex.Message;
                return false;
            }
        }
        public async Task<int> IsEmailExist(string email, int companyId, bool isSignup = false)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user 
                          WHERE (lower(email)=@email  OR mobile=@email) AND company_id=@companyId ";
            var parameters = new DynamicParameters();

            if (isSignup)
            {
                sQuery += " AND role_id!=@Temporary";
                parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);
            }
            parameters.Add("@email", email.ToLower());
            parameters.Add("@companyId", companyId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsMobileExist(string mobile, int companyId, bool isSignup = false)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user  
                          WHERE lower(mobile)=@mobile  AND company_id=@companyId ";

            var parameters = new DynamicParameters();
            if (isSignup)
            {
                sQuery += " AND role_id!=@Temporary";
                parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);
            }
            parameters.Add("@mobile", mobile.ToLower());
            parameters.Add("@companyId", companyId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> IsOperatorEmailExist(string email, int companyId)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user  
                          WHERE lower(email)=@email AND company_id=@companyId";
            var parameters = new DynamicParameters();
            parameters.Add("@email", email.ToLower());
            parameters.Add("@companyId", companyId);
            parameters.Add("@Operator", (int)RoleMasterEnum.Operator);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsOperatorMobileExist(string mobile, int companyId)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user  
                          WHERE lower(mobile)=@mobile  AND company_id=@companyId";
            var parameters = new DynamicParameters();
            parameters.Add("@mobile", mobile.ToLower());
            parameters.Add("@companyId", companyId);
            parameters.Add("@Operator", (int)RoleMasterEnum.Operator);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsEmailInTempUserExist(string email, int companyId)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user  
                          WHERE lower(email)=@email AND company_id=@companyId";
            var parameters = new DynamicParameters();
            parameters.Add("@email", email.ToLower());
            parameters.Add("@companyId", companyId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsMobileInTempUserExist(string mobile, int companyId)
        {
            var sQuery = @"SELECT Id
                            FROM ohd_user  
                          WHERE lower(mobile)=@mobile  AND company_id=@companyId";
            var parameters = new DynamicParameters();
            parameters.Add("@mobile", mobile.ToLower());
            parameters.Add("@companyId", companyId);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdateUser(UpdateUserCommand request, int proofDocumentId, bool isVerified)
        {
            int CommId = 0;
            bool isContained = false;
            List<int> ExistcommIds = new List<int>();
            var existingUserCommunications = await _communicationRepository.GetCommunicationsByUserId(request.Id).ConfigureAwait(false);
            if (existingUserCommunications == null || !existingUserCommunications.Any())
            {
                await _communicationRepository.InsertUserCommunications(request.CommunicationTypesIds, request.Id).ConfigureAwait(false);
            }
            else
            {
                if (existingUserCommunications != null && existingUserCommunications.Count() >= 1)
                {
                    string strStatus = ((int)StatusEnum.Active).ToString();
                    ExistcommIds = existingUserCommunications.Where(t => t.Status.Equals(strStatus))
                                                              .Select(t => t.Id).ToList();
                    isContained = request.CommunicationTypesIds.Any(id => ExistcommIds.Contains(id));

                }
                if (existingUserCommunications != null && isContained)
                {

                    await _communicationRepository.UpdateUserCommunications(request.CommunicationTypesIds, request.Id, ExistcommIds).ConfigureAwait(false);
                    await _communicationRepository.InsertUserCommunications(request.CommunicationTypesIds, request.Id).ConfigureAwait(false);
                }
                else
                {
                    await _communicationRepository.UpdateUserCommunications(request.CommunicationTypesIds, request.Id, ExistcommIds).ConfigureAwait(false);
                    await _communicationRepository.InsertUserCommunications(request.CommunicationTypesIds, request.Id).ConfigureAwait(false);
                }
            }


            var sQuery = @" UPDATE ohd_user
                            SET title = @Title
                               ,first_name = @FirstName
                               ,last_name = @LastName 
                               --,address_lattitude = @AddressLatitude
                               --,address_longitude = @AddressLongitude
                               ,address_line_1 = @AddressLine1
                               --,address_line_2 = @AddressLine2
                               ,city = @City
                               ,state = @State
                               ,country = @Country
                                ,tax_number=@TaxNumber
                               ,status_id = @Status
                               ,proof_document_id=@ProofDocumentId
                               ,modified_at = @ModifiedAt
                                ,terms_accepted=@TermsAccepted
                                ,comments=@Comments
                                ,isverified=@IsVerified
                                ,accepted_terms_conditions_version=@TermConditionsVersion
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id);
            parameters.Add("@Title", request.TitleId);
            parameters.Add("@FirstName", request.FirstName);
            parameters.Add("@LastName", request.LastName);
            //parameters.Add("@AddressLatitude", request.AddressLatitude);
            //parameters.Add("@AddressLongitude", request.AddressLongitude);
            parameters.Add("@AddressLine1", request.AddressLine1);
            //parameters.Add("@AddressLine2", request.AddressLine2);
            parameters.Add("@City", request.City);
            parameters.Add("@State", request.State);
            parameters.Add("@Country", request.Country);
            parameters.Add("@ProofDocumentId", proofDocumentId);
            parameters.Add("@Status", request.StatusId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@TermsAccepted", request.TermsAccepted ? true : false);
            parameters.Add("@Comments", request.Comments);
            parameters.Add("@IsVerified", isVerified);
            parameters.Add("@TermConditionsVersion", request.TermConditionsVersion);
            if (!string.IsNullOrEmpty(request.TaxNumber))
            {
                parameters.Add("@TaxNumber", request.TaxNumber);
            }
            else
            {
                parameters.Add("@TaxNumber", null);
            }
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public async Task<int> UpdateUserPofilePic(string profileUrl, int id)
        {
            var sQuery = @" UPDATE ohd_user
                            SET profile_url=@ProfileUrl
                               ,modified_at = @ModifiedAt
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@ProfileUrl", profileUrl);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> AddUser(UserProfileDto request, string password, int propertyusertype)
        {
            var sQuery = @" INSERT INTO public.ohd_user(
	                      company_id
                          ,  title
                          , first_name
                          , last_name
                          , mobile
                          , email
                          , password
                          , role_id
                          , status_id
                          , created_at
                          ,tax_number  )
                          VALUES (@CompanyId
                           , @Title
                           , @FirstName
                           , @LastName
                           , @Mobile
                           , @Email
                           , @Password
                           , @Role
                           , @StatusId
                           , @Created_at
                           ,@TaxNumber)
                          RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@Title", request.TitleId);
            parameters.Add("@FirstName", request.FirstName);
            parameters.Add("@LastName", request.LastName);
            parameters.Add("@Mobile", request.Mobile);
            parameters.Add("@Email", request.Email);
            parameters.Add("@Password", password);
            parameters.Add("@Role", (int)RoleMasterEnum.Temporary);//as owner
            parameters.Add("@StatusId", (int)StatusEnum.Pending); //pending
            parameters.Add("@Created_at", DateTime.UtcNow);
            if (propertyusertype == (int)PropertyUserRelationEnum.Tenant)
            {
                parameters.Add("TaxNumber", request.TaxNumber);

            }
            else
            {
                parameters.Add("TaxNumber", "");
            }
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #region Operator
        public async Task<DatatableModel<OperatorDto>> GetOperators(GetOperatorsQuery request)
        {
            var dt = new DatatableModel<OperatorDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };
            if (string.IsNullOrEmpty(request.order) && (request.order == "string"))
                request.order = "asc";
            var sortQuery = "";

            if (string.IsNullOrEmpty(request.sort))
            {
                request.sort = "Operator";
            }
            switch (request.sort.ToLower())
            {
                case "createdon":
                    request.sort = " u.created_at ";
                    break;
                case "operator":
                    request.sort = " u.first_name , u.last_name ";
                    break;

                case "email":
                    request.sort = " u.email ";
                    break;

                case "phone":
                    request.sort = " u.mobile ";
                    break;

            }
            if (!string.IsNullOrEmpty(request.sort) && (request.sort != "string"))
            {
                sortQuery += " order by " + request.sort + " " + request.order;
            }

            try
            {
                var sQuery = @"SELECT u.id
                            , u.profile_url as profileUrl
                            ,concat(u.first_name,' ', u.last_name)as Operator
                            ,u.email as Email,u.mobile as Phone
                            , to_char(u.created_at::date,'dd-MM-yyyy')  as CreatedOn
                            ,CASE WHEN u.status_id=1 THEN true ELSE false END as Status
                             ,u.password As Password
                            ,u.first_name as FirstName
                            ,u.last_name AS LastName
        		             FROM ohd_user AS u WHERE u.role_id =@RoleId";
                var parameters = new DynamicParameters();

                if (request.IsActive.HasValue)
                {
                    sQuery += @" AND u.status_id = @StatusId";
                    if (request.IsActive.Value)
                        parameters.Add("@StatusId", (int)StatusEnum.Active);
                    else
                        parameters.Add("@StatusId", (int)StatusEnum.Inactive);

                }
                if (request.Id != 0)
                {
                    sQuery += @" AND u.id = @Id";
                    parameters.Add("@Id", request.Id);
                }
                sQuery += sortQuery;
                parameters.Add("@RoleId", request.RoleId);
                var operators = await _genericRepository.GetAsync<OperatorDto>(sQuery, parameters).ConfigureAwait(false);
                var result = operators.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                dt.Data = result.ToList();
                dt.TotalRecords = operators.Count();
            }
            catch (Exception ex)
            {

            }
            return dt;
        }

        public async Task<int> UpdateOperatorUser(AddOrUpdateOperatorQuery request)
        {

            var sQuery = @" UPDATE ohd_user
                            SET title = @Title
                               ,first_name = @FirstName
                               ,last_name = @LastName 
                               ,status_id = @Status
                               ,modified_at = @ModifiedAt
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.Id);
            parameters.Add("@FirstName", request.FirstName);
            parameters.Add("@LastName", request.LastName);
            parameters.Add("@Status", (int)StatusEnum.Active);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> AddOperatorUser(AddOrUpdateOperatorQuery request)
        {
            var sQuery = @" INSERT INTO public.ohd_user(
	                      company_id
                          , first_name
                          , last_name
                          , mobile
                          , email
                          , password
                          , role_id
                          , status_id
                          , created_at
                          ,is_forced_password)
                          VALUES (@CompanyId
                           , @FirstName
                           , @LastName
                           , @Mobile
                           , @Email
                           , @Password
                           , @Role
                           , @StatusId
                           , @Created_at
                            ,@IsForcedPassword)
                          RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@FirstName", request.FirstName);
            parameters.Add("@LastName", request.LastName);
            parameters.Add("@Mobile", request.MobileNumber);
            parameters.Add("@Email", request.EmailId);
            parameters.Add("@Password", request.Password);
            parameters.Add("@Role", request.RoleId);//as Operator
            parameters.Add("@StatusId", (int)StatusEnum.Active); //active
            parameters.Add("@Created_at", DateTime.UtcNow);
            parameters.Add("@IsForcedPassword", true);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task DeleteOperatorUserById(int userId)
        {
            var sQuery = @"UPDATE public.ohd_user
                         SET status_id=@StatusId, modified_at = @ModifiedAt
                         WHERE Id =@Id";

            var parameters = new DynamicParameters();

            parameters.Add("@Id", userId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }

        #endregion

        #region GetRregRequest
        public async Task<DatatableModel<GetRegistrationRequestDto>> GetRegistrationRequests(GetRegistrationRequestQuery request)
        {
            var dt = new DatatableModel<GetRegistrationRequestDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };
            if (string.IsNullOrEmpty(request.order) || (request.order == "string"))
                request.order = "desc";
            var sortQuery = "";
            if (!string.IsNullOrEmpty(request.sort) && (request.sort != "string"))
            {
                sortQuery += " order by " + request.sort + " " + request.order;
            }
            try
            {
                var sQuery = @"SELECT u.id ,u.profile_url AS ProfileUrl ,
                            concat(u.profile_url ,' ' ,et.name ,' ' ,u.first_name,' ', u.last_name)AS Consumer,
                            concat(u.address_line_1,' ',u.address_line_2,' ',u.city,' ',u.state,' ',u.Country)AS Address,
                            concat(u.email,' ', u.mobile )AS contact,
                            doc.url AS document,
                            to_char(u.created_at::date, 'dd-MM-yyyy')  AS Date,
                            CASE WHEN u.status_id = 4 THEN false ELSE true END AS status
                            FROM ohd_user AS u
                            LEFT JOIN public.ohd_document AS doc ON doc.id=u.proof_document_id
                            LEFT JOIN public.ohd_enum_title AS et ON et.id=u.title
                             where u.status_id =@StatusId AND u.role_id=@RoleId";
                var parameters = new DynamicParameters();

                parameters.Add("@StatusId", (int)StatusEnum.Pending);
                parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);

                if (!string.IsNullOrEmpty(request.SearchText.Trim()) && (request.SearchText.Trim() != "string"))
                {
                    var searchText = request.SearchText.Trim();
                    var searchTerms = searchText.ToLower().Split(' ');
                    var searchConditions = new List<string>();
                    var index = 0;

                    // Add condition for the full search text
                    var fullSearchTextParam = "@SearchTextFull";
                    searchConditions.Add($@"(lower(u.first_name) like {fullSearchTextParam}
                           OR lower(u.last_name) like {fullSearchTextParam}
                           OR lower(u.address_line_1) like {fullSearchTextParam}
                           OR lower(u.address_line_2) like {fullSearchTextParam}
                           OR lower(u.mobile) like {fullSearchTextParam}
                           OR lower(u.email) like {fullSearchTextParam}
                           OR lower(to_char(u.created_at::date, 'dd-MM-yyyy')) like {fullSearchTextParam})");
                    parameters.Add(fullSearchTextParam, "%" + searchText.ToLower() + "%");

                    // Add conditions for each split term
                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        searchConditions.Add($@"(lower(u.first_name) like {paramName}
                               OR lower(u.last_name) like {paramName}
                               OR lower(u.address_line_1) like {paramName}
                               OR lower(u.address_line_2) like {paramName}
                               OR lower(u.mobile) like {paramName}
                               OR lower(u.email) like {paramName}
                               OR lower(to_char(u.created_at::date, 'dd-MM-yyyy')) like {paramName})");
                        parameters.Add(paramName, "%" + term + "%");
                        index++;
                    }

                    if (searchConditions.Any())
                    {
                        sQuery += " AND (" + string.Join(" OR ", searchConditions) + ")";
                    }
                }

                sQuery += sortQuery;

                var companyhHelper = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
                var RegRequest = await _genericRepository.GetAsync<GetRegistrationRequestDto>(sQuery, parameters).ConfigureAwait(false);
                if (RegRequest != null)
                {
                    foreach (var i in RegRequest)
                    {

                        if (i.ProfileUrl != null)
                        {
                            i.ProfileUrl = companyhHelper.Domain + i.ProfileUrl;

                            byte[] fileBytes = null;

                            fileBytes = await _genericRepository.GetDocumentAsBytesAsync(i.ProfileUrl).ConfigureAwait(false);
                            if (fileBytes != null)
                            {
                                var DocProfile = new DocumentResultDto
                                {
                                    FileName = Path.GetFileName(i.ProfileUrl),
                                    Type = Path.GetExtension(i.ProfileUrl),
                                    Document = fileBytes
                                };

                                i.ProfileImage = DocProfile;

                            }
                            i.ProfileUrl = null;

                        }
                        if (i.Document != null)
                        {
                            byte[] fileBytesdoc = null;
                            i.Document = companyhHelper.Domain + i.Document;
                            fileBytesdoc = await _genericRepository.GetDocumentAsBytesAsync(i.Document).ConfigureAwait(false);
                            if (fileBytesdoc != null)
                            {
                                var documnetProof = new DocumentResultDto
                                {
                                    FileName = Path.GetFileName(i.Document),
                                    Type = Path.GetExtension(i.Document),
                                    Document = fileBytesdoc
                                };

                                i.DocumnetProof = documnetProof;


                            }
                        }
                        i.Document = null;



                    }
                }

                var result = RegRequest.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);


                if (result.Any())
                {
                    result = result.Select(i =>
                    {
                        //i.ProfileUrl = companyhHelper.Domain + i.ProfileUrl;
                        //i.Document = companyhHelper.Domain + i.Document;
                        if (i.DocumnetProof != null)
                        {
                            i.DocumnetProof = i.DocumnetProof;
                        }
                        if (i.ProfileImage != null)
                        {
                            i.ProfileImage = i.ProfileImage;
                        }

                        return i;
                    }).ToList();


                }
                dt.Data = result.ToList();
                dt.TotalRecords = RegRequest.Count();
            }
            catch (Exception ex)
            {

            }
            return dt;
        }
        #endregion

        #region ApproveRejectRegRequest
        public async Task<bool> IsPendingUserIdExist(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user  
                          WHERE id=@Id and status_id = @StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@StatusId", (int)StatusEnum.Pending);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }

        public async Task<int> ApproveRejectRegistrationRequestById(ApproveRejectRegistrtionRequestQuery request)
        {
            var sQuery = @"Update public.ohd_user
                         Set status_id=@StatusId, modified_at = @ModifiedAt , comments=@Comments,isverified=@IsVerified
                         where id=@Id;
                         Select Id from public.ohd_user
                         WHERE id=@Id;  ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.ID);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Comments", request.Comments);
            parameters.Add("@IsVerified", true);
            switch (request.StatusId)
            {
                case (int)StatusEnum.All:
                    {
                        if (request.IsApproved)
                            parameters.Add("@StatusId", (int)StatusEnum.Active);
                        else
                            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                    }
                    break;
                case (int)StatusEnum.Pending:
                    {
                        if (request.IsApproved)
                            parameters.Add("@StatusId", (int)StatusEnum.Active);
                        else
                            parameters.Add("@StatusId", (int)StatusEnum.Rejected);
                    }
                    break;
                case (int)StatusEnum.Active:
                    {
                        if (request.IsApproved)
                            parameters.Add("@StatusId", (int)StatusEnum.Active);
                        else
                            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                    }
                    break;
                case (int)StatusEnum.Inactive:
                    {
                        if (request.IsApproved)
                            parameters.Add("@StatusId", (int)StatusEnum.Active);
                        else
                            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                    }
                    break;
                case (int)StatusEnum.Rejected:
                    {
                        if (request.IsApproved)
                            parameters.Add("@StatusId", (int)StatusEnum.Active);
                        else
                            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                    }
                    break;
            }

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region ResetPassword
        public async Task<int> UpdatePassword(string password, int id)
        {

            var sQuery = @" UPDATE ohd_user
                            SET password=@Password
                               ,modified_at = @ModifiedAt
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Password", password);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region FailedLogedInAttempt

        public async Task<LoginAttemptUserDto> GetLoginAttemptByEmailMobile(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT Id, 
                        first_name AS FirstName,
                        mobile As Mobile,
                        email As Email,
                        password as Password,
                        failed_to_validate as FailedCountAttempted, 
                        failedtovalidate_modified_at as LastAttempted, 
                        isblocked AS IsBlocked, 
                        ( 
                        (DATE_PART('Day', now() - failedtovalidate_modified_at) * 24 +
                        DATE_PART('Hour', now() - failedtovalidate_modified_at)) * 60 +
                        DATE_PART('Minute', now() - failedtovalidate_modified_at)
                        ) AS MinutesFromLastFailledattempts
                           FROM public.ohd_user
                         WHERE (lower(email)=@EmailMobile OR mobile =@EmailMobile ) AND company_id=@CompanyId  and status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@EmailMobile", emailMobile);
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            try
            {
                return await _genericRepository.GetFirstOrDefaultAsync<LoginAttemptUserDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<int> GetUserRoleByEmailMobile(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT role_id
                            
                           FROM public.ohd_user
                         WHERE (lower(email)=@EmailMobile OR mobile =@EmailMobile ) AND company_id=@CompanyId  and status_id=@Pending";
            var parameters = new DynamicParameters();
            parameters.Add("@EmailMobile", emailMobile);
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Pending", (int)StatusEnum.Pending);
            try
            {
                return await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<int> UpdateLoginAttempt(int id, int count, bool block)
        {
            var sQuery = @"Update public.ohd_user
                          Set failed_to_validate= @Count,
                          failedtovalidate_modified_at=@ModifiedAt
                          ,isblocked=@Blocked
                          where id=@Id;
                          Select id from public.ohd_user
                          where id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Count", count);
            parameters.Add("@Blocked", block);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);

        }
        #endregion

        #region LastLoginDate
        public async Task<int> UpdateLastLoginDate(int id)
        {
            try
            {
                var sQuery = @"Update public.ohd_user
                          Set last_login_date= @LastLoginDate                          
                          where id=@Id;
                          Select id from public.ohd_user
                          where id=@Id";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@LastLoginDate", DateTime.UtcNow);

                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex) { }
            return 0;

        }
        #endregion

        #region DeleteUserById
        public async Task DeleteUserById(int userId)
        {
            var updateUserQuery = @"UPDATE public.ohd_user
                         SET status_id=@StatusId, modified_at = @ModifiedAt
                         WHERE Id =@Id;                         
                         SELECT COUNT(id) from ohd_user WHERE id=@Id;";

            var propetyUpdateQuery = @"UPDATE ohd_property SET status_id=@StatusId,modified_at=@Modifiedat
                                       WHERE owner_id=@Id;
                                       SELECT COUNT(id) from ohd_property WHERE owner_id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", userId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var updateUser = _genericRepository.ExecuteScalarAsync<int>(updateUserQuery, parameters);
            var updateProperty = _genericRepository.ExecuteScalarAsync<int>(propetyUpdateQuery, parameters);

            await Task.WhenAll(updateUser, updateProperty).ConfigureAwait(false);
        }
        #endregion

        #region saveUserSettings


        public async Task<int> IsUserSettingsExist(int UserId)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user_communication_setting  
                          WHERE User_id=@UserId";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> SaveUserSettings(SaveUserSettingsQuery request)
        {
            bool isContained = false;

            List<int> existcommIds = new List<int>();
            var result = new List<UserCommunications>();
            var existingUserCommunications = await _communicationRepository.GetCommunicationsByUserId(request.UserId).ConfigureAwait(false);
            if (request.IsEmailEnabled == true)
            {
                result.Add(new UserCommunications
                {
                    CommId = (int)CommunicationTypeEnum.Email,
                    StatusId = (int)StatusEnum.Active
                });

            }
            else
            {
                result.Add(new UserCommunications
                {
                    CommId = (int)CommunicationTypeEnum.Email,
                    StatusId = (int)StatusEnum.Inactive
                });
            }
            if (request.IsMobileEnabled == true)
            {
                result.Add(new UserCommunications
                {
                    CommId = (int)CommunicationTypeEnum.Mobile,
                    StatusId = (int)StatusEnum.Active
                });
            }

            else
            {
                result.Add(new UserCommunications
                {
                    CommId = (int)CommunicationTypeEnum.Mobile,
                    StatusId = (int)StatusEnum.Inactive
                });
            }

            await _communicationRepository.DeleteUserSettings(request.UserId).ConfigureAwait(false);
            foreach (var r in result)
            {

                await _communicationRepository.InsertUserSettings(r.CommId, request.UserId, r.StatusId).ConfigureAwait(false);
            }

            return request.UserId;

        }
        #endregion

        #region ChangePassword
        public async Task<bool> IsOldPasswordExist(ChangePasswordQuery request)
        {

            var sQuery = @"SELECT ur.id  
                          FROM ohd_user as Ur                           
                          WHERE ur.password=@password and id=@UserId";

            var parameters = new DynamicParameters();

            parameters.Add("@password", request.OldPassword);
            parameters.Add("@UserId", request.UserId);
            var userId = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return userId > 0;
        }
        public async Task<string> GetUserPassword(int userId)
        {

            var sQuery = @"SELECT ur.password  
                          FROM ohd_user as Ur                           
                          WHERE  id=@UserId";

            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            return await _genericRepository.GetFirstOrDefaultAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<string> GetInactiveRejPendUserPassword(string email, int companyId)
        {

            var sQuery = @"SELECT ur.password  
                          FROM ohd_user as Ur                           
                          WHERE  company_id=@CompanyId AND lower(email)=@EmailMobile OR mobile =@EmailMobile";

            var parameters = new DynamicParameters();

            parameters.Add("@EmailMobile", email);
            parameters.Add("@CompanyId", companyId);
            return await _genericRepository.GetFirstOrDefaultAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<int> ChangePassword(ChangePasswordQuery request)
        {
            var sQuery = @" UPDATE ohd_user
                            SET password=@Password
                               ,modified_at = @ModifiedAt
                                ,is_forced_password=false
                             WHERE id = @UserId ;
                            Select Id From ohd_user
                             WHERE id = @UserId ";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@Password", request.NewPassword);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region UpdateUserDocument
        public async Task<int> UpdateUserDocument(UpdateUserDocumentQuery request, int proofDocumentId)
        {
            var sQuery = @" UPDATE ohd_user
                            SET  proof_document_id=@ProofDocumentId
                               ,modified_at = @ModifiedAt
                            
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.UserId);
            parameters.Add("@ProofDocumentId", proofDocumentId);

            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        #endregion

        #region SaveUserSentNotifications
        public async Task<int> SaveUserSentNotifications(int UserId, string ReceieverMail, string SenderMail, string NotificationType, int StatusId)
        {
            var sQuery = @"INSERT INTO public.ohd_user_Send_Notifications(
                        user_id,
                        receiver_mail_id,
                        sender_mail_id, 
                        notification_type, 
                        status_id,
                        created_at)
                        VALUES(
                        @UserId,
                        @ReceieverMailId,
                        @SenderMailId,
                        @notificationType,
                        @StatusId,
                        @CreatedAt
                        )  
                        RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@ReceieverMailId", ReceieverMail);
            parameters.Add("@SenderMailId", SenderMail);
            parameters.Add("@notificationType", NotificationType);
            parameters.Add("@StatusId", StatusId);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        public async Task<int> IsUserInactive(string emailMobile, int companyId)
        {
            try
            {
                var sQuery = @"SELECT id FROM ohd_user  
                          WHERE (lower(email)=@Email OR mobile=@Email) AND company_id=@CompanyId AND status_id=@InActive";
                var parameters = new DynamicParameters();
                parameters.Add("@Email", emailMobile.ToLower());
                parameters.Add("@CompanyId", companyId);
                parameters.Add("@InActive", (int)StatusEnum.Inactive);
                parameters.Add("@Rejected", (int)StatusEnum.Rejected);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> IsUserRejected(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT id FROM ohd_user  
                          WHERE (lower(email)=@Email OR mobile=@Email) AND company_id=@CompanyId AND  status_id=@Rejected";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", emailMobile.ToLower());
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Rejected", (int)StatusEnum.Rejected);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> GetUserStatus(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT status_id FROM public.ohd_user  
                          WHERE (lower(email)=@Email OR mobile=@Email) AND company_id=@CompanyId
                           -- AND  status_id=@Active
";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", emailMobile.ToLower());
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> IsUserPending(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT id FROM ohd_user  
                          WHERE (lower(email)=@Email OR mobile=@Email) AND company_id=@CompanyId AND  status_id=@Pending";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", emailMobile.ToLower());
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Pending", (int)StatusEnum.Pending);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }


        #region DeactiveUser
        public async Task DeActiveUserById(int userId)
        {
            var updateUserQuery = @"UPDATE public.ohd_user
                         SET status_id=@StatusId, modified_at = @ModifiedAt
                         WHERE Id =@Id;                         
                         SELECT COUNT(id) from ohd_user WHERE id=@Id;";

            var propetyUpdateQuery = @"UPDATE ohd_property SET status_id=@StatusId,modified_at=@Modifiedat
                                       WHERE owner_id=@Id;
                                       SELECT COUNT(id) from ohd_property WHERE owner_id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", userId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Deactive);

            var updateUser = _genericRepository.ExecuteScalarAsync<int>(updateUserQuery, parameters);
            var updateProperty = _genericRepository.ExecuteScalarAsync<int>(propetyUpdateQuery, parameters);

            await Task.WhenAll(updateUser, updateProperty).ConfigureAwait(false);
        }


        #endregion
        public async Task<UserDto> GetUserByPayerReferenceNumber(string payerReferenceNumber)
        {
            if (!string.IsNullOrEmpty(payerReferenceNumber))
            {
                var eftNumber = payerReferenceNumber;
                if (payerReferenceNumber.Contains(' '))
                {
                    eftNumber = payerReferenceNumber.Split(' ')[1];
                }

                var sQuery = @"Select u.Id, 
                              concat(u.first_name,' ',u.last_name) as UserName, 
                              u.company_id as CompanyId, 
                              u.Email,u.role_id as RoleId,
                              ur.Name as RoleName ,
                              mt.id as MeterId
                              FROM public.ohd_property as per
                              JOIN public.ohd_user as u on per.owner_id=u.id  
                              JOIN ohd_user_role_master as ur on u.role_id =ur.id
                              JOIN public.ohd_meter as mt on per.id =mt.property_id
                               WHERE(mt.eft_number = @payerReferenceNumber or mt.old_eft_no = @payerReferenceNumber)";
                var parameters = new DynamicParameters();
                parameters.Add("@payerReferenceNumber", eftNumber);

                return await _genericRepository.GetFirstOrDefaultAsync<UserDto>(sQuery, parameters);
            }
            else
                return new UserDto();
        }
        public async Task<UserDto> GetPropertyUserById(int userId, int propertyId)
        {


            var sQuery = @"Select u.Id, 
                              concat(u.first_name,' ',u.last_name) as UserName, 
                              u.company_id as CompanyId, 
                              u.Email,u.role_id as RoleId,
                              ur.Name as RoleName ,
                              mt.id as MeterId
                              FROM  public.ohd_property_user_relation as per 
                              JOIN public.ohd_user as u on per.user_id=u.id  
                              JOIN ohd_user_role_master as ur on u.role_id =ur.id
                              JOIN public.ohd_meter as mt on per.property_id =mt.property_id
                              WHERE u.id= @UserId AND per.property_id=@PropertyId AND mt.status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            return await _genericRepository.GetFirstOrDefaultAsync<UserDto>(sQuery, parameters);


        }
        public async Task<UserDto> GetPropertyUserByPayerReferenceNumber(string payerReferenceNumber)
        {

            if (!string.IsNullOrEmpty(payerReferenceNumber))
            {
                var eftNumber = payerReferenceNumber;
                if (payerReferenceNumber.Contains(' '))
                {
                    eftNumber = payerReferenceNumber.Split(' ')[1];
                }

                var sQuery = @"Select u.Id, 
                              concat(u.first_name,' ',u.last_name) as UserName, 
                              u.company_id as CompanyId, 
                              u.Email,u.role_id as RoleId,
                              ur.Name as RoleName ,
                              mt.id as MeterId
                              FROM public.ohd_property as per
                              JOIN public.ohd_user as u on per.owner_id=u.id  
                              JOIN ohd_user_role_master as ur on u.role_id =ur.id
                              JOIN public.ohd_meter as mt on per.id =mt.property_id
                              WHERE mt.eft_number = @payerReferenceNumber";
                var parameters = new DynamicParameters();
                parameters.Add("@payerReferenceNumber", eftNumber);

                return await _genericRepository.GetFirstOrDefaultAsync<UserDto>(sQuery, parameters);
            }
            else
                return new UserDto();
        }
        public async Task CreatePayerReferenceNumber(int userId, int propertyId, string payerReferenceNumber)
        {
            var sQuery = @"insert into ohd_user_referencenumber(userid, propertyid, referencenumber) 
                            values(@userId, @propertyId, @referenceNumber, now())";
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@propertyId", propertyId);
            parameters.Add("@referenceNumber", payerReferenceNumber);

            await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters);
        }
        public async Task<int> InsertDeviceToken(int UserId, string DeviceToken)
        {
            var sQuery = @" INSERT INTO public.ohd_user_device_token(
	                      user_id,device_token, created_at, modified_at)
                          VALUES (@UserId,@DeviceToken,@created_at,@modified_at)
                          RETURNING lastval()";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@DeviceToken", DeviceToken);
            parameters.Add("@created_at", DateTime.UtcNow);
            parameters.Add("@modified_at", DateTime.UtcNow);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<string> GetDeviceToken(int UserId)
        {
            var sQuery = @" SELECT device_token FROM public.ohd_user_device_token
	                      WHERE user_id= @UserId order by id desc";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);

            var result = await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task DeleteDeviceToken(int UserId, string devicetoken)
        {
            var sQuery = @" delete  FROM public.ohd_user_device_token
	                      WHERE device_token=@DeviceToken";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", UserId);
            parameters.Add("@DeviceToken", devicetoken);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<UserDto> GetUserByMeterId(int meterId)
        {
            var sQuery = @"Select u.Id, 
                              concat(u.first_name, ' ', u.last_name) as UserName, 
                              u.company_id as CompanyId, 
                              u.Email,u.role_id as RoleId,
                              ur.Name as RoleName ,
                              mt.id as MeterId,
                              u.email as Email,
                                u.company_id As CompanyId,
							  udt.device_token AS DeviceToken
                              FROM public.ohd_property as per
                              JOIN public.ohd_user as u on per.owner_id=u.id
                              JOIN ohd_user_role_master as ur on u.role_id =ur.id
                              JOIN public.ohd_meter as mt on per.id =mt.property_id
							  LEFT JOIN public.ohd_user_device_token as udt ON u.id=udt.user_id
                              WHERE mt.id = @MeterId";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);


            var result = await _genericRepository.GetFirstOrDefaultAsync<UserDto>(sQuery, parameters).ConfigureAwait(false);
            return result;

        }

        public async Task SaveUserLogInDeatils(int userId, string sessionKey)
        {
            var sQuery = @"INSERT INTO public.ohd_user_log(
	                       user_id, login_status, login_at, session_key)
                          VALUES (@UserId,@LoginStatus,@LogIntime,@SessionKey)";
            var parameters = new DynamicParameters();

            parameters.Add("@UserId", userId);
            parameters.Add("@LoginStatus", true);
            parameters.Add("@LogIntime", DateTime.UtcNow);
            parameters.Add("@SessionKey", sessionKey);
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task UpdateUserLogOutDeatils(int userId, string sessionKey)
        {
            var sQuery = @"UPDATE public.ohd_user_log
	                         SET login_status=@LoginStatus
                                ,logout_at=@LogOutTime    
                                ,session_key=null    
                             WHERE user_id=@UserId AND session_key!=@SessionKey;";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@LoginStatus", false);
            parameters.Add("@LogOutTime", DateTime.UtcNow);
            parameters.Add("@SessionKey", sessionKey);
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task LogOutUsersSessions(int userId, string sessionKey)
        {
            var sQuery = @"UPDATE public.ohd_user_log
	                         SET login_status=@LoginStatus
                                ,logout_at=@LogOutTime  
                                ,session_key=null
                             WHERE user_id=@UserId and session_key=@SessionKey; ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@LoginStatus", false);
            parameters.Add("@LogOutTime", DateTime.UtcNow);
            parameters.Add("@SessionKey", sessionKey);
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task LogOutUsersCurrentSession(int userId, string sessionKey)
        {
            var sQuery = @"UPDATE public.ohd_user_log
	                         SET login_status=@LoginStatus
                                ,logout_at=@LogOutTime  
                                ,session_key=null
                             WHERE user_id=@UserId; ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@LoginStatus", false);
            parameters.Add("@LogOutTime", DateTime.UtcNow);
            parameters.Add("@SessionKey", sessionKey);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task LogOutUserSessionDeivcetoken(int userId)
        {
            var sQuery = @"DELETE FROM public.ohd_user_device_token
                             WHERE user_id=@UserId ";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
          
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<string> GenerateSessionKey(string numericCharacter)
        {
            Random TransactionNumner = new Random();
            int number = TransactionNumner.Next(10000, 99999);
            string digits = number.ToString();
            string strTransactionNumber = "";
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString("ddMMyy");
            strTransactionNumber = "UR" + date + numericCharacter + digits;
            return strTransactionNumber;
        }

        public async Task<int> IsSessionKeyExist(string sessionKey)
        {

            var sQuery = @"SELECT count(id) from public.ohd_user_log                              
                           WHERE session_key=@SessionKey;";
            var parameters = new DynamicParameters();
            parameters.Add("@SessionKey", sessionKey);
            int count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }
        public async Task<int> UpdateUserContacts(string contact, int userId, bool isEmail)
        {
            var sQuery = "";
            if (isEmail)
            {
                sQuery = @" Update ohd_user  
                            Set email=@Contact,
                            modified_at=@Date
                          WHERE id=@UserId ;
                          Select Id from ohd_user 
                            WHERE id=@UserId ";
            }
            else
            {
                sQuery = @" Update ohd_user  
                            Set mobile=@Contact,
                            modified_at=@Date
                          WHERE id=@UserId ;
                          Select Id from ohd_user 
                            WHERE id=@UserId ";
            }
            var parameters = new DynamicParameters();
            parameters.Add("@Contact", contact);
            parameters.Add("@UserId", userId);
            parameters.Add("@Date", DateTime.UtcNow);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> VeirfyUserById(int userId)
        {
            var updateUserQuery = @"UPDATE public.ohd_user
                                 SET status_id=@StatusId,
                                    isverified=true,
                                    modified_at = @ModifiedAt
                                    WHERE Id =@Id;                         
                                    SELECT (id) from ohd_user WHERE id=@Id;";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", userId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            var result = await _genericRepository.ExecuteScalarAsync<int>(updateUserQuery, parameters);
            return result;
        }
        public async Task LogOutUsersAllSessions(int userId)
        {
            var sQuery = @"UPDATE public.ohd_user_log
	                         SET login_status=@LoginStatus
                                ,logout_at=@LogOutTime  
                                ,session_key=null
                             WHERE user_id=@UserId; ";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@LoginStatus", false);
            parameters.Add("@LogOutTime", DateTime.UtcNow);
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task LogOutUsersDeivcetoken(int userId)
        {
            var sQuery = @"DELETE FROM public.ohd_user_device_token
                             WHERE user_id=@UserId;";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<DeviceTokensDto>> GetDeviceTokens()
        {
            var sQuery = @" SELECT device_token Token,
                            created_at AS CreatedAt,
                            modified_at AS LastActive
                            FROM public.ohd_user_device_token";
            var parameters = new DynamicParameters();

            var result = await _genericRepository.GetAsync<DeviceTokensDto>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<IEnumerable<int>> GetActiveUserIds(List<int> userIds)
        {
            var sQuery = @" SELECT id FROM public.ohd_user
	                      WHERE id=ANY(@UserIds) AND status_id=@Active order by id desc";
            var parameters = new DynamicParameters();
            parameters.Add("@UserIds", userIds);
            parameters.Add("@Active", (int)StatusEnum.Active);

            var result = await _genericRepository.GetAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<int> RegisterBiometric(RegisterBiometricRequest request)
        {
            var sQuery = @"INSERT INTO public.ohd_user_biometrics
                        (
	                     user_id, 
	                     device_id, 
	                     public_key,
	                     created_at
                        )
	                    VALUES 
                        (@UserId, 
			              @DeviceId, 
			              @PublicKey, 
			              @CreatedAt)
                        RETURNING  id;";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", request.UserId);
            parameters.Add("@DeviceId", request.DeviceId);
            parameters.Add("@PublicKey", request.PublicKey);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task<UserBiometric> GetUserBiometricByDeviceIdUserId(string deviceId, int userId)
        {
            try
            {
                var sQuery = @"SELECT  user_id AS UserId,device_id AS DeviceId,
                            public_key AS PublicKey,
                            created_at AS CreatedAt
                            FROM public.ohd_user_biometrics
                            WHERE user_id=@UserId AND device_id=@DeviceId";
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@DeviceId", deviceId);
                var result = await _genericRepository.GetFirstOrDefaultAsync<UserBiometric>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch { return new UserBiometric(); }
        }
        public async Task<LoginResult> IsBiometricUserExist(GetUserByEmailQuery model)
        {

            var sQuery = @"SELECT ur.id as UserId
                            ,ur.Mobile 
                            ,ur.company_id AS CompanyId
                            ,Case when ur.id>0 then 1 else 0 end as IsUserValid
                            ,ur.first_name as firstname
                            ,ur.last_name as lastname
                            ,ur.Email as EmailId
                            ,r.Name as Role
                             --,ur.lockoutenabled
                             -- ,ur.accessfailedcount  
                            ,last_login_date,
                             failed_to_validate as FailedCountAttempted
                           ,failedtovalidate_modified_at as LastAttempted
                           ,isblocked as IsBlocked
                           ,es.name as Status
						   ,ur.comments As Comments 
                            ,ur.isbusiness AS IsBusiness
                            ,ur.accepted_terms_conditions_version AS AcceptedTermConditionVersion
                            ,ur.is_forced_password AS IsForcedPasswordChange
                            --,config.value as IsEstateEnable
                           FROM ohd_user as Ur
                           Join public.ohd_user_role_master as r on Ur.role_id = r.Id
                           JOIN public.ohd_enum_status as es ON ur.status_id=es.id
                             --JOIN public.ohd_configuration AS config ON ur.company_id=config.company_id
                          WHERE (lower(ur.email)=@email or ur.mobile=@email)  and ur.company_id=@companyId and Ur.status_id!=@Inactive";
            //--AND config.name='isestateenable' ";

            var parameters = new DynamicParameters();

            parameters.Add("@email", model.Email.ToLower());
            parameters.Add("@companyId", model.CompanyId);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            try
            {
                return await _genericRepository.GetFirstOrDefaultAsync<LoginResult>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new LoginResult();
            }
        }
        public async Task<int> IsBiometricExist(int userId, string deviceId)
        {
            var sQuery = @"SELECT id FROM public.ohd_user_biometrics
                          WHERE  user_id=@UserId AND
	                     device_id=@DeviceId; ";
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@DeviceId", deviceId);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> UpdateBiometric(int id, string key)
        {
            var sQuery = @"UPDATE  public.ohd_user_biometrics
                          SET public_key=@PublicKey  
                            WHERE id=@Id;
                        SELECT id FROM public.ohd_user_biometrics
                         WHERE id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@PublicKey", key);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        public async Task DeleteUserPermanentById(int userId)
        {
            try
            {
                var sQuery = @"DELETE FROM  public.ohd_user
                          WHERE Id =@Id;";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", userId);
                await _genericRepository.ExecuteScalarAsync(sQuery, parameters);
            }
            catch (Exception ex) { }
        }
        public async Task<int> InsertUser(BulkUsers user, int docId)
        {
            try
            {
                string userQuery = @"
                        INSERT INTO public.ohd_user
                        (
                            title, first_name, last_name, mobile, role_id,
                            email, password, company_id, address_line_1,proof_document_id,
                            tax_number, terms_accepted, accepted_terms_conditions_version,
                            status_id, mobile_country_code,is_bulk_user,is_self_registered
                        )
                        VALUES
                        (
                            @TitleId, @FirstName, @LastName, @Mobile, @RoleId,
                            @Email, @Password, @CompanyId, @AddressLine1,@DocumentId,
                            @TaxNumber, @TermsAccepted, @TermConditionsVersion,
                            @StatusId, @CountryCode,@IsBulkUser,@IsSelfRegistered
                        )
                        RETURNING lastval();";

                var parameters = new DynamicParameters();
                parameters.Add("@TitleId", user.Title);
                parameters.Add("@FirstName", user.FirstName);
                parameters.Add("@LastName", user.LastName);
                parameters.Add("@Mobile", user.Mobile);
                parameters.Add("@RoleId", user.RoleId);
                parameters.Add("@DocumentId", docId);
                parameters.Add("@Email", user.Email);
                parameters.Add("@CompanyId", user.CompanyId);
                parameters.Add("@AddressLine1", user.AddressLine1);
                parameters.Add("@AddressLine1", user.AddressLine1);
                parameters.Add("@TaxNumber", user.TaxNumber);
                parameters.Add("@TermsAccepted", user.TermsAccepted);
                parameters.Add("@TermConditionsVersion", user.TermConditionsVersion);
                parameters.Add("@Password", _encryptionandDecryption.Encrypt("Admin@123"));
                parameters.Add("@StatusId", (int)StatusEnum.Active);
                parameters.Add("@CountryCode", user.CountryCodeId);
                parameters.Add("@IsBulkUser", true);
                parameters.Add("@IsSelfRegistered", false);

                // ── Use existing 2-param overload ✅ ─────────────────────────────
                return await _genericRepository.ExecuteScalarAsync<int>(userQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> InsertUserDocument(int userId, BulkUserDocument doc)
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var docTypes = await documentRepository.GetDocumentTypeMasters().ConfigureAwait(false);
            var docTypesName = docTypes.Select(s => s.Name).ToList();


            int docTypesId = docTypes.ToList().Where(s => s.Name.Equals(doc.DocumentType)).Select(t => t.Id).FirstOrDefault();

            var uploadDocDto = new UploadDocumentDto
            {
                UploadFile = doc.FileDoc,
                DocumentTypeId = docTypesId,
                FileName = userId + "_" + date + "_consumer_identity" + Path.GetExtension(doc.FileDoc.FileName),
                Id = 0,
                Title = doc.FileDoc.FileName,
                DocumentNumber = doc.DocNumber,

            };

            return await _documentRepository.UploadDocument(uploadDocDto).ConfigureAwait(false);
        }

        public async Task<int> BulkInsertUsers(IEnumerable<BulkUsers> users)
        {
            int totalInserted = 0;
            IDbTransaction transaction = null;
            int userId = 0;
            try
            {
                transaction = _genericRepository.TransactionOpen();

                // ✅ Sequential loop — safe with single DB transaction
                foreach (var user in users)
                {
                    try
                    {
                        // ── 1. Insert User ──────────────────────────────────
                        //var userId = await InsertUser(user);

                        var tasks = new List<Task>();
                        int? docId = 0;
                        if (user.UserDocument != null && user.UserDocument.FileDoc != null)
                            docId = await InsertUserDocument(0, user.UserDocument); // userId = 0 placeholder for filename

                        // ── 2. Insert User with docId ──────────────────────────
                        userId = await InsertUser(user, docId.Value);
                        // ── 2. Insert User Document ─────────────────────────
                        if (user.UserDocument != null && user.UserDocument.FileDoc != null)
                            tasks.Add(InsertUserDocument(userId, user.UserDocument));

                        // ── 3. Insert Communication Types ───────────────────
                        if (user.CommunicationTypesIds?.Any() == true)
                            tasks.Add(_communicationRepository.InsertUserCommunications(
                                user.CommunicationTypesIds, userId));

                        // ── 4. Insert Properties → Meters ───────────────────
                        if (user.UserProperties?.Any() == true && user.UserProperties.Count() > 0)
                            tasks.Add(InsertPropertiesParallel(userId, user.UserProperties, user.CompanyId));

                        await Task.WhenAll(tasks);

                        totalInserted++; // ✅ Simple increment, no threading issue
                    }
                    catch (Exception ex)
                    {
                        // ✅ Log individual user failure without stopping all
                        // _logger.LogError(ex, $"Failed to insert user: {user.Email}");
                        throw; // or continue; based on your requirement
                    }
                }

                transaction.Commit();
                return userId;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw;
            }
            finally
            {
                _genericRepository.TransactionClose();
            }
        }

        public async Task InsertPropertiesParallel(int userId, List<PropertyRequestDto> properties, int companyId)
        {
            var propertyTasks = properties.Select(async property =>
            {
                // Map PropertyRequestDto → AddOrUpdatePropertyQuery
                var addPropertyRequest = new AddOrUpdatePropertyQuery
                {
                    Name = property.PropertyName,
                    UnitNumber = property.UnitNumber,
                    EstateId = property.EstateId,
                    AddressLine1 = property.AddressLine1,
                    OwnerId = userId,
                    CompanyId = companyId,
                    StatusId = (int)StatusEnum.Active
                };

                // Reuse existing AddProperty function ✅
                var propertyId = await _propertyRepository.AddProperty(addPropertyRequest).ConfigureAwait(false);

                // Insert Meters linked to this property
                if (property.Meters?.Any() == true && property.Meters.Count() > 0)
                    await InsertMetersParallel(propertyId, property.Meters, companyId);
            });

            await Task.WhenAll(propertyTasks);
        }

        public async Task InsertMetersParallel(int propertyId, List<PropertyMeterRequestDto> meters, int companyId)
        {
            bool isSolar = false;
            var meterTasks = meters.Select(async meter =>
            {
                // ── 1. Upload Contract Document first (if exists) ───────────
                int contractDocumentId = 0;
                if (meter.ContractDocumentUrl != null)
                {
                    var date = DateTime.UtcNow.ToString("yyyyMMdd");
                    var uploadDocDto = new UploadDocumentDto
                    {
                        UploadFile = meter.ContractDocumentUrl,
                        DocumentTypeId = meter.MeterTypeId,
                        FileName = propertyId + "_" + date + "_meter_contract"
                                         + Path.GetExtension(meter.ContractDocumentUrl.FileName),
                        Id = 0,
                        Title = meter.ContractDocumentUrl.FileName,
                        DocumentNumber = meter.MeterNumber
                    };
                    contractDocumentId = await _documentRepository.UploadDocument(uploadDocDto).ConfigureAwait(false);
                }
                var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);
                var meterUrl = _masterApiSetting.BaseUrl + _masterApiSetting.MeterNumberApi + "?meter.meterNum=" + meter.MeterNumber + "&paging=(limit)(5)(offset)(0)";
                var meterResult = await _masterApiConnectService.GetMeter(meterUrl).ConfigureAwait(false);
                string utiltyType = "";
                string utilityName = "";
                utiltyType = meterResult.Data[0].Meter.Model.ServiceResource;
                utilityName = meterResult.Data[0].Meter.Model.Name;

                if (utilityName.Contains("SMART HOT_WATER"))
                {
                    utiltyType = "HOT WATER";
                }
                var utility = meterTypes.FirstOrDefault(t => t.name.ToUpper().Contains(utiltyType));

                // ── 2. Map PropertyMeterRequestDto → AddUpdateMeterQuery ────
                var addMeterRequest = new AddUpdateMeterQuery
                {
                    PropertyId = propertyId,
                    MeterNumber = meter.MeterNumber,
                    MeterTypeId = utility.id,
                    MeterAlias = meter.Alias,
                    DailyTargetConsumption = meter.TargetConsumption,
                    ContractEndDate = meter.ContractEndDate,
                    ContractProofDocumentId = contractDocumentId,
                    StatusId = (int)StatusEnum.Active
                };

                if (meterResult != null && meterResult.Data.Count() > 0)
                {
                    var customerAgreementId = "";
                    var mastercustomerAgreementId = "";
                    customerAgreementId = await _propertyRepository.GetCustomerAgreementId(addMeterRequest.PropertyId).ConfigureAwait(false);
                    var meterdata = meterResult.Data;
                    MeterType type = meterResult.Data[0].Meter.Type;
                    var idExternal = meterResult.Data[0].Meter.IdExternal;
                    if (type != null && type.Id != "STS" && type.Name != "STS Meter" && !idExternal)
                    {
                        isSolar = true;
                    }
                }
                // ── 3. Insert Meter ─────────────────────────────────────────
                var meterId = await _meterRepository.AddMeter(
                    request: addMeterRequest,
                    meterMasterTypeId: meter.MeterTypeId,
                    eftNo: string.Empty,
                    isverified: true
                ).ConfigureAwait(false);

                if (meterResult != null && meterResult.Data.Count() > 0)
                {
                    var customerAgreementId = "";
                    var mastercustomerAgreementId = "";
                    customerAgreementId = await _propertyRepository.GetCustomerAgreementId(addMeterRequest.PropertyId).ConfigureAwait(false);
                    var meterdata = meterResult.Data;
                    MeterType type = meterResult.Data[0].Meter.Type;
                    var idExternal = meterResult.Data[0].Meter.IdExternal;
                    if (type != null && type.Id != "STS" && type.Name != "STS Meter" && !idExternal)
                    {
                        isSolar = true;
                    }
                    foreach (var item in meterdata)
                    {
                        if (item.CustomerAgreement != null)
                        {
                            if (item.CustomerAgreement.Id != "")
                            {
                                mastercustomerAgreementId = item.CustomerAgreement.Id.ToString();
                                if ((customerAgreementId == null || customerAgreementId == "") && mastercustomerAgreementId != "")
                                {
                                    await _propertyRepository.UpdatePropertyCustomerAgreementId(addMeterRequest.PropertyId, mastercustomerAgreementId);

                                }
                            }
                        }
                    }

                }


                // ── 4. Generate EftNo after meter inserted ──────────────────
                if (meterId > 0)
                {
                    var eftNo = await GenerateEFTNumber(meterId).ConfigureAwait(false);

                    // ── 5. Update meter with generated EftNo ────────────────
                    if (!string.IsNullOrWhiteSpace(eftNo))
                    {
                        await _meterRepository.UpdateEFTNumberByMeterId(meterId, eftNo).ConfigureAwait(false);
                    }
                }

            });

            await Task.WhenAll(meterTasks);
        }
        private async Task<string> GenerateEFTNumber(int meterId)
        {

            string strEFTNo = "";
            string strPrecharacterEft = "";
            int eftRandomDigitNumberLength = 0;
            var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
            if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("eftprecharacters")))
            {

                var approvalConfig = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("eftprecharacters"));
                if (approvalConfig != null)
                {
                    strPrecharacterEft = approvalConfig.Value;
                }
                var eftRandomDigitNumber = configurations.FirstOrDefault(t => t.Name.Equals("eftReferenceRandomDigitNumberLength"));
                if (eftRandomDigitNumber != null)
                {
                    eftRandomDigitNumberLength = Convert.ToInt32(eftRandomDigitNumber.Value);
                }
            }

            DateTime currentDate = DateTime.Now;
            string monthNumber = currentDate.ToString("MM");
            string yearNumber = currentDate.ToString("yyyy");

            strEFTNo = strPrecharacterEft.ToLower() + "00" + meterId;
            return strEFTNo;
        }
        public async Task<bool> IsBulkUserEmailExist(string email, int companyId)
        {
            try
            {
                var sQuery = @"SELECT is_bulk_user
                            FROM public.ohd_user 
                          WHERE (lower(email)=@email  OR mobile=@email) AND company_id=@companyId ";
                var parameters = new DynamicParameters();
                parameters.Add("@email", email.ToLower());
                parameters.Add("@companyId", companyId);

                var result = await _genericRepository.ExecuteScalarAsync<bool>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) { return false; }
        }
        public async Task<bool> IsBulkUserMobileExist(string mobile, int companyId)
        {
            var sQuery = @"SELECT is_bulk_user
                            FROM public.ohd_user  
                          WHERE lower(mobile)=@mobile  AND company_id=@companyId ";

            var parameters = new DynamicParameters();


            parameters.Add("@mobile", mobile.ToLower());
            parameters.Add("@companyId", companyId);

            var result = await _genericRepository.ExecuteScalarAsync<bool>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }

        public async Task<int> UpdateBulkRegisterUser(RegisterUserCommand request, int userId)
        {
            try
            {
                var sQuery = @" UPDATE  public.ohd_user SET
	                      mobile_country_code=@countryCodeId,
                                company_id= @companyId,
                                mobile =@mobile,
                                email= @email,
                                password=@password,
                                role_id= @role , 
                                status_id=@statusId,
                                modified_at=@modified_at,
                                isbusiness= @IsBusiness,
                                isverified= @IsVerified,
                                is_self_registered=@IsSelfRegistered
                          WHERE id=@Id;  
                          Select id from public.ohd_user
                          where id=@Id";
                var parameters = new DynamicParameters();
                parameters.Add("@countryCodeId", request.CountryCodeId);
                parameters.Add("@Id", userId);
                parameters.Add("@companyId", request.CompanyId);
                parameters.Add("@mobile", request.MobileNumber);
                parameters.Add("@email", request.EmailId.ToLower());
                parameters.Add("@password", request.Password);
                parameters.Add("@role", (int)RoleMasterEnum.Customer);//as owner
                parameters.Add("@statusId", (int)StatusEnum.InProcess); //pending
                parameters.Add("@created_at", DateTime.UtcNow);
                parameters.Add("@modified_at", DateTime.UtcNow);
                parameters.Add("@IsBusiness", request.Isbusiness);
                parameters.Add("@IsVerified", false);
                parameters.Add("@IsSelfRegistered", true);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> GetBulkUserId(string email, string mobile, int companyId)
        {
            try
            {
                var sQuery = @"SELECT id
                            FROM public.ohd_user 
                          WHERE (lower(email)=@email  AND mobile=@mobile) AND company_id=@companyId AND is_bulk_user=@IsBulkUser";
                var parameters = new DynamicParameters();
                parameters.Add("@email", email.ToLower());
                parameters.Add("@mobile", mobile.ToLower());
                parameters.Add("@IsBulkUser", true);
                parameters.Add("@companyId", companyId);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex) { return 0; }
        }
        public async Task<bool> GetUserRegisteredStatus(string emailMobile, int companyId)
        {
            var sQuery = @"SELECT is_self_registered FROM ohd_user  
                          WHERE (lower(email)=@Email OR mobile=@Email) AND company_id=@CompanyId
                            AND ( status_id=@Active OR status_id=@InProcess) ";
            var parameters = new DynamicParameters();
            parameters.Add("@Email", emailMobile.ToLower());
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@InProcess", (int)StatusEnum.InProcess);
            var result = await _genericRepository.ExecuteScalarAsync<bool>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
    }
    public class UserCommunications
    {
        public int CommId { get; set; }
        public int StatusId { get; set; }
    }
}




