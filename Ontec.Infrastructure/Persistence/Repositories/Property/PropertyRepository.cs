using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.Property.Handler;

namespace Ontec.Infrastructure.Persistence.Property.Repository
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IMeterRepository _meterRepository;
        private readonly IPropertyUserRepository _propertyUserRepository;
        private readonly IWorkContext _workContext;
        public PropertyRepository(IGenericRepository genericRepository
                                  , IMeterRepository meterRepository,
            IPropertyUserRepository propertyUserRepository, IWorkContext workContext)
        {
            _genericRepository = genericRepository;
            _meterRepository = meterRepository;
            _propertyUserRepository = propertyUserRepository;
            _workContext = workContext;
        }

        public async Task<int> UpdateProperty(PropertyModelDto request)
        {
            var sQuery = @"UPDATE ohd_property
                          SET name =@name 
                          ,unit_number=@unit_number
                          ,Owner_Id=@Owner_Id
                          ,company_id=@company_id
                          ,address_line_1=@address_line_1
                         --,address_line_2=@address_line_2
                          -- ,city=@city
                          --,state=@state
                          --,country=@country
                          ,modified_at=@modified_at
                          --,eft_number=@EftNumber
                        ,estate_id=@EstateId
                        WHERE id= @Id;
                        Select Id FROM ohd_property
                        WHERE id= @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@id", request.Id);
            parameters.Add("@name", request.Name);
            parameters.Add("@unit_number", request.UnitNumber);
            parameters.Add("@Owner_Id", request.OwnerId);
            parameters.Add("@company_id", request.CompanyId);
            parameters.Add("@address_line_1", request.AddressLine1);
            parameters.Add("@address_line_2", request.AddressLine2);
            parameters.Add("@city", request.City);
            parameters.Add("@state", request.State);
            parameters.Add("@country", request.Country);
            parameters.Add("@modified_at", DateTime.UtcNow);
            parameters.Add("@EstateId", request.EstateId);
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
        public async Task<int> AddProperty(AddOrUpdatePropertyQuery request)
        {
            try
            {
                var sQuery = @" INSERT INTO ohd_property(name
                                                  ,unit_number
                                                  --,eft_number      
                                                  ,Owner_Id
                                                  ,company_id
                                                  ,status_id
                                                  ,address_line_1
                                                  --,address_line_2
                                                  --,city
						                          --,state
                                                  --,country
                                                  ,created_at
                                                    )
                        VALUES ( @name
                                ,@unit_number
                               -- ,@EFtNo
                                ,@Owner_Id
                                ,@company_id
                                ,@status_id
                                ,@address_line_1
                                --,@address_line_2
                                --,@city
						        --,@state
                                --,@country
                                ,@created_at
                                )
                     RETURNING lastval()";


                var parameters = new DynamicParameters();
                parameters.Add("@name", request.Name);
                // parameters.Add("@EFtNo", eftNumber);
                parameters.Add("@unit_number", request.UnitNumber);
                parameters.Add("@Owner_Id", request.OwnerId);
                parameters.Add("@company_id", request.CompanyId);
                parameters.Add("@status_id", request.StatusId);
                parameters.Add("@address_line_1", request.AddressLine1);
                //parameters.Add("@address_line_2", request.AddressLine2);
                //parameters.Add("@city", request.City);
                //parameters.Add("@state", request.State);
                //parameters.Add("@country", request.Country);
                parameters.Add("@created_at", DateTime.UtcNow);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }
        }
        public async Task<IEnumerable<PropertyDto>> GetPropertiesByOwnerId(GetPropertiesByOwnerIdQuery request)
        {
            var sQuery = @"SELECT distinct p.Id ,
                            COALESCE(m.meter_count, 0) AS MeterCount, 
                            COALESCE(r_count.relation_count, 0) AS TenentCount,
                            p.name as Name,
                            p.unit_number as UnitNumber, 
                            p.address_line_1 as Address , 
                           
                            p.status_id AS StatusId	,
							es.display_value as Status,	
                            CASE WHEN p.owner_id=@OwnerId THEN 'Owner' ELSE role.name END as RoleName,
                            CASE WHEN p.owner_id=@OwnerId THEN 0 else r.id end as PropertyRelationId,
                            to_char(p.created_at,'dd-MM-yyyy') as AddedOn
						 FROM ohd_property AS p
                            LEFT JOIN (select * from ohd_property_user_relation where status_id!=@Inactive) AS r ON p.id = r.property_id
                              LEFT JOIN public.ohd_enum_status as es on p.status_id=es.id
                            LEFT JOIN ohd_enum_user_relation as role on role.id=r.user_relation_id
                           
                            LEFT JOIN (
                                SELECT property_id, COUNT(*) AS meter_count
                                FROM ohd_meter
                                where status_id!=@InActive AND status_id!=@DeActive
                                GROUP BY property_id
                            ) AS m ON p.id = m.property_id
                            LEFT JOIN (
                                SELECT property_id, COUNT(*) AS relation_count
                                FROM ohd_property_user_relation
                                where status_id=@StatusId
                                GROUP BY property_id
                            ) AS r_count ON p.id = r_count.property_id
                            WHERE (r.user_id = @OwnerId OR p.owner_id =@OwnerId)AND p.status_id !=@DeActive";

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@OwnerId", request.UserId);
            if (request.InActive.HasValue)
            {
                sQuery += " AND (p.status_id = @StatusId OR p.status_id=@InActiveProperty)";
                parameters.Add("@InActiveProperty", (int)StatusEnum.Inactive);
            }
            if (!request.InActive.HasValue)
            {
                sQuery += " AND (p.status_id = @StatusId)";
            }
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@InActive", (int)StatusEnum.Inactive);
            parameters.Add("@DeActive", (int)StatusEnum.Deactive);

            sQuery += " ORDER BY p.Id desc";
            var properties = await _genericRepository.GetAsync<PropertyDto>(sQuery, parameters).ConfigureAwait(false);

            foreach (var property in properties)
            {
                var meterTypes = await _meterRepository.GetMeterTypesByPropertyId(property.Id).ConfigureAwait(false);
                if (meterTypes != null)
                    property.MeterTypeList = meterTypes.Distinct().ToList();
            }
            return properties;
        }
        public async Task<PropertyModelDto> GetPropertyById(int propertyId)
        {
            try
            {
                var sQuery = @"SELECT per.Id, per.name AS Name,per.unit_number AS UnitNumber
                           ,per.Owner_Id as OwnerId
                           ,per.Company_Id as CompanyId
                           ,per.Status_Id as StatusId
                           ,per.address_line_1 as AddressLine1,per.address_line_2 as AddressLine2,
                            per.city,
                            per.state,
                            per.country              
                           FROM ohd_property as per
                           
                           WHERE per.id=@id and per.Status_Id!=@StatusId";
                var parameters = new DynamicParameters();
                parameters.Add("@id", propertyId);
                parameters.Add("@StatusId", (int)StatusEnum.Inactive);

                var result = await _genericRepository.GetFirstOrDefaultAsync<PropertyModelDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<PropertyModelDto> GetAllPropertyById(int propertyId)
        {
            try
            {
                var sQuery = @"SELECT per.Id, per.name AS Name,per.unit_number AS UnitNumber
                           ,per.Owner_Id as OwnerId
                           ,per.Company_Id as CompanyId
                           ,per.Status_Id as StatusId
                           ,per.address_line_1 as AddressLine1,per.address_line_2 as AddressLine2,
                            per.city,
                            per.state,
                            per.country
                           FROM ohd_property as per
                           WHERE per.id=@id";
                var parameters = new DynamicParameters();
                parameters.Add("@id", propertyId);
                parameters.Add("@StatusId", (int)StatusEnum.Inactive);

                var result = await _genericRepository.GetFirstOrDefaultAsync<PropertyModelDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<bool> IsPropertyIdExist(int propertyId)
        {
            var sQuery = @"SELECT Count(Id) from public.ohd_property
                          WHERE Id=@PropertyId and Status_Id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<bool> IsPropertyExist(int propertyId)
        {
            var sQuery = @"SELECT Count(Id) from public.ohd_property
                          WHERE Id=@PropertyId";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> DeletePropertyById(DeletePropertyById request)
        {
            var parameters = new DynamicParameters();
            int propertyId = 0;
            if (request.IsRestore)
            {
                var propertyQuery = @"Update public.ohd_property
                         Set status_id=@Active ,
                        modified_at = @ModifiedAt
                         WHERE id=@PropertyId;
                        SELECT id FROM public.ohd_property
                        WHERE id=@PropertyId";
                parameters.Add("@PropertyId", request.Id);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@Active", (int)StatusEnum.Active);
                propertyId = await _genericRepository.ExecuteScalarAsync<int>(propertyQuery, parameters);

            }
            if (request.IsPermanentDelete)
            {
                var sQuery = @"Update public.ohd_meter
                         Set status_id=@StatusId, 
                        modified_at = @ModifiedAt                        
                         where property_id=@PropertyId";

                parameters.Add("@PropertyId", request.Id);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                parameters.Add("@DeActive", (int)StatusEnum.Deactive);


                await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
                var propertyQuery = @"Update public.ohd_property
                         Set status_id=@DeActive ,
                        customer_agreement_id=null,
                        modified_at = @ModifiedAt
                         WHERE id=@PropertyId;
                        SELECT id FROM public.ohd_property
                        WHERE id=@PropertyId";
                propertyId = await _genericRepository.ExecuteScalarAsync<int>(propertyQuery, parameters).ConfigureAwait(false);



            }
            if (!request.IsPermanentDelete && !request.IsRestore)
            {
                var propertyQuery = @"Update public.ohd_property
                         Set status_id=@InActive ,
                        modified_at = @ModifiedAt
                         where id=@PropertyId;
                        SELECT id FROM public.ohd_property
                        WHERE id=@PropertyId";
                parameters.Add("@PropertyId", request.Id);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@InActive", (int)StatusEnum.Inactive);
                propertyId = await _genericRepository.ExecuteScalarAsync<int>(propertyQuery, parameters);

            }

            return propertyId;
        }
        public async Task<bool> IsUnitNumberExist(string unitNumber, int propertyId)
        {
            var sQuery = @" Select Count(Id) FROM public.ohd_property
                          WHERE Id!=@PropertyId and unit_number=@UnitNumber";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@UnitNumber", unitNumber);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> GetPropertyUsersCountByPropertyId(int propertyId, int propertyUserTypeId)
        {
            var sQuery = @"select count (ID) from ohd_Property_User_Relation 
                        where property_id=@PropertyId and user_relation_id = @PropertyUserTypeId and status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@PropertyUserTypeId", propertyUserTypeId);
            parameters.Add("@Active", (int)StatusEnum.Active);

            var count = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }
        public async Task<IEnumerable<ConsumptionPropertyList>> GetConsumptionPropertyList(int userId, bool isAdmin, int consumerId)
        {

            var sQuery = @"SELECT distinct p.id as PropertyId, p.name as PropertyName,p.unit_number AS UnitNumber
	                      FROM public.ohd_property AS p
						  LEFT JOIN (select * from public.ohd_property_user_relation where status_id=@StatusId) AS pur ON pur.property_id=p.id
	                      WHERE p.status_id=@StatusId AND P.status_id!=@Deactive ";

            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@Deactive", (int)StatusEnum.Deactive);

            if (!isAdmin)
            {
                sQuery += " and (p.Owner_Id=@Owner OR pur.user_id=@userId)";
                parameters.Add("@Owner", userId);
                parameters.Add("@UserId", userId);
            }
            else
            {
                sQuery += " and (p.Owner_Id=@ConsumerId OR pur.user_id=@ConsumerId)";
                parameters.Add("@ConsumerId", consumerId);

            }
            sQuery += " Order By name";


            var propertyList = await _genericRepository.GetAsync<ConsumptionPropertyList>(sQuery, parameters).ConfigureAwait(false);

            var meterQuery = @"SELECT M.meter_number as MeterId,M.meter_alias as Name, lower(em.Name) as UtilityType  
                              FROM public.ohd_meter as M 
                              Join public.ohd_enum_meter_type as em on M.meter_type_id=em.Id
	                          WHERE M.property_id=@PropertyId and M.Status_Id=@StatusId
                              Order By M.meter_alias";
            parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            var peropertyMeterList = new List<ConsumptionPropertyList>();

            foreach (var item in propertyList)
            {
                parameters.Add("@PropertyId", item.PropertyId);
                var meterList = await _genericRepository.GetAsync<MeterSelectList>(meterQuery, parameters).ConfigureAwait(false);
                if (meterList.Any())
                {
                    item.MeterList = meterList.ToList();
                    peropertyMeterList.Add(item);
                }
            }
            return peropertyMeterList;
        }
        public async Task<IEnumerable<string>> GetMeterNumbersByPropertyId(int propertyId)
        {
            var sQuery = @"Select mt.meter_number from public.ohd_property as per
                            LEFT JOIN public.ohd_meter as mt on per.Id=mt.property_id
                            where per.Id=@PropertyId AND per.status_id=@Active AND mt.status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@Active", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<PropertyDetailsDto> GetPropertyDashboard(int userId)
        {
            PropertyDetailsDto dashboardDto = new();
            try
            {
                var totalPropertyCountQuery = @"SELECT COUNT(DISTINCT p.Id) FROM public.ohd_property as P 
                                                LEFT JOIN (select * from public.ohd_property_user_relation where status_id=@StatusId)as Pur on P.Id= Pur.property_id
                                                WHERE (P.owner_id=@UserId or pur.user_id=@UserId) AND P.status_id=@StatusId AND P.status_id!=@Deactive ";

                var totalMeterInstalledCountQuery = @"SELECT Count(Id) FROM public.ohd_meter 
                                                      WHERE property_id IN (
                                                      SELECT DISTINCT p.Id FROM public.ohd_property as P 
                                                      LEFT JOIN (select * from public.ohd_property_user_relation where status_id=@StatusId) as Pur on P.Id= Pur.property_id 
                                                      WHERE (P.owner_id=@UserId or pur.user_id=@UserId) AND P.status_id=@StatusId AND P.status_id!=@Deactive )
                                                      AND status_id!=@InActive";

                var totaltenantAsscociateCountQuery = @"SELECT count(pur.id) AS UsersCount
                                                      FROM public.ohd_property AS per
                                                      LEFT JOIN (select * from public.ohd_property_user_relation where status_id=@StatusId) AS pur ON pur.property_id=per.id
                                                      WHERE (per.owner_id=@UserId OR pur.user_id=@UserId ) AND per.status_id=@StatusId AND per.status_id!=@Deactive ";

                var parameter = new DynamicParameters();
                parameter.Add("@UserId", userId);
                parameter.Add("@StatusId", (int)StatusEnum.Active);
                parameter.Add("@InActive", (int)StatusEnum.Inactive);
                parameter.Add("@Deactive", (int)StatusEnum.Deactive);



                var totalPropertyCount = _genericRepository.GetFirstOrDefaultAsync<int>(totalPropertyCountQuery, parameter);
                var totalMeterInstalledCount = _genericRepository.GetFirstOrDefaultAsync<int>(totalMeterInstalledCountQuery, parameter);
                var totaltenantAsscociateCount = _genericRepository.GetFirstOrDefaultAsync<int>(totaltenantAsscociateCountQuery, parameter);


                await Task.WhenAll(totalPropertyCount, totalMeterInstalledCount, totaltenantAsscociateCount).ConfigureAwait(false);
                dashboardDto.TotalPropertycount = totalPropertyCount.Result;
                dashboardDto.TotalMeterInstalledCount = totalMeterInstalledCount.Result;
                dashboardDto.TotalTenantAssociateCount = totaltenantAsscociateCount.Result;
            }
            catch (Exception ex) { }

            return dashboardDto;

        }
        #region TransactionMaster
        public async Task<IEnumerable<TransactionPropertyList>> GetTransactionPropertyList(int userId, bool isAdmin)
        {


            var result = new List<TransactionPropertyList>();
            var associateUserQuery = @"SELECT distinct mt.meter_number as MeterNumber,
                                   
                            mt.id as MeterId,
                            mt.meter_type_id AS MeterTypeId, 
                            emt.name As MeterType,		
                            concat (per.name,' ',emt.name,' ','meter', ' ',mt.meter_number) as PropertyName
                            ,per.name As PropName
                            ,LOWER(mt.eft_number) AS EFTNo
                            ,per.id AS PropertyId 
                            --,CASE when aus.allow_top_up=1 then 1 else 0 end  AS AllowTopUp
                        ,CASE WHEN aus.allow_top_up is null then 0 else aus.allow_top_up END as AllowTopUp
							,puer.user_relation_id AS PropertyRelationId
                        FROM public.ohd_property AS per
                        LEFT JOIN (select * from public.ohd_property_user_relation where status_id=@StatusId) AS puer ON puer.property_id=per.id
                        LEFT JOIN public.ohd_associate_user_settings as aus on puer.id=aus.property_user_id
                        LEFT JOIN ohd_meter AS mt ON mt.property_id=per.id
                        LEFT JOIN public.ohd_enum_meter_type AS emt ON emt.id=mt.meter_type_id
                        WHERE per.status_id =@StatusId AND 
                                mt.status_id=@StatusId AND
                               ( puer.user_id=@OnwerId) AND
                               (aus.allow_top_up is null or aus.allow_top_up=1) 
                        Order By mt.meter_number";

            var ownerQuery = @"SELECT distinct mt.meter_number as MeterNumber,
                            mt.id as MeterId,
                            mt.meter_type_id AS MeterTypeId, 
                             emt.name As MeterType,		
                            concat (per.name,' ',emt.name,' ','meter', ' ',mt.meter_number) as PropertyName
                            ,per.name As PropName
                            ,mt.eft_number AS EFTNo
                            ,per.id AS PropertyId 
                            ,1 AS AllowTopUp
							,0 AS PropertyRelationId
                        FROM public.ohd_property AS per
                        LEFT JOIN ohd_meter AS mt ON mt.property_id=per.id
                        LEFT JOIN public.ohd_enum_meter_type AS emt ON emt.id=mt.meter_type_id
                        WHERE per.status_id =@StatusId AND 
                                mt.status_id=@StatusId AND
                               (per.owner_id=@OnwerId ) 
                        Order By mt.meter_number";

            var tenantQuery = @"SELECT distinct mt.meter_number as MeterNumber,
                                   
                            mt.id as MeterId,
                            mt.meter_type_id AS MeterTypeId, 
                            concat (per.name,' ',emt.name,' ','meter', ' ',mt.meter_number) as PropertyName
                            ,per.name As PropName
                            ,mt.eft_number AS EFTNo
                            ,per.id AS PropertyId 
                            ,true AS AllowTopUp
                           -- ,CASE WHEN aus.allow_top_up is null then 0 else aus.allow_top_up END as AllowTopUp
                            --,CASE when aus.allow_top_up=1 then 1 else 0 end  AS AllowTopUp
							,puer.user_relation_id AS PropertyRelationId
                        FROM public.ohd_property AS per
                        LEFT JOIN (select * from public.ohd_property_user_relation where
								   status_id=@StatusId) AS puer ON puer.property_id=per.id
                        
                        LEFT JOIN ohd_meter AS mt ON mt.property_id=per.id
                        LEFT JOIN public.ohd_enum_meter_type AS emt ON emt.id=mt.meter_type_id
                        WHERE per.status_id =@StatusId AND 
                                mt.status_id=@StatusId AND
                               ( puer.user_id=@OnwerId AND puer.user_relation_id=@PropertyUserType)
                              
                        Order By mt.meter_number";

            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            parameters.Add("@PropertyUserType", (int)PropertyUserRelationEnum.Tenant);
            parameters.Add("@OnwerId", userId);
            var associatePropertiesTask = _genericRepository.GetAsync<TransactionPropertyList>(associateUserQuery, parameters);
            var ownerPropertiesTask = _genericRepository.GetAsync<TransactionPropertyList>(ownerQuery, parameters);
            var tenantPropertiesTask = _genericRepository.GetAsync<TransactionPropertyList>(tenantQuery, parameters);

            try
            {
                await Task.WhenAll(associatePropertiesTask, ownerPropertiesTask, tenantPropertiesTask).ConfigureAwait(false);
                int serialNo = 0;
                var associateProperties = await associatePropertiesTask;
                var ownerProperties = await ownerPropertiesTask;
                var tenantProperties = await tenantPropertiesTask;
                if (ownerProperties != null)
                {
                    //foreach (var p in ownerProperties)
                    //{

                    //    string eft = p.EFTNo;

                    //    p.EFTNo = p.EFTNo.Insert(2, "00");


                    //}
                }
                if (associateProperties != null)
                {
                    foreach (var p in associateProperties)
                    {
                        serialNo = await _propertyUserRepository.GetSerialNumberByPropertyUserId(p.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                        string eft = p.EFTNo;

                        if (serialNo < 10)
                        {
                            int index = eft.IndexOf('0', eft.IndexOf('0') + 1);
                            if (index >= 0)
                            {
                                eft = eft.Remove(index, 1).Insert(index, serialNo.ToString()); // replace with 'X'
                            }
                            p.EFTNo = eft;
                            //p.EFTNo = p.EFTNo.Insert(2, "0" + serialNo.ToString());
                        }
                        else
                        {
                            int index = eft.IndexOf('0', eft.IndexOf('0') + 1);
                            if (index >= 0)
                            {
                                eft = eft.Remove(index, 1).Insert(index, serialNo.ToString()); // replace with 'X'
                            }
                            p.EFTNo = eft;
                            //p.EFTNo = p.EFTNo.Insert(2, serialNo.ToString());
                        }
                    }
                }
                if (tenantProperties != null)
                {
                    foreach (var p in tenantProperties)
                    {
                        serialNo = await _propertyUserRepository.GetSerialNumberByPropertyUserId(p.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                        string eft = p.EFTNo;

                        if (serialNo < 10)
                        {
                            int index = eft.IndexOf('0', eft.IndexOf('0') + 1);
                            if (index >= 0)
                            {
                                eft = eft.Remove(index, 1).Insert(index, serialNo.ToString()); // replace with 'X'
                            }
                            p.EFTNo = eft;
                            //p.EFTNo = p.EFTNo.Insert(2, "0" + serialNo.ToString());
                        }
                        else
                        {
                            int index = eft.IndexOf('0', eft.IndexOf('0') + 1);
                            if (index >= 0)
                            {
                                eft = eft.Remove(index, 1).Insert(index, serialNo.ToString()); // replace with 'X'
                            }
                            p.EFTNo = eft;
                            // p.EFTNo = p.EFTNo.Insert(2, serialNo.ToString());
                        }
                    }
                }
                result.AddRange(tenantProperties);
                result.AddRange(associateProperties);
                result.AddRange(ownerProperties);



                result = result.GroupBy(t => t.MeterNumber).Select(g => g.First()).ToList();

            }

            catch (Exception ex) { }

            return result.Distinct();
        }
        #endregion
        public async Task<IEnumerable<DashboardMastersDto>> GetDashboardMastersDtoByUserId(int userId)
        {

            var sQuery = @"SELECT DISTINCT 
                            p.Id AS PropertyId,
                            p.unit_number AS UnitNumber,
                            p.name AS PropertyName
                            --p.eft_number AS EFTNumber 
                        FROM 
                            public.ohd_property AS p 
                        LEFT JOIN 
                            (select *  from public.ohd_property_user_relation where status_id=1) AS pur ON p.Id = pur.property_id
                        LEFT JOIN 
                            public.ohd_meter AS mt ON p.id = mt.property_id
                        WHERE 
                            p.status_id = @StatusId
                            AND mt.status_id = @StatusId AND (p.owner_id=@UserId or pur.user_id=@UserId)
                        GROUP BY 
                            p.Id, p.unit_number, p.name
                        HAVING 
                            COUNT(mt.id) > 0
                        ORDER BY 
                            p.name;";

            var parameter = new DynamicParameters();
            parameter.Add("@UserId", userId);
            parameter.Add("@StatusId", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<DashboardMastersDto>(sQuery, parameter).ConfigureAwait(false);
        }
        public async Task<PropertyUserDetail> GetPropertyUserByMeterId(int propertyId)
        {
            var sQuery = @"SELECT distinct  per.id as PropertyId,
                            concat(ur.first_name,' ',ur.last_name) as Owner,
                            per.name AS Complex,   
                            concat(per.address_line_1,'',per.address_line_2) AS Address,
                            ur. email AS Email,
                            ur.mobile AS ContactNo,       
                            ur.tax_number AS TaxNumber,	
                            c.companylogourl as CompanyLogo                            
                            FROM public.ohd_property AS per                       
                            LEFT JOIN public.ohd_meter As mt ON mt.property_id=per.id
                            LEFT JOIN public.ohd_user AS ur ON ur.id= per.owner_id
                            LEFT JOIN public.ohd_company c on ur.company_id=c.id
                            WHERE per.status_id =@StatusId
                            AND  per.id=@PropertyId";
            var parameter = new DynamicParameters();
            // parameter.Add("@MeterId",meterId);
            parameter.Add("@PropertyId", propertyId);
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var result = await _genericRepository.GetFirstOrDefaultAsync<PropertyUserDetail>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UpdatePropertyCustomerAgreementId(int propertyId, string CustomerAgreementId)
        {
            var sQuery = @"UPDATE ohd_property
                          SET customer_agreement_id =@CustomerAgreementId 
                          ,modified_at=@modified_at
                        WHERE id= @Id;
                        Select Id FROM ohd_property
                        WHERE id= @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@id", propertyId);
            parameters.Add("@CustomerAgreementId", CustomerAgreementId);
            parameters.Add("@modified_at", DateTime.UtcNow);
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
        public async Task<string> GetCustomerAgreementId(int propertyId)
        {

            var sQuery = @"SELECT CASE WHEN customer_agreement_id is NULL THEN '' ELSE customer_agreement_id END
                            FROM
                            public.ohd_property
                            WHERE id= @Id";

            var parameters = new DynamicParameters();
            parameters.Add("@id", propertyId);

            var result = await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return result;


        }

        public async Task<PropertyCustomerAgreementDto> IsCustomerAgreementIdExist(string CustAgrrementId)
        {
            var sQuery = @" Select Count(customer_agreement_id) AS CustomerAgreementIdCount,id AS PropertyId FROM public.ohd_property
                          WHERE customer_agreement_id=@CustAgreementId AND status_id=@Active
                            Group By id";
            var parameters = new DynamicParameters();
            parameters.Add("@CustAgreementId", CustAgrrementId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            return await _genericRepository.GetFirstOrDefaultAsync<PropertyCustomerAgreementDto>(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<int> UpdateCustomerAgreementId(int propertyId)
        {
            var sQuery = @"Update public.ohd_Property
                         Set 
                        modified_at = @ModifiedAt,
                        customer_agreement_id=null
                         where id=@PropertyId;
                        SELECT id FROM public.ohd_Property
                         where id=@PropertyId;";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters);
            return id;
        }
        public async Task<DatatableModel<PropertyDto>> GetAllProperties(GetAllPropertiesRequestQuery request)
        {
            var dt = new DatatableModel<PropertyDto>()
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
                var sQuery = @"SELECT 
                              DISTINCT
                              p.id,
                              CONCAT(u.first_name, ' ', u.last_name) AS Owner,
                              p.name AS Name,
                              p.unit_number AS UnitNumber,
                              STRING_AGG(DISTINCT mt.meter_number, ', ') AS MeterNumbers,
                              CONCAT(COUNT(DISTINCT per.user_id), ' Users ', ' ',
                              COUNT(DISTINCT mt.id) ,' Meters')  AS Sources,
                              p.address_line_1 AS Address,                             
                              p.status_id AS StatusId,
                              es.display_value AS Status
                            FROM 
                              ohd_property AS p
                              LEFT JOIN public.ohd_enum_status AS es ON p.status_id = es.id                             
                              LEFT JOIN ohd_user AS u ON p.owner_id = u.id
                              LEFT JOIN ohd_meter AS mt ON p.id = mt.property_id  AND mt.status_id=@Active
                              LEFT JOIN  public.ohd_property_user_relation AS per ON p.id = per.property_id  AND per.status_id=@Active
                            WHERE p.status_id!=@DeActive";

                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@DeActive", (int)StatusEnum.Deactive);
                if (request.StatusId.HasValue && request.StatusId.Value != 0)
                {
                    sQuery += " AND p.status_id =@StatusId ";
                    parameters.Add("@StatusId", request.StatusId);

                }
                if (request.EstateId.HasValue && request.EstateId.Value != 0)
                {
                    sQuery += " AND p.estate_id =@EstateId ";
                    parameters.Add("@EstateId", request.EstateId);

                }
                //parameters.Add("@PageSize", request.PageSize);
                //parameters.Add("@Offset", request.Page * request.PageSize);


                parameters.Add("@Active", (int)StatusEnum.Active);
                

                if (!string.IsNullOrWhiteSpace(request.SearchText) && request.SearchText.Trim() != "string")
                {
                    var searchTerms = request.SearchText
                                            .Trim()
                                            .ToLower()
                                            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    int index = 0;

                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        parameters.Add(paramName, "%" + term + "%");

                        // For each term, create OR block (term must match any column)
                        sQuery += $@"
                                    AND (
                                            LOWER(p.name) ILIKE {paramName}
                                         OR LOWER(p.unit_number) ILIKE {paramName}
                                         OR LOWER(p.address_line_1) ILIKE {paramName}
                                         OR LOWER(mt.meter_number) ILIKE {paramName}
                                        )";

                        index++;
                    }

                }
                sQuery += " GROUP BY  p.id, CONCAT(u.first_name, ' ', u.last_name), p.name, p.unit_number,  p.address_line_1, p.estate_id,  p.status_id, es.display_value";
                sQuery += " ORDER BY p.status_id asc";
                // sQuery += " LIMIT @PageSize OFFSET @Offset ";
                var properties = await _genericRepository.GetAsync<PropertyDto>(sQuery, parameters).ConfigureAwait(false);


                if (request.PageSize > 0)
                {
                    var result = properties.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                    dt.Data = result.ToList();
                    dt.TotalRecords = properties.Count();
                }
                else
                {
                    dt.Data = properties.ToList();
                    dt.TotalRecords = properties.Count();
                }
            }
            catch (Exception ex)
            {

            }
            return dt;
        }

        public async Task<DatatableModel<PropertyDto>> GetAllPropertiesNew(GetAllPropertiesRequestQuery request)
        {
            var dt = new DatatableModel<PropertyDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };


            // Order by with columns
            if (string.IsNullOrEmpty(request.order) || request.order == "string")
                request.order = "desc";

            var parameters = new DynamicParameters();
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@DeActive", (int)StatusEnum.Deactive);

            // common query  for count & data
            string baseQuery = @"
                                    FROM ohd_property p
                                    LEFT JOIN public.ohd_enum_status es ON p.status_id = es.id
                                   -- LEFT JOIN ohd_estate e ON p.estate_id = e.id
                                    LEFT JOIN ohd_user u ON p.owner_id = u.id
                                    LEFT JOIN ohd_meter mt ON p.id = mt.property_id AND mt.status_id = @Active
                                    LEFT JOIN public.ohd_property_user_relation per ON p.id = per.property_id AND per.status_id = @Active
                                    --WHERE p.status_id != @DeActive
                     ";

            // filter according conditions
            string whereClause = string.Empty;
            // filter according conditions
            if (request.StatusId.HasValue && request.StatusId.Value != 0)
            {
                whereClause += " WHERE p.status_id = @StatusId ";
                parameters.Add("@StatusId", request.StatusId);
            }
            if (request.EstateId.HasValue && request.EstateId.Value != 0)
            {
                if (!string.IsNullOrEmpty(whereClause))
                {

                    whereClause += " AND p.estate_id = @EstateId ";
                    parameters.Add("@EstateId", request.EstateId);
                }
                else
                {

                }
                whereClause += " WHERE p.estate_id = @EstateId ";
                parameters.Add("@EstateId", request.EstateId);

            }
            // search functionality conditions
            if (!string.IsNullOrWhiteSpace(request.SearchText) && request.SearchText.Trim() != "string")
            {
                var terms = request.SearchText.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int index = 0;
                foreach (var t in terms)
                {
                    string param = "@Search" + index;
                    parameters.Add(param, "%" + t + "%");

                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        whereClause += $@" AND (
                                        LOWER(u.first_name) ILIKE {param}
                                         OR LOWER(u.last_name) ILIKE {param}
                                         OR LOWER(p.name) ILIKE {param}
                                         OR LOWER(p.unit_number) ILIKE {param}
                                         OR LOWER(p.address_line_1) ILIKE {param}
                                         OR LOWER(mt.meter_number) ILIKE {param}
                                       )";
                    }
                    else
                    {
                        whereClause += $@" WHERE (
                                        LOWER(u.first_name) ILIKE {param}
                                         OR LOWER(u.last_name) ILIKE {param}
                                         OR LOWER(p.name) ILIKE {param}
                                         OR LOWER(p.unit_number) ILIKE {param}
                                         OR LOWER(p.address_line_1) ILIKE {param}
                                         OR LOWER(mt.meter_number) ILIKE {param}
                                       )";

                    }
                    index++;
                }
            }
            if (!string.IsNullOrEmpty(whereClause))
            {
                baseQuery += whereClause;
            }

            // Total count query
            string countQuery = $"SELECT COUNT(DISTINCT p.id) {baseQuery}";
            int total = await _genericRepository.ExecuteScalarAsync<int>(countQuery, parameters);

            // Records with pagination query
            string dataQuery = @"
                                SELECT DISTINCT
                                p.id,
                                CONCAT(u.first_name, ' ', u.last_name) AS Owner,
                                p.name AS Name,
                                p.unit_number AS UnitNumber,
                                STRING_AGG(DISTINCT mt.meter_number, ', ') AS MeterNumbers,
                                CONCAT(COUNT(DISTINCT per.user_id), ' Users ', ' ',
                                       COUNT(DISTINCT mt.id), ' Meters') AS Sources,
                                p.address_line_1 AS Address,                               
                                p.status_id AS StatusId,
                                es.display_value AS Status
                                " +
                            baseQuery +
                            @"
                                GROUP BY 
                                    p.id, Owner, p.name, p.unit_number, p.address_line_1,
                                     p.status_id, es.display_value
                                ORDER BY p.status_id ASC
                                LIMIT @PageSize OFFSET @Offset
                                ";

            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@Offset", request.Page * request.PageSize);

            var rows = await _genericRepository.GetAsync<PropertyDto>(dataQuery, parameters);

            dt.TotalRecords = total;
            dt.Data = rows.ToList();
            return dt;
        }
        public async Task<IEnumerable<PropertyMeter>> GetAllMeterNumbersByPropertyId(int propertyId)
        {
            try
            {
                var sQuery = @"Select mt.meter_number AS MeterNumber, mt.id AS MeterId,
                             es.display_value as MeterStatus,
                            mt.status_id AS MeterStatusId,
							per.unit_number  AS PropertyUnitNumber
                            from public.ohd_property as per
                            LEFT JOIN public.ohd_meter as mt on per.id=mt.property_id
                            LEFT JOIN public.ohd_enum_status as es ON mt.status_id=es.id
                            where per.id=@PropertyId";
                var parameters = new DynamicParameters();
                parameters.Add("@PropertyId", propertyId);

                return await _genericRepository.GetAsync<PropertyMeter>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<PropertyMeter>();
            }
        }

        public async Task<IEnumerable<LinkedProperty>> GetMeterLinkedProperty(List<string> meterNumbers)
        {
            try
            {
                var sQuery = @"
                            SELECT
                            m.meter_number AS MeterNumber ,
                            p.name as LinkedPropertyName,
                            p.id AS LinkedPropertyId,
                            p.unit_number AS LinkedPropertyUnitNumber,
                            es.display_value AS LinkedPropertyMeterStatus
                            FROM public.ohd_meter  as m                         
                            LEFT JOIN public.ohd_property as p ON m.property_id=p.id
                            LEFT JOIN public.ohd_enum_status as es ON m.status_id=es.id
                            WHERE m.meter_number =ANY(@MeterNumbers) AND( m.status_id=@Active OR m.status_id=@Pending OR m.status_id=@Rejected)";
                var parameters = new DynamicParameters();
                parameters.Add("@MeterNumbers", meterNumbers);
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Pending", (int)StatusEnum.Pending);
                parameters.Add("@Rejected", (int)StatusEnum.Rejected);

                return await _genericRepository.GetAsync<LinkedProperty>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<LinkedProperty>();
            }
        }
        public async Task<int> UpdateMeterStatus(List<int> ids)
        {
            try
            {
                var sQuery = @"
                            WITH updated AS (
                                UPDATE public.ohd_meter
                                SET status_id = @Active,
                                    modified_at = @ModifiedAt
                                WHERE id = ANY(@Ids)
                                RETURNING id
                            )
                            SELECT COUNT(*) FROM updated";
                var parameters = new DynamicParameters();
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Ids", ids);

                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateCustomerAgreementValueByPropertyId(int propertyId, string value)
        {
            var sQuery = @"Update public.ohd_Property
                         Set status_id=@Active,
                        modified_at = @ModifiedAt,
                        customer_agreement_id=@Value
                         where id=@PropertyId;
                        SELECT id FROM public.ohd_Property
                         where id=@PropertyId;";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@Value", value);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);

            int id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters);
            return id;
        }
    }
}
