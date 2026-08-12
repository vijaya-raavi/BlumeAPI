using Dapper;
using Microsoft.AspNetCore.Components.RenderTree;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Meter.Queries;
using Ontec.Infrastructure.Helper;

namespace Ontec.Infrastructure.Persistence.Repositories.MeterRepository
{
    public class MeterRepository : IMeterRepository
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ICompanyHelper _companyHelper ;
        private readonly IWorkContext _workContext ;
        public MeterRepository(IGenericRepository genericRepository
                               , IConfigurationRepository configurationRepository
                                , ICompanyHelper companyHelper
                                , IWorkContext workContext)
        {
            _genericRepository = genericRepository;
            _configurationRepository = configurationRepository;
            _companyHelper= companyHelper;
            _workContext= workContext;
        }
        public async Task<int> GetMeterCountByPropertyId(int propertyId)
        {
            var sQuery = @"SELECT Count(Id) FROM ohd_meter 
                           WHERE property_id=@PropertyId and status_id!=@StatusId";

            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            return await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<MeterDto>> GetMetersByPropertyId(int propertyId)
        {
            //var meterDto=new MeterDto();
            IEnumerable<MeterDto> meters = Enumerable.Empty<MeterDto>();
            try
            {
                var sQuery = @"Select mr.id, mr.meter_number AS MeterNumber
                           ,mr.meter_alias AS  MeterAlias
                           ,mr.daily_target_consumption AS Target
                           ,mrt.display_value AS MeterType
						   ,S.display_value AS Status
						   ,doc.url AS MeterDocument
                           ,mrt.unitofmeasure as UnitOfMeasure
                           , mr.comments As Comments
                            ,mr.is_solar AS IsSolar
                           , to_char(mr.contract_end_date::date,'dd-MM-yyyy')  AS contractEndDate
                            ,mr.eft_number AS EFTNumber
                           FROM public.ohd_meter AS mr
						   JOIN public.ohd_enum_status AS s ON mr.status_id=s.Id
						   JOIN public.ohd_enum_meter_type AS mrt ON mr.meter_type_id=mrt.id
						   LEFT JOIN public.ohd_document AS doc ON doc.id=mr.contract_proof_document
                           WHERE mr.property_id=@PropertyId AND mr.status_id!=@StatusId AND mr.status_id!=@Deactive";
                var parameters = new DynamicParameters();
                parameters.Add("@PropertyId", propertyId);
                parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                var company = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
                meters=  await _genericRepository.GetAsync<MeterDto>(sQuery, parameters).ConfigureAwait(false);
                if (meters.Any())
                {
                    foreach (var m in meters)
                    {
                        if (m.MeterDocument != null)
                        {
                            m.MeterDocument = company.Domain + m.MeterDocument;
                            byte[] fileBytes = null;
                            fileBytes = await _genericRepository.GetDocumentAsBytesAsync(m.MeterDocument).ConfigureAwait(false);
                            if (fileBytes != null)
                            {
                                var meterDoc = new DocumentResultDto
                                {
                                    FileName = Path.GetFileName(m.MeterDocument),
                                    Type = Path.GetExtension(m.MeterDocument),
                                    Document = fileBytes
                                };

                                m.MeterDoc = meterDoc;
                            }
                            m.MeterDocument = null;
                        }
                    }
                }
                return meters;
            }
            catch (Exception ex)
            {
                return new List<MeterDto>();
            }
        }
        public async Task<IEnumerable<MeterDto>> GetAllMetersByPropertyId(int propertyId)
        {
            try
            {
                var sQuery = @"Select mr.id, mr.meter_number AS MeterNumber
                           ,mr.meter_alias AS  MeterAlias
                           ,mr.daily_target_consumption AS Target
                           ,mrt.display_value AS MeterType
						   ,S.display_value AS Status
						   ,doc.url AS MeterDocument
                           ,mrt.unitofmeasure as UnitOfMeasure
                           , mr.comments As Comments
                            ,mr.isverified As IsVerified   
                           , to_char(mr.contract_end_date::date,'dd-MM-yyyy')  AS contractEndDate
                            ,mr.eft_number AS EFTNumber
                           FROM public.ohd_meter AS mr
						   JOIN public.ohd_enum_status AS s ON mr.status_id=s.Id
						   JOIN public.ohd_enum_meter_type AS mrt ON mr.meter_type_id=mrt.id
						   LEFT JOIN public.ohd_document AS doc ON doc.id=mr.contract_proof_document
                           WHERE mr.property_id=@PropertyId ";
                var parameters = new DynamicParameters();
                parameters.Add("@PropertyId", propertyId);

                return await _genericRepository.GetAsync<MeterDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<MeterDto>();
            }
        }
        public async Task<IEnumerable<MeterDto>> GetAllMeters()
        {
            try
            {
                var sQuery = @"Select mr.id, mr.meter_number AS MeterNumber
                           ,mr.meter_alias AS  MeterAlias
                           ,mr.daily_target_consumption AS Target
                           ,mrt.display_value AS MeterType
						   ,S.display_value AS Status
						   ,doc.url AS MeterDocument
                           ,mrt.unitofmeasure as UnitOfMeasure
                           , mr.comments As Comments
                            ,mr.isverified As IsVerified   
                           , to_char(mr.contract_end_date::date,'dd-MM-yyyy')  AS contractEndDate
                           FROM public.ohd_meter AS mr
						   JOIN public.ohd_enum_status AS s ON mr.status_id=s.Id
						   JOIN public.ohd_enum_meter_type AS mrt ON mr.meter_type_id=mrt.id
						   LEFT JOIN public.ohd_document AS doc ON doc.id=mr.contract_proof_document
                           WHERE mr.status_id in (@Active,@Pending,@Rejected,@Deactive)";
                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Pending", (int)StatusEnum.Pending);
                parameters.Add("@Rejected", (int)StatusEnum.Rejected);
                parameters.Add("@Deactive", (int)StatusEnum.Deactive);

                return await _genericRepository.GetAsync<MeterDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<MeterDto>();
            }
        }
        public async Task<bool> IsMeterIdExist(int meterId)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE Id=@MeterId and Status_Id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<bool> IsMeterNumberDeactivated(string meter)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE meter_number=@Meter and Status_Id=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@Meter", meter);
            parameters.Add("@StatusId", (int)StatusEnum.Deactive);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> IsMeterExist(int meterId)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE Id=@MeterId  ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }
       
        public async Task<bool> IsMeterTypeIdExist(int id)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_enum_meter_type
                          WHERE Id=@MeterTypeId and Status_Id!=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterTypeId", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task DeleteMeterById(int meterId)
        {
            var sQuery = @"Update public.ohd_meter
                         Set status_id=@StatusId, modified_at = @ModifiedAt
                         where id=@MeterId";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            await _genericRepository.ExecuteScalarAsync(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<string>> GetMeterTypesByPropertyId(int propertyId)
        {
            var sQuery = @" SELECT mrt.display_value as MeterType 
                           FROM public.ohd_meter as mr
						   JOIN public.ohd_enum_meter_type as mrt on mr.meter_type_id=mrt.id
                           WHERE mr.property_id=@PropertyId and mr.status_id != @StatusId";

            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);

            return await _genericRepository.GetAsync<string>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<UpdateMeterDto> GetMeterById(int id)
        {
            var sQuery = @" Select mr.id, mr.meter_number as MeterNumber
                           ,mr.meter_alias as  MeterAlias
						   ,mr.property_id  as PropertyId
                           ,mr.daily_target_consumption as DailyTargetConsumption
						   ,mrt.display_value as MeterType
						   ,mrt.Id as MeterTypeId 
                           ,to_char(mr.contract_end_date,'yyyy-MM-dd') AS ContractEndDate
			               ,s.display_value as Status
						   ,s.Id as StatusId
                           ,mr.eft_number As EFTNumber 
						   ,mr.contract_proof_document as ContractProofDocumentId
						   ,doc.url AS MeterDocument
                           , mr.comments As Comments
                           FROM public.ohd_meter as mr
						   JOIN public.ohd_enum_meter_type as mrt on mr.meter_type_id=mrt.id
						   join public.ohd_enum_status as s on mr.status_id= s.Id
						   LEFT JOIN public.ohd_document AS doc ON mr.contract_proof_document =doc.id
                           WHERE mr.id=@MeterId AND mr.status_id!=@StatusId ";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", id);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@Rejected", (int)StatusEnum.Rejected);


            return await _genericRepository.GetFirstOrDefaultAsync<UpdateMeterDto>(sQuery, parameters).ConfigureAwait(false);

        }
        public async Task<UpdateMeterDto> GetMeterByMeterNumber(string meterNumber)
        {
            var sQuery = @" Select mr.id, mr.meter_number as MeterNumber
                           ,mr.meter_alias as  MeterAlias
						   ,mr.property_id  as PropertyId
                           ,mr.daily_target_consumption as DailyTargetConsumption
						   ,mrt.display_value as MeterType
                           ,mrt.unitofmeasure
						   ,mrt.Id as MeterTypeId 
                           ,mr.eft_number As EFTNumber 
                           ,to_char(mr.contract_end_date,'yyyy-MM-dd') AS ContractEndDate
			               ,s.display_value as Status
						   ,s.Id as StatusId
						   ,mr.contract_proof_document as ContractProofDocumentId
						   ,doc.url AS MeterDocument
                           FROM public.ohd_meter as mr
						   JOIN public.ohd_enum_meter_type as mrt on mr.meter_type_id=mrt.id
						   join public.ohd_enum_status as s on mr.status_id= s.Id
						   LEFT JOIN public.ohd_document AS doc ON mr.contract_proof_document =doc.id
                           WHERE mr.meter_number=@MeterNumber AND mr.status_id=@Active ";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterNumber", meterNumber);
            parameters.Add("@Active", (int)StatusEnum.Active);

            var meter = await _genericRepository.GetFirstOrDefaultAsync<UpdateMeterDto>(sQuery, parameters).ConfigureAwait(false);
            var company=await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            if (meter.MeterDocument != null)
            {
                meter.MeterDocument = company.Domain + meter.MeterDocument;
            }
            return meter;

        }

        public async Task<UpdateMeterDto> GetMeterDetails(string meterNumber)
        {
            var sQuery = @" Select mr.id, mr.meter_number as MeterNumber
                           ,mr.meter_alias as  MeterAlias
						   ,mr.property_id  as PropertyId
                           ,mr.daily_target_consumption as DailyTargetConsumption
						   ,mrt.display_value as MeterType
                           ,mrt.unitofmeasure
						   ,mrt.Id as MeterTypeId 
                           ,mr.eft_number As EFTNumber 
                           ,to_char(mr.contract_end_date,'yyyy-MM-dd') AS ContractEndDate
			               ,s.display_value as Status
						   ,s.Id as StatusId
						   ,mr.contract_proof_document as ContractProofDocumentId
						   ,doc.url AS MeterDocument
                           FROM public.ohd_meter as mr
						   JOIN public.ohd_enum_meter_type as mrt on mr.meter_type_id=mrt.id
						   join public.ohd_enum_status as s on mr.status_id= s.Id
						   LEFT JOIN public.ohd_document AS doc ON mr.contract_proof_document =doc.id
                           WHERE mr.meter_number=@MeterNumber and mr.status_id=@Active ";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterNumber", meterNumber);
            parameters.Add("@Active", (int)StatusEnum.Active);
            var meter = await _genericRepository.GetFirstOrDefaultAsync<UpdateMeterDto>(sQuery, parameters).ConfigureAwait(false);
            var company = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
            if (meter!=null && meter.MeterDocument != null)
            {
                meter.MeterDocument = company.Domain + meter.MeterDocument;
            }
            return meter;

        }
        public async Task<EditMeterMasters> GetEditMeterMasters(int userId)
        {
            var result = new EditMeterMasters();
            var sQuery = @"SELECT id, name  
                         FROM public.ohd_property
                         WHERE owner_id=@OwnerId AND Status_id=@StatusId";
            var parameters = new DynamicParameters();
            parameters.Add("@OwnerId", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            result.PropertiesList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            sQuery = @"SELECT id, display_value as  name , unitOfMeasure as OtherText
                      FROM public.ohd_enum_meter_type
                      where Status_id=@StatusId ";

            parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Active);

            result.MeterTypeList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            sQuery = @"SELECT id, name  
                      FROM public.ohd_document_type
                      where Status_id=@StatusId";
            result.DocumentTypeList = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameters).ConfigureAwait(false);

            var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
            if (configurations.Any())
            {
                var documentSize = configurations.FirstOrDefault(t => t.Name.Equals("ContractDocumentSizeInMB"));
                if (documentSize != null)
                    result.ContractDocumentSizeInMb = int.Parse(documentSize.Value);
            }
            return result;
        }
        public async Task<int> IsActivePendingRejectedMeterNumberExist(string meterNumber, int meterId)
        {
            var sQuery = @" SELECT Id FROM public.ohd_meter
                            WHERE Id!=@MeterId AND meter_number=@MeterNumber AND (status_id=@Active OR status_id=@Pending OR status_id=@Rejected OR status_id=@DeActive)";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);
            parameters.Add("@MeterNumber", meterNumber);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@Pending", (int)StatusEnum.Pending);
            parameters.Add("@Rejected", (int)StatusEnum.Rejected);
            parameters.Add("@DeActive", (int)StatusEnum.Deactive);
            try
            { 
            var id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return id;
            }
            catch(Exception ex) { return 0; }
        }

        public async Task<int> UpdateMeter(UpdateMeterDto request)
        {
            var sQuery = @"UPDATE public.ohd_meter
	                         SET property_id=@PropertyId
                             ,meter_type_id=@MeterTypeId
                             ,meter_alias=@MeterAlias
                             ,daily_target_consumption=@DailyTargetConsumption
                             ,contract_end_date=@ContractEndDate
                             ,modified_at=@ModifiedAt
                             ,status_id=@StatusId
                            ,eft_number=@EFTNumber
                            ,contract_proof_document=@ContractProofDocument
                            ,comments=@Comment
                            ,isverified=@IsVerified
	                       WHERE id=@Id;
                           Select Id from public.ohd_meter
                           WHERE id=@Id;";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", request.PropertyId);
            parameters.Add("@MeterTypeId", request.MeterTypeId);
            parameters.Add("@MeterAlias", request.MeterAlias);
            parameters.Add("@DailyTargetConsumption", request.DailyTargetConsumption);
            
            DateTime? contractEndDate = null;
            if (!string.IsNullOrEmpty(request.ContractEndDate))
            {
                DateValidator.IsValidDate(request.ContractEndDate, out contractEndDate);
            }
            parameters.Add("@ContractEndDate", contractEndDate);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@StatusId", request.StatusId);
            parameters.Add("@EFTNumber", request.EFTNumber.ToLower());
            parameters.Add("@ContractProofDocument", request.ContractProofDocumentId);
            parameters.Add("@IsVerified", true);
            if (request.StatusId == (int)StatusEnum.Pending)
                parameters.Add("@Comment", "Meter sent for admin approval");
            else
                parameters.Add("@Comment", null);
            parameters.Add("@Id", request.Id);
            try
            {
                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> AddMeter(AddUpdateMeterQuery request, int meterMasterTypeId, string eftNo, bool isverified)
        {
            var sQuery = @"INSERT INTO public.ohd_meter(
	                                property_id
                                    ,meter_number
                                    ,meter_type_id
                                    ,meter_alias
                                    ,daily_target_consumption
                                    ,contract_proof_document
                                    ,contract_end_date
                                    ,status_id
                                    ,created_at
                                    ,meter_master_typeid
                                    ,comments
                                    ,eft_number
                                    ,isverified)
                            	VALUES (@PropertyId
                                       ,@MeterNumber
                                       ,@MeterTypeId
                                       ,@MeterAlias
                                       ,@DailyTargetConsumptionId
                                       ,@ContractProofDocument
                                       ,@ContractEndDate
                                       ,@StatusId
                                       ,@CreatedAt
                                       ,@MeterMasterTypeId
                                       ,@Comment
                                        ,@EFTNo
                                        ,@IsVerified)
                            RETURNING lastval() ";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", request.PropertyId);
            parameters.Add("@MeterNumber", request.MeterNumber);
            parameters.Add("@MeterTypeId", request.MeterTypeId);
            parameters.Add("@MeterAlias", request.MeterAlias);
            parameters.Add("@DailyTargetConsumptionId", request.DailyTargetConsumption);
            if (request.ContractEndDate != null)
            {
                parameters.Add("@ContractEndDate", request.ContractEndDate.ParseDateTime());
            }
            else
            {
                parameters.Add("@ContractEndDate", null);
            }
            parameters.Add("@ContractProofDocument", request.ContractProofDocumentId == 0 ? null : request.ContractProofDocumentId);
            parameters.Add("@StatusId", request.StatusId);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            parameters.Add("@MeterMasterTypeId", meterMasterTypeId);
            parameters.Add("@IsVerified", isverified);
            if (request.StatusId == (int)StatusEnum.Pending)
                parameters.Add("@Comment", "Meter sent for admin approval");
            else
                parameters.Add("@Comment", null);
            parameters.Add("@EFTNo", eftNo);
            try
            {
                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<MeterConsumptionUnitDto> GetTargetConsumptionByMeterNumber(string meterNumber)
        {
            var sQuery = @"Select mr.daily_target_consumption as DailyTargetConsumption 
                            , mrt.unitofmeasure
                            , mrt.meterreadingtypeId as MeterReadingType
                           FROM public.ohd_meter as mr
						   JOIN public.ohd_enum_meter_type as mrt on mr.meter_type_id=mrt.id 
                           WHERE mr.meter_number= @MeterId AND mr.status_id=@Active";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterNumber);
            parameters.Add("@Active", (int)StatusEnum.Active);
            try
            {
                var data = await _genericRepository.GetFirstOrDefaultAsync<MeterConsumptionUnitDto>(sQuery, parameters).ConfigureAwait(false);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }
        #region GetMeterRequest
        public async Task<DatatableModel<MeterRequestDto>> GetMeter(GetMeterRequestQuery request)
        {
            var dt = new DatatableModel<MeterRequestDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize
            };
            if (string.IsNullOrEmpty(request.order) || (request.order == "string"))
                request.order = "desc";
            var sortQuery = "";

            if (string.IsNullOrEmpty(request.sort) || (request.sort == "string"))
                request.sort = "mt.created_at";
            
            if (!string.IsNullOrEmpty(request.sort) || (request.sort != "string"))
            {
                sortQuery += " order by " + request.sort + " " + request.order;
            }
            switch (request.sort.ToLower())
            {
                case "date":
                    request.sort = " mt.created_at ";
                    break;
                case "meternumber":
                    request.sort = " mt.meter_number ";
                    break;

                case "propertytitle":
                    request.sort = " p.name  ";
                    break;
                case "meteralias":
                    request.sort = " mt.meter_alias ";
                    break;
               
                case "customer":
                    request.sort = " u.first_name ,u.last_name,u.mobile  ";
                    break;
            }
            try
            {
                var sQuery = @"SELECT  concat(p.name,' ' ,emt.name, '  meter ' ) AS MeterDetails 
                            ,mt.meter_alias As MeterAlias,mt.meter_number As MeterNumber
                            ,emt.name as UtilityType
                            ,mt.daily_target_consumption AS TargetConsumption
                            ,p.unit_number As UnitNumber,p.name As PropertyTitle
                            ,concat(p.address_line_1,' ',p.address_line_2) AS PropertyAddress
                            ,concat(u.first_name ,' ',u.last_name,' ',u.mobile )AS Customer
                            ,doc.url AS DOCUMENT
                            ,to_char(mt.created_at,'dd-MM-yyyy') AS DATE
                            ,CASE WHEN mt.status_id = 4 THEN false ELSE true END AS Status
                            ,mt.Id AS MeterID
                            ,emt.unitOfMeasure
                            FROM public.ohd_meter AS mt
                            LEFT JOIN public.ohd_property AS p ON mt.property_id=p.id 
                            LEFT JOIN public.ohd_enum_meter_type AS emt ON emt.id=mt.meter_type_id
                            LEFT JOIN public.ohd_user AS u ON u.id=p.owner_id
                            LEFT JOIN public.ohd_document AS doc ON doc.id=mt.contract_proof_document
                            WHERE ";
                var parameters = new DynamicParameters();

                sQuery += @" mt.status_id = @StatusId";
                parameters.Add("@StatusId", (int)StatusEnum.Pending);

                if (!string.IsNullOrEmpty(request.SearchText.Trim()) && (request.SearchText.Trim() != "string"))
                {
                    var searchText = request.SearchText.Trim();
                    var searchTerms = searchText.ToLower().Split(' ');
                    var searchConditions = new List<string>();
                    var index = 0;

                    // Add condition for the full search text
                    var fullSearchTextParam = "@SearchTextFull";
                    searchConditions.Add($@"(lower(mt.meter_alias) like {fullSearchTextParam}
                                    OR lower(p.name) like {fullSearchTextParam}
                                    OR lower(emt.name) like {fullSearchTextParam}
			                        OR lower(mt.meter_number)like {fullSearchTextParam}
			                        OR lower(p.unit_number)like {fullSearchTextParam}
                                    OR lower(p.address_line_1) like {fullSearchTextParam}
                                    OR lower(p.address_line_2) like {fullSearchTextParam}
			                        OR lower(u.first_name) like {fullSearchTextParam}
                                    OR lower(u.last_name) like {fullSearchTextParam}
                                    OR lower(u.mobile) like {fullSearchTextParam}
                                    OR lower(to_char(mt.created_at::date, 'dd-MM-yyyy')) like {fullSearchTextParam})");
                    parameters.Add(fullSearchTextParam, "%" + searchText.ToLower() + "%");

                    // Add conditions for each split term
                    foreach (var term in searchTerms)
                    {
                        var paramName = "@SearchText" + index;
                        searchConditions.Add($@"(lower(mt.meter_alias) like {paramName}
                                           OR lower(p.name) like {paramName}
                                        OR lower(emt.name) like {paramName}
			                            OR lower(mt.meter_number)like {paramName}
			                            OR lower(p.unit_number)like {paramName}
                                        OR lower(p.address_line_1) like {paramName}
                                        OR lower(p.address_line_2) like {paramName}
			                            OR lower(u.first_name) like {paramName}
                                        OR lower(u.last_name) like {paramName}
                                        OR lower(u.mobile) like {paramName}
                                        OR lower(to_char(mt.created_at::date, 'dd-MM-yyyy')) like {paramName})");
                        parameters.Add(paramName, "%" + term + "%");
                        index++;
                    }

                    if (searchConditions.Any())
                    {
                        sQuery += " AND (" + string.Join(" OR ", searchConditions) + ")";
                    }
                }

                if (sortQuery != "")
                {
                    sQuery += sortQuery;
                }

                var meterRequest = await _genericRepository.GetAsync<MeterRequestDto>(sQuery, parameters).ConfigureAwait(false);
                var result = meterRequest.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                var companyhHelper = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
                //if (result.Any())
                //{
                //    result = result.Select(i =>
                //    {

                //        i.Document = companyhHelper.Domain + i.Document;
                //        return i;
                //    }).ToList();
                //}
                dt.Data = result.ToList();
                dt.TotalRecords = meterRequest.Count();
            }
            catch (Exception ex)
            {

            }
            return dt;
        }
        #endregion

        #region ApproveRejectMeterRequest
        public async Task<bool> IsPendingMeterIdExist(int meterId)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE Id=@MeterId and Status_Id=@StatusId ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);
            parameters.Add("@StatusId", (int)StatusEnum.Pending);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count > 0;
        }
        public async Task<int> ApproveRejectMeterById(GetApproveRejectMeterQuery request)
        {
            var sQuery = @"Update public.ohd_meter
                         Set status_id=@StatusId
                        , modified_at = @ModifiedAt
                            ,isverified=@IsVerified
                         ,comments=@Comments
                         where id=@MeterId;
                         Select Id from public.ohd_meter
                         WHERE id=@MeterId;  ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", request.MeterID);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Comments", request.Comments);
            parameters.Add("@IsChecked", request.IsChecked);
            parameters.Add("@IsVerified", true);
            if (request.IsChecked && !request.IsApproved)
            {
                parameters.Add("@StatusId", (int)StatusEnum.Deactive);
            }
            else
            {
                if (request.IsApproved)
                    parameters.Add("@StatusId", (int)StatusEnum.Active);
                else
                    parameters.Add("@StatusId", (int)StatusEnum.Rejected);
            }

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
        #endregion

        #region DashboardMeters
        public async Task<IEnumerable<DashboardMeterDayConsumptionDto>> GetMeterMasterByPropertyId(int propertyId)
        {
            var sQuery = @"Select mr.meter_number AS MeterNumber 
                           ,mr.daily_target_consumption AS DailyTargetConsumption
                           ,mrt.display_value AS MeterType
						   ,mrt.unitofmeasure as UnitOfMeasure	 
                           ,mrt.meterreadingtypeid as MeterReadingType
                           ,mr.is_solar AS IsSolar 
                           FROM public.ohd_meter AS mr 
						   JOIN public.ohd_enum_meter_type AS mrt ON mr.meter_type_id=mrt.id 
                           WHERE mr.property_id=@PropertyId AND mr.status_id=@Active";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);
            parameters.Add("@Active", (int)StatusEnum.Active);

            return await _genericRepository.GetAsync<DashboardMeterDayConsumptionDto>(sQuery, parameters).ConfigureAwait(false);
        }
        #endregion

        #region MeterTypeMaster
        public async Task<int> AddMeterMasterType(string meterMasterType, int MeterTypeId)
        {
            var sQuery = @"INSERT INTO public.ohd_meter_typemaster(
                                    name
                                    ,meter_type_id
                                   
                                    ,created_at)
                            	VALUES (@Name
                                       ,@MeterTypeId
                                       
                                       ,@CreatedAt)
                            RETURNING lastval() ";
            var parameters = new DynamicParameters();
            parameters.Add("@Name", meterMasterType);
            parameters.Add("@MeterTypeId", MeterTypeId);
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
        public async Task<bool> IsExistMeterMasterType(string meterMasterType)
        {
            try
            {
                var sQuery = @"select count(id) from ohd_meter_typemaster where name=@Name";
                var parameters = new DynamicParameters();
                parameters.Add("@Name", meterMasterType);
                var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return count > 0;
            }
            catch (Exception ex) { }
            return false;
        }
        #endregion
        public async Task<MeterIdWithNumberDto> GetOwnerIdByMeterId(int meterId)
        {
            var sQuery = @" Select pr.owner_id as Id, mr.meter_number as MeterNumber
                           FROM public.ohd_meter as mr
						  Join public.ohd_property as pr on mr.property_id=pr.id
                           WHERE mr.id=@MeterId";

            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);

            return await _genericRepository.GetFirstOrDefaultAsync<MeterIdWithNumberDto>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<MeterExpiringDto>> GetMeterExpiringDtos(int userId)
        {
            var sQuery = @"SELECT per.name AS Property
                                ,to_char(mt.contract_end_date,'dd-MM-yyyy') AS ExpiredOn
                                ,mt.meter_number AS MeterNumber   
                                FROM public.ohd_property AS per 
                          LEFT JOIN public.ohd_meter AS mt ON  per.id=mt.property_id
                          LEFT JOIN public.ohd_property_user_relation AS pur ON per.id= pur.property_id 
                          WHERE (per.owner_id = @UserId OR pur.user_id=@UserId) AND per.status_id=@Active AND mt.status_id=@Active
                          AND mt.contract_end_date <=@CheckDate AND mt.contract_end_date IS NOT NULL";

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@CheckDate", DateTime.UtcNow.AddDays(7));

            return await _genericRepository.GetAsync<MeterExpiringDto>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<Utilities>> GetMeterTypes()
        {
            var sQuery = @"SELECT id,
                          name
                          --unitofmeasure As UnitOfMeasure,
                          --mindailytarget AS MinDailyTarget,
                          --maxdailytarget AS MaxDailyTarget
                          FROM public.ohd_enum_meter_type
                          WHERE  status_id != @StatusId";

            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            return await _genericRepository.GetAsync<Utilities>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<MeterTypes>> GetMeterType()
        {
            var sQuery = @"SELECT id,
                          name,
                          unitofmeasure As UnitOfMeasure,
                          mindailytarget AS MinDailyTarget,
                          maxdailytarget AS MaxDailyTarget
                          FROM public.ohd_enum_meter_type
                          WHERE  status_id != @StatusId";

            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            return await _genericRepository.GetAsync<MeterTypes>(sQuery, parameters).ConfigureAwait(false);
        }
        #region AddMetersFromMeterList
        public async Task<int> AddMetersFromMeterList(AddMetersFromListHelperClass obj)
        {
            try
            {
                var sQuery = @" INSERT INTO public.ohd_meter(
                                            meter_number,
                                            property_id,
                                            meter_alias,
                                            daily_target_consumption,
                                            meter_type_id,
                                            status_id,
                                            created_at,
                                            eft_number,
                                            isverified)
                                            VALUES (
                                        @MeterNumber,
                                        @PropertyId,
                                        @MeterAlias,
                                        @DailyTargetConsumption,
                                        @MeterTypeId,
                                        @StatusId, 
                                        @CreatedAt,
                                        @EFTNo,
                                       @IsVerified)
                                          RETURNING lastval()";

                var parameters = new DynamicParameters();

                parameters.Add("@MeterNumber", obj.MeterNumber);  
                parameters.Add("@PropertyId", obj.PropertyId);
                parameters.Add("@MeterAlias", obj.MeterAlias);
                parameters.Add("@DailyTargetConsumption", obj.TargetConsumption);
                parameters.Add("@MeterTypeId", obj.MeterTypeId);//for temprory
                parameters.Add("@StatusId", obj.StatusId);
                parameters.Add("@CreatedAt", DateTime.UtcNow);
                parameters.Add("@EFTNo", obj.EFTNo);
                parameters.Add("@IsVerified", obj.IsVerified);
                var id=await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion
        public async Task<MeterDto> GetMetersById(int meterId)
        {
            var sQuery = @"Select mr.id, mr.meter_number AS MeterNumber
                           ,mr.meter_alias AS  MeterAlias
                           ,mr.daily_target_consumption AS Target
                           ,mrt.display_value AS MeterType
						   ,S.display_value AS Status
						   ,doc.url AS MeterDocument
                           ,mrt.unitofmeasure as UnitOfMeasure
                           , mr.comments As Comments
                           , to_char(mr.contract_end_date::date,'dd-MM-yyyy')  AS contractEndDate
                           FROM public.ohd_meter AS mr
						   JOIN public.ohd_enum_status AS s ON mr.status_id=s.Id
						   JOIN public.ohd_enum_meter_type AS mrt ON mr.meter_type_id=mrt.id
						   LEFT JOIN public.ohd_document AS doc ON doc.id=mr.contract_proof_document
                           WHERE mr.id=@MeterId AND mr.status_id!=@StatusId AND mr.status_id!=@Deactive";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterId", meterId);
            parameters.Add("@StatusId", (int)StatusEnum.Inactive);
            parameters.Add("@Deactive", (int)StatusEnum.Deactive);
            return await _genericRepository.GetFirstOrDefaultAsync<MeterDto>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> IsEftNoExist(string eftNumber)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE eft_number=@EFTNumber  ";
            var parameters = new DynamicParameters();
            parameters.Add("@EFTNumber", eftNumber.ToLower());

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;

        }
        public async Task<int> IsMeterNumberExist(string meterNumber)
        {
            var sQuery = @"SELECT Count(Id) from  public.ohd_meter
                          WHERE meter_number=@MeterNumber";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterNumber", meterNumber);

            var count = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return count;
        }
        public async Task<int> GetPropertyIdByEFTNumber(string eftNumber)
        {
            var sQuery = @"SELECT property_id from  public.ohd_meter
                          WHERE eft_number=@EFT  ";
            var parameters = new DynamicParameters();
            parameters.Add("@EFT", eftNumber.ToLower());

            var propertyId = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return propertyId;
        }
        public async Task<int> GetMeterIdByEFTNumber(string eftNumber)
        {
            var sQuery = @"SELECT id from  public.ohd_meter
                          WHERE eft_number=@EFT  ";
            var parameters = new DynamicParameters();
            parameters.Add("@EFT", eftNumber.ToLower());

            var meterId = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return meterId;
        }
        public async Task<int> GetPropertyIdByMeterNumber(string meterNumber)
        {
            var sQuery = @"SELECT property_id from  public.ohd_meter
                          WHERE meter_number=@MeterNumber  ";
            var parameters = new DynamicParameters();
            parameters.Add("@MeterNumber", meterNumber);

            var propertyId = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return propertyId;
        }
        public async Task<int> IsMeterSolar(int  propertyId)
        {
            var sQuery = @"SELECT  count (is_solar) from  public.ohd_meter
                          WHERE property_id =@PropertyId AND is_solar=true";
            var parameters = new DynamicParameters();
            parameters.Add("@PropertyId", propertyId);

            var isSolar = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return isSolar;
        }
        public async Task<int> UpdateEFTNumberByMeterId(int meterId, string eftNo)
        {
            var sQuery = @"UPDATE public.ohd_meter
	                         SET eft_number=@EFTNumber                            
	                       WHERE id=@Id;
                           Select Id from public.ohd_meter
                           WHERE id=@Id;";
            var parameters = new DynamicParameters();

            parameters.Add("@EFTNumber", eftNo);
            parameters.Add("@Id", meterId);
            try
            {
                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<string> GetUnitOfMeasure(string meter)
        {
            var sQuery = @"select mt.unitofmeasure from ohd_meter  as m
                            LEFT JOIN public.ohd_enum_meter_type mt ON m.meter_type_id=mt.id
                            where meter_number=@Meter";
            var parameters = new DynamicParameters();
            parameters.Add("@Meter", meter);

            var unit = await _genericRepository.ExecuteScalarAsync<string>(sQuery, parameters).ConfigureAwait(false);
            return unit;
        }
        public async Task<int> UpdateMeterMasterType(int id, string masterMeterType)
        {
            var sQuery = @"UPDATE  ohd_meter  as m
                           SET meter_master_type=@MasterMeterType
                            where id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@MasterMeterType", masterMeterType);

            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result;
        }
    }
}

