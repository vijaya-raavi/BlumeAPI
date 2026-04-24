using Dapper;
using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Consumer;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumer;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.Infrastructure.Persistence.Repositories.Consumer
{
    public class ConsumerRepository : IConsumerRepository
    {
        private readonly IGenericRepository _genericRepository;
        private readonly ICompanyHelper _companyHelper;
        private readonly IWorkContext _workContext;
        public ConsumerRepository(IGenericRepository genericRepository, ICompanyHelper companyHelper, IWorkContext workContext)
        {
            _genericRepository = genericRepository;
            _companyHelper = companyHelper;
            _workContext = workContext;

        }
        public async Task<ConsumerMasterDto> GetConsumerMasters()
        {
            var sQuery = @"SELECT ur.status_id as StatusId,COUNT(ur.id) as UserCount FROM public.ohd_user as ur 
                         WHERE ur.role_id=@RoleId AND ur.status_id in(@Active,@Pending,@Inactive,@Deactive)
                         GROUP BY ur.status_id";


            var notVerifiedQuery = @"SELECT  COUNT(id) FROM public.ohd_user 
                                    WHERE role_id=@RoleId and isverified=false 
                                    AND status_id!=@InProcess 
                                    AND status_id!=@Inactive 
                                    AND status_id!=@Pending AND status_id!=@Deactive
                                    GROUP BY isverified";
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@Pending", (int)StatusEnum.Pending);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            parameters.Add("@Deactive", (int)StatusEnum.Deactive);
            parameters.Add("@NotVerified", (int)StatusEnum.NotVerified);
            parameters.Add("@InProcess", (int)StatusEnum.InProcess);

            var userCounts = await _genericRepository.GetAsync<ConsumerStatusCountDto>(sQuery, parameters).ConfigureAwait(false);
            int notVerifiedUsersCount = await _genericRepository.GetFirstOrDefaultAsync<int>(notVerifiedQuery, parameters);
            var result = new ConsumerMasterDto();
            var statusList = new List<OntecSelectListItem>{
                new() {
                    Id = (int)StatusEnum.All,
                    Name = StatusEnum.All.ToString(),
                     OtherText="0"
                },
                 new() {
                    Id = (int)StatusEnum.Pending,
                    Name = StatusEnum.Pending.ToString(),
                     OtherText="0"
                    },
                 new() {
                    Id = (int)StatusEnum.Active,
                    Name =StatusEnum.Active.ToString(),
                     OtherText="0"
                },
                 new() {
                    Id = (int)StatusEnum.Inactive,
                    Name = StatusEnum.Inactive.ToString(),
                     OtherText="0"
                },
                  new() {
                    Id = (int)StatusEnum.NotVerified,
                    Name = StatusEnum.NotVerified.ToString(),
                     OtherText="0"
                },

                };
            if (userCounts != null)
            {
                statusList = new List<OntecSelectListItem>{
                new() {
                    Id = (int)StatusEnum.All,
                    Name = StatusEnum.All.ToString(),
                    OtherText=userCounts.Where(t => !t.StatusId.Equals((int)StatusEnum.Pending)).Sum(t => t.UserCount).ToString(),
                },
                 new() {
                    Id = (int)StatusEnum.Pending,
                    Name = StatusEnum.Pending.ToString(),
                    OtherText=userCounts.Where(t => t.StatusId.Equals((int)StatusEnum.Pending)).Sum(t => t.UserCount).ToString()
                    },
                 new() {
                    Id = (int)StatusEnum.Active,
                    Name =StatusEnum.Active.ToString(),
                    OtherText=userCounts.Where(t => (t.StatusId.Equals((int)StatusEnum.Active) || (t.StatusId.Equals((int)StatusEnum.Deactive)))).Sum(t => t.UserCount).ToString()
                },
                 new() {
                    Id = (int)StatusEnum.Inactive,
                    Name = StatusEnum.Inactive.ToString(),
                    OtherText=userCounts.Where(t => t.StatusId.Equals((int)StatusEnum.Inactive)).Sum(t => t.UserCount).ToString()

                },
                  new() {
                    Id = (int)StatusEnum.NotVerified,
                    Name = StatusEnum.NotVerified.ToString(),
                    OtherText=notVerifiedUsersCount.ToString()
                  }
                };
            }
            result.StatusList = statusList;
            return result;
        }

        public async Task<ConsumerMasterDto> NewGetConsumerMaster(int estateId)
        {
            var sQuery = "";
            var notVerifiedQuery = "";
            if (estateId > 0)
            {
                sQuery = @"SELECT 
                                ur.status_id AS StatusId,
                                COUNT(DISTINCT ur.id) AS UserCount
                            FROM public.ohd_user ur
                            LEFT JOIN public.ohd_property p ON ur.id = p.owner_id
                            LEFT JOIN public.ohd_property_user_relation pr ON ur.id = pr.user_id
                            LEFT JOIN public.ohd_property pp ON pr.property_id = pp.id
                            WHERE 
                                ur.role_id = @RoleId
                                AND ur.status_id IN (@Active, @Deactive, @Inactive)
                                AND (
                                    p.estate_id = @EstateId OR 
                                    pp.estate_id = @EstateId
                                )
                                AND (pr.status_id = @Active OR pr.status_id IS NULL)
                            GROUP BY ur.status_id;";


                notVerifiedQuery = @"SELECT COUNT(ur.id) as UserCount 
                        FROM public.ohd_user as ur 
                         LEFT JOIN public.ohd_property p ON ur.id = p.owner_id
                            LEFT JOIN public.ohd_property_user_relation pr ON ur.id = pr.user_id
                            LEFT JOIN public.ohd_property pp ON pr.property_id = pp.id
                            WHERE ur.role_id=@RoleId AND ur.status_id  not in (@InProcess,@Deactive,@Pending,@Inactive)  
                           AND (
                                    p.estate_id = @EstateId AND 
                                    pp.estate_id = @EstateId
                                )
                                AND (pr.status_id = @Active OR pr.status_id IS NULL)
                            AND  p.status_id=@Active
                        GROUP BY isverified";
            }

            else
            {
                sQuery = @"SELECT ur.status_id as StatusId,COUNT(ur.id) as UserCount FROM public.ohd_user as ur 
                         WHERE ur.role_id=@RoleId AND ur.status_id in(@Active,@Pending,@Inactive,@Deactive)
                         GROUP BY ur.status_id";

                notVerifiedQuery = @"SELECT  COUNT(id) FROM public.ohd_user 
                                    WHERE role_id=@RoleId and isverified=false 
                                    AND status_id!=@InProcess 
                                    AND status_id!=@Inactive 
                                    AND status_id!=@Pending AND status_id!=@Deactive 
                                    GROUP BY isverified ";
            }
            var result = new ConsumerMasterDto();
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);
            parameters.Add("@Active", (int)StatusEnum.Active);
            parameters.Add("@Pending", (int)StatusEnum.Pending);
            parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            parameters.Add("@Deactive", (int)StatusEnum.Deactive);
            parameters.Add("@NotVerified", (int)StatusEnum.NotVerified);
            parameters.Add("@InProcess", (int)StatusEnum.InProcess);
            parameters.Add("@EstateId", estateId);
            try
            {
                var userCounts = await _genericRepository.GetAsync<ConsumerStatusCountDto>(sQuery, parameters).ConfigureAwait(false);
                int notVerifiedUsersCount = await _genericRepository.GetFirstOrDefaultAsync<int>(notVerifiedQuery, parameters);

                var statusList = new List<OntecSelectListItem>{
                new() {
                    Id = (int)StatusEnum.All,
                    Name = StatusEnum.All.ToString(),
                     OtherText="0"
                },
                 new() {
                    Id = (int)StatusEnum.Pending,
                    Name = StatusEnum.Pending.ToString(),
                     OtherText="0"
                    },
                 new() {
                    Id = (int)StatusEnum.Active,
                    Name =StatusEnum.Active.ToString(),
                     OtherText="0"
                },
                 new() {
                    Id = (int)StatusEnum.Inactive,
                    Name = StatusEnum.Inactive.ToString(),
                     OtherText="0"
                },
                  new() {
                    Id = (int)StatusEnum.NotVerified,
                    Name = StatusEnum.NotVerified.ToString(),
                     OtherText="0"
                },

                };
                if (userCounts != null)
                {
                    statusList = new List<OntecSelectListItem>{
                new() {
                    Id = (int)StatusEnum.All,
                    Name = StatusEnum.All.ToString(),
                    OtherText=userCounts.Where(t => !t.StatusId.Equals((int)StatusEnum.Pending)).Sum(t => t.UserCount).ToString(),
                },
                 new() {
                    Id = (int)StatusEnum.Pending,
                    Name = StatusEnum.Pending.ToString(),
                    OtherText=userCounts.Where(t => t.StatusId.Equals((int)StatusEnum.Pending)).Sum(t => t.UserCount).ToString()
                    },
                 new() {
                    Id = (int)StatusEnum.Active,
                    Name =StatusEnum.Active.ToString(),
                    OtherText=userCounts.Where(t => (t.StatusId.Equals((int)StatusEnum.Active) || (t.StatusId.Equals((int)StatusEnum.Deactive)))).Sum(t => t.UserCount).ToString()
                },
                 new() {
                    Id = (int)StatusEnum.Inactive,
                    Name = StatusEnum.Inactive.ToString(),
                    OtherText=userCounts.Where(t => t.StatusId.Equals((int)StatusEnum.Inactive)).Sum(t => t.UserCount).ToString()

                },
                  new() {
                    Id = (int)StatusEnum.NotVerified,
                    Name = StatusEnum.NotVerified.ToString(),
                    OtherText=notVerifiedUsersCount.ToString()
                  }
                };
                }
                result.StatusList = statusList;
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }

        }
        public async Task<DatatableModel<ConsumerDto>> GetConsumers(GetConsumersQuery request)
        {
            var dto = new ConsumerDto();
            var dt = new DatatableModel<ConsumerDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                SearchText = request.SearchText,
            };
            var sortQuery = "";
            if (string.IsNullOrEmpty(request.order) || (request.order != "string"))
                request.order = " desc";
            if (string.IsNullOrEmpty(request.sort) || (request.sort != "string"))
                request.sort = " createDate";

            switch (request.sort.ToLower())
            {
                case "createdate":
                    request.sort = " u.created_at ";
                    break;
                case "consumer":
                    request.sort = " u.first_name , u.last_name ";
                    break;

                case "contact":
                    request.sort = " u.email ";
                    break;

                case "address":
                    request.sort = " u.address_line_1 ,u.address_line_2 ";
                    break;
            }
            if (!string.IsNullOrEmpty(request.sort) && (request.sort != "string"))
            {
                sortQuery += " order by " + request.sort + " " + request.order;
            }
            var statusCase = "";

            var whereClause = " WHERE ur.id = @RoleId ";
            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);
            parameters.Add("@Active", (int)StatusEnum.Active);
            switch (request.StatusId)
            {
                case (int)StatusEnum.All:
                    statusCase = " ,CASE WHEN u.status_id = @Active THEN 1 ELSE 0 END AS status ";
                    whereClause += @" AND u.status_id in (@Active,@Deactive,@Inactive)";

                    parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                    parameters.Add("@Inactive", (int)StatusEnum.Inactive);
                    break;
                case (int)StatusEnum.Pending:
                    // statusCase = " ,CASE WHEN u.status_id = @StatusId THEN false ELSE false END AS status ";
                    whereClause += @" AND u.status_id = @StatusId ";
                    parameters.Add("@StatusId", (int)StatusEnum.Pending);
                    break;
                case (int)StatusEnum.Active:
                    statusCase = " ,CASE WHEN u.status_id = @Active THEN 1 ELSE 0 END AS status ";
                    whereClause += @" AND u.status_id in (@Active,@Deactive)";

                    parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                    break;
                case (int)StatusEnum.Inactive:
                    whereClause += @" AND u.status_id = @StatusId ";
                    parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                    parameters.Add("@Active", (int)StatusEnum.Active);
                    break;
                case (int)StatusEnum.NotVerified:
                    whereClause += @" AND u.isverified =@IsVerified";
                    whereClause += @" AND u.status_id = @Active ";
                    parameters.Add("@Active", (int)StatusEnum.Active);
                    parameters.Add("@IsVerified", request.IsVerified);
                    break;
            }
          
            var consumerQuery = @"SELECT
                            DISTINCT  u.Id,
                            u.profile_url AS profileUrl,
                            CONCAT(u.first_name, ' ', u.last_name) AS consumer,
                            CONCAT(u.address_line_1) AS address,
                            CONCAT(u.mobile, ' ', u.email) AS Contact,
                            TO_CHAR(u.created_at::date, 'dd-MM-yyyy') AS createDate,
                            doc.url as DocumentUrl,
                            u.status_id AS Status,
                            uw.balance AS WalletBalance,
                            u.isverified AS IsVerified,
                            doc.doc_number As DocNumber,
							doc.document_type AS DocTypeId,
							dt.name As DocType,
                            u.tax_number AS VATNumber
                            FROM ohd_User AS u
                            LEFT JOIN ohd_user_role_master AS ur ON ur.id = u.role_id
                            LEFT JOIN public.ohd_document as doc ON u.proof_document_id = doc.id 
                            LEFT JOIN public.ohd_document_type  AS dt ON doc.document_type = dt.id
                             LEFT JOIN public.ohd_user_wallet as uw ON u.id= uw.user_id
                             LEFT JOIN public.ohd_property as p on u.id=p.owner_id
                             LEFT JOIN public.ohd_property_user_relation as pur on u.id=pur.user_id
                               LEFT JOIN public.ohd_property AS pur_prop ON pur.property_id = pur_prop.id
							LEFT JOIN public.ohd_estate AS e ON  p.estate_id = e.id OR pur_prop.estate_id = e.id 

                             ";
            if (request.EstateId > 0)
            {
                parameters.Add("@EstateId", request.EstateId);
                whereClause += " AND e.id=@EstateId";
                whereClause += " AND pur.status_id=@Active";
            }
            
            consumerQuery += whereClause;
            consumerQuery += sortQuery;
            
            try
            {
                var consumerList = await _genericRepository.GetAsync<ConsumerDto>(consumerQuery, parameters).ConfigureAwait(false);
               
                var propertyCounts = await _genericRepository.GetAsync<PropertyCountDto>(
                     @"SELECT owner_id AS UserId, COUNT(*) AS PropertyCount
                      FROM ohd_property WHERE status_id = @Active GROUP BY owner_id",
                     new { Active = (int)StatusEnum.Active }
                        );

                // All active meter counts
                var meterCounts = await _genericRepository.GetAsync<MeterCountDto>(
                    @"SELECT per.owner_id AS UserId, COUNT(mt.id) AS MeterCount
                      FROM ohd_property per
                      INNER JOIN ohd_meter mt ON mt.property_id = per.id AND mt.status_id = @Active
                      WHERE per.status_id = @Active
                      GROUP BY per.owner_id",
                    new { Active = (int)StatusEnum.Active }
                );

                // All meter numbers
                 var meterNumbers = await _genericRepository.GetAsync<MeterNumberDto>(
                    @"SELECT per.owner_id AS UserId, STRING_AGG(mt.meter_number, ', ') AS MeterNumbers
                      FROM ohd_property per
                      INNER JOIN ohd_meter mt ON mt.property_id = per.id AND mt.status_id = @Active
                      WHERE per.status_id = @Active
                      GROUP BY per.owner_id",
                    new { Active = (int)StatusEnum.Active }
                );
                // var consumers = await _genericRepository.GetAsync<ConsumerDto>(sQuery, parameters).ConfigureAwait(false);
                var consumers = consumerList.ToList();
                var consumerDetails = (from user in consumers
                                       join pc in propertyCounts on user.Id equals pc.UserId into pcj
                                       from pc in pcj.DefaultIfEmpty()

                                       join mc in meterCounts on user.Id equals mc.UserId into mcj
                                       from mc in mcj.DefaultIfEmpty()

                                       join mn in meterNumbers on user.Id equals mn.UserId into mnj
                                       from mn in mnj.DefaultIfEmpty()

                                       select new ConsumerDto
                                       {
                                           Id = user.Id,
                                           ProfileUrl = user.ProfileUrl,
                                           Consumer = user.Consumer,
                                           Address = user.Address,
                                           Contact = user.Contact,
                                           CreateDate = user.CreateDate,
                                           DocumentUrl = user.DocumentUrl,
                                           Status = user.Status,
                                           WalletBalance = user.WalletBalance,
                                           DocNumber=user.DocNumber,
                                           DocTypeId=user.DocTypeId,
                                           DocType=user.DocType,
                                           IsVerified = user.IsVerified,
                                           VATNumber=user.VATNumber,
                                           Sources = "Property "+  (pc?.PropertyCount ?? 0 )+ " Meters "+ (mc?.MeterCount ?? 0),
                                           meter_numbers = mn?.MeterNumbers ?? string.Empty
                                       }).ToList();

                if (!string.IsNullOrWhiteSpace(request.SearchText)&& request.SearchText!="string")
                {
                    var searchText = request.SearchText.Trim().ToLower();

                    consumerDetails = consumerDetails.Where(x =>
                        (x.Consumer?.ToLower().Contains(searchText) ?? false) ||
                        (x.Address?.ToLower().Contains(searchText) ?? false) ||
                        (x.Contact?.ToLower().Contains(searchText) ?? false) ||
                        (x.CreateDate?.ToLower().Contains(searchText) ?? false) ||
                        (x.Sources?.ToLower().Contains(searchText) ?? false) ||
                        (x.meter_numbers?.ToLower().Contains(searchText) ?? false)
                    ).ToList();
                }
                var result = consumerDetails.ToList().Skip(request.Page * request.PageSize).Take(request.PageSize);
                var companyhHelper = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);
                if (result.Any())
                {
                    var task = result.Select(async i =>
                {
                    if (i.DocumentUrl != null)
                    {
                        i.DocumentUrl = companyhHelper.Domain + i.DocumentUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(i.DocumentUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var doc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(i.DocumentUrl),
                                Type = Path.GetExtension(i.DocumentUrl),
                                Document = fileBytes
                            };

                            i.Document = doc;
                        }
                        i.DocumentUrl = null;
                    }

                    if (i.ProfileUrl != null)
                    {
                        i.ProfileUrl = companyhHelper.Domain + i.ProfileUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(i.ProfileUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var profileDoc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(i.ProfileUrl),
                                Type = Path.GetExtension(i.ProfileUrl),
                                Document = fileBytes
                            };

                            i.Profile = profileDoc;
                        }
                        i.ProfileUrl = null;
                    }

                    return i;
                });
                    result = (await Task.WhenAll(task)).ToList();
                }
                dt.Data = result.ToList();
                dt.TotalRecords = consumerList.Count();

            }
            catch (Exception ex)
            {

            }

            return dt;
        }


        public async Task<DatatableModel<ConsumerDto>> GetConsumersNew(GetConsumersQuery request)
        {
            var dt = new DatatableModel<ConsumerDto>()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                SearchText = request.SearchText
            };

            var parameters = new DynamicParameters();
            parameters.Add("@RoleId", (int)RoleMasterEnum.Customer);
            parameters.Add("@Active", (int)StatusEnum.Active);

            // ----------------------------------------------------
            // SORT HANDLING
            // ----------------------------------------------------
            if (string.IsNullOrEmpty(request.order) || request.order == "string")
                request.order = "DESC";

            if (string.IsNullOrEmpty(request.sort) || request.sort == "string")
                request.sort = "CreateDate";

            switch (request.sort.ToLower())
            {
                case "createdate": request.sort = "u.id,u.created_at"; break;
                case "consumer": request.sort = "u.first_name, u.last_name"; break;
                case "contact": request.sort = "u.email"; break;
                case "address": request.sort = "u.address_line_1"; break;
                default: request.sort = "u.id, u.created_at"; break;
            }

            string sortQuery = $" ORDER BY {request.sort} {request.order} ";

            // ----------------------------------------------------
            // BASE QUERY (used for both count + data)
            // ----------------------------------------------------
            string baseQuery = @"
                            FROM ohd_User u
                            LEFT JOIN ohd_user_role_master ur ON ur.id = u.role_id
                            LEFT JOIN public.ohd_document doc ON u.proof_document_id = doc.id 
                            LEFT JOIN public.ohd_document_type dt ON doc.document_type = dt.id
                            LEFT JOIN public.ohd_user_wallet uw ON u.id = uw.user_id
                            LEFT JOIN public.ohd_property p ON u.id = p.owner_id
                            LEFT JOIN public.ohd_property_user_relation pur ON u.id = pur.user_id
                            LEFT JOIN public.ohd_property pur_prop ON pur.property_id = pur_prop.id
                            LEFT JOIN public.ohd_estate e ON p.estate_id = e.id OR pur_prop.estate_id = e.id
                            WHERE ur.id = @RoleId
                        ";

            // ----------------------------------------------------
            // STATUS FILTERS
            // ----------------------------------------------------
            switch (request.StatusId)
            {
                case (int)StatusEnum.All:
                    baseQuery += " AND u.status_id IN (@Active, @Deactive, @Inactive)";
                    parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                    parameters.Add("@Inactive", (int)StatusEnum.Inactive);
                    break;

                case (int)StatusEnum.Active:
                    baseQuery += " AND u.status_id IN (@Active, @Deactive)";
                    parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                    break;

                case (int)StatusEnum.Pending:
                    baseQuery += " AND u.status_id = @Pending";
                    parameters.Add("@Pending", (int)StatusEnum.Pending);
                    break;

                case (int)StatusEnum.Inactive:
                    baseQuery += " AND u.status_id = @Inactive";
                    parameters.Add("@Inactive", (int)StatusEnum.Inactive);
                    break;

                case (int)StatusEnum.NotVerified:
                    baseQuery += " AND u.isverified = @IsVerified AND u.status_id = @Active";
                    parameters.Add("@IsVerified", request.IsVerified);
                    break;
            }

            // ----------------------------------------------------
            // ESTATE FILTER
            // ----------------------------------------------------
            if (request.EstateId > 0)
            {
                parameters.Add("@EstateId", request.EstateId);
                baseQuery += " AND e.id = @EstateId AND pur.status_id = @Active";
            }

            // ----------------------------------------------------
            // SEARCH FILTER
            // ----------------------------------------------------
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var terms = request.SearchText.Trim()
                                              .ToLower()
                                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int i = 0;
                foreach (var term in terms)
                {
                    string param = "@Search" + i;
                    parameters.Add(param, $"%{term}%");

                    baseQuery += $@"
                AND (
                       LOWER(u.first_name) ILIKE {param}
                    OR LOWER(u.last_name) ILIKE {param}
                    OR LOWER(u.address_line_1) ILIKE {param}
                    OR LOWER(u.email) ILIKE {param}
                    OR LOWER(u.mobile) ILIKE {param}
                )";
                    i++;
                }
            }

            // ----------------------------------------------------
            // COUNT QUERY
            // ----------------------------------------------------
            string countQuery = "SELECT COUNT(DISTINCT u.id) " + baseQuery;
            int totalRecords = await _genericRepository.ExecuteScalarAsync<int>(countQuery, parameters).ConfigureAwait(false);

            // ----------------------------------------------------
            // PAGED DATA QUERY
            // ----------------------------------------------------
            string dataQuery = @"
        SELECT DISTINCT ON(u.id) u.id as Id,
            u.profile_url AS ProfileUrl,
            CONCAT(u.first_name, ' ', u.last_name) AS Consumer,
            u.address_line_1 AS Address,
            CONCAT(u.mobile, ' ', u.email) AS Contact,
            TO_CHAR(u.created_at::date, 'dd-MM-yyyy') AS CreateDate,
            doc.url AS DocumentUrl,
            u.status_id AS Status,
            uw.balance AS WalletBalance,
            u.isverified AS IsVerified,
            doc.doc_number AS DocNumber,
            doc.document_type AS DocTypeId,
            dt.name AS DocType,
            u.tax_number AS VATNumber
        " +
                baseQuery +
                sortQuery +
                " LIMIT @PageSize OFFSET @Offset";

            parameters.Add("@PageSize", request.PageSize);
            parameters.Add("@Offset", request.Page * request.PageSize);

            var consumers = await _genericRepository.GetAsync<ConsumerDto>(dataQuery, parameters).ConfigureAwait(false);
            var result = consumers.ToList();
            // ----------------------------------------------------
            // PROPERTY COUNTS
            // ----------------------------------------------------
            var propertyCounts = await _genericRepository.GetAsync<PropertyCountDto>(
                @"SELECT owner_id AS UserId, COUNT(*) AS PropertyCount
          FROM ohd_property 
          WHERE status_id = @Active 
          GROUP BY owner_id",
                new { Active = (int)StatusEnum.Active }
            );

            var meterCounts = await _genericRepository.GetAsync<MeterCountDto>(
                @"SELECT per.owner_id AS UserId, COUNT(mt.id) AS MeterCount
          FROM ohd_property per
          INNER JOIN ohd_meter mt ON mt.property_id = per.id AND mt.status_id = @Active
          WHERE per.status_id = @Active
          GROUP BY per.owner_id",
                new { Active = (int)StatusEnum.Active }
            );

            var meterNumbers = await _genericRepository.GetAsync<MeterNumberDto>(
                @"SELECT per.owner_id AS UserId, STRING_AGG(mt.meter_number, ', ') AS MeterNumbers
          FROM ohd_property per
          INNER JOIN ohd_meter mt ON mt.property_id = per.id AND mt.status_id = @Active
          WHERE per.status_id = @Active
          GROUP BY per.owner_id",
                new { Active = (int)StatusEnum.Active }
            ); 
            var companyhHelper = await _companyHelper.GetCompany(_workContext.CurrentCompanyId).ConfigureAwait(false);

            if (result.Any())
            {
                var task = result.Select(async i =>
                {
                    if (i.DocumentUrl != null)
                    {
                        i.DocumentUrl = companyhHelper.Domain + i.DocumentUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(i.DocumentUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var doc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(i.DocumentUrl),
                                Type = Path.GetExtension(i.DocumentUrl),
                                Document = fileBytes
                            };

                            i.Document = doc;
                        }
                        i.DocumentUrl = null;
                    }

                    if (i.ProfileUrl != null)
                    {
                        i.ProfileUrl = companyhHelper.Domain + i.ProfileUrl;
                        byte[] fileBytes = null;
                        fileBytes = await _genericRepository.GetDocumentAsBytesAsync(i.ProfileUrl).ConfigureAwait(false);
                        if (fileBytes != null)
                        {
                            var profileDoc = new DocumentResultDto
                            {
                                FileName = Path.GetFileName(i.ProfileUrl),
                                Type = Path.GetExtension(i.ProfileUrl),
                                Document = fileBytes
                            };

                            i.Profile = profileDoc;
                        }
                        i.ProfileUrl = null;
                    }

                    return i;
                });
                result = (await Task.WhenAll(task)).ToList();
            }
            // ----------------------------------------------------
            // MERGE RESULTS
            // ----------------------------------------------------
            var consumerDetails = (from c in result
                                   join pc in propertyCounts on c.Id equals pc.UserId into pcj
                                   from pc in pcj.DefaultIfEmpty()

                                   join mc in meterCounts on c.Id equals mc.UserId into mcj
                                   from mc in mcj.DefaultIfEmpty()

                                   join mn in meterNumbers on c.Id equals mn.UserId into mnj
                                   from mn in mnj.DefaultIfEmpty()

                                   select new ConsumerDto
                                   {
                                       Id = c.Id,
                                       ProfileUrl = c.ProfileUrl,
                                       Consumer = c.Consumer,
                                       Address = c.Address,
                                       Contact = c.Contact,
                                       CreateDate = c.CreateDate,
                                       DocumentUrl = c.DocumentUrl,
                                       Status = c.Status,
                                       WalletBalance = c.WalletBalance,
                                       DocNumber = c.DocNumber,
                                       DocTypeId = c.DocTypeId,
                                       DocType = c.DocType,
                                       IsVerified = c.IsVerified,
                                       VATNumber = c.VATNumber,
                                       Document=c.Document,
                                       Profile=c.Profile,
                                       Sources = $"Property {(pc?.PropertyCount ?? 0)} Meters {(mc?.MeterCount ?? 0)}",
                                       meter_numbers = mn?.MeterNumbers ?? ""
                                   }).ToList();

            // ----------------------------------------------------
            // SET FINAL RESPONSE
            // ----------------------------------------------------
           
            dt.Data = consumerDetails;
            dt.TotalRecords = totalRecords;
            return dt;
        }
        public async Task<IEnumerable<PropertyCountDto>> ConsumerWisePropertyCount()
        {
            var sQuery = @"SELECT owner_id AS UserId
                        COUNT(ID) AS property_count
                        FROM ohd_property
                        WHERE status_id = @Active 
                        GROUP BY owner_id";
            var parameters = new DynamicParameters();
            parameters.Add(" @Active", (int)StatusEnum.Active);
            try
            {
               return await _genericRepository.GetAsync<PropertyCountDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<PropertyCountDto>();
            }
        }
        public async Task<IEnumerable<MeterCountDto>> ConsumerWiseMeterCount()
        {
            var sQuery = @"SELECT  per.Owner_Id,COUNT(mt.id) AS meter_count
                                     FROM ohd_property AS per
                                     INNER JOIN ohd_Meter AS mt ON mt.property_id = per.id AND mt.status_id =  @Active
                                    WHERE per.status_id =  @Active
                                     GROUP BY per.Owner_Id  ";
            var parameters = new DynamicParameters();
            parameters.Add(" @Active", (int)StatusEnum.Active);
            try
            {
                return await _genericRepository.GetAsync<MeterCountDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<MeterCountDto>();
            }
        }
        public async Task<IEnumerable<MeterNumberDto>> ConsumerWiseMeters()
        {
            var sQuery = @"SELECT
                               per.Owner_Id,
                              STRING_AGG(mt.meter_number, ', ') AS meter_numbers
                          FROM ohd_property AS per
                         INNER JOIN ohd_Meter AS mt ON mt.property_id = per.id AND mt.status_id =  @Active
                         WHERE per.status_id =  @Active 
                        GROUP BY per.Owner_Id";
            var parameters = new DynamicParameters();
            parameters.Add(" @Active", (int)StatusEnum.Active);
            try
            {
                return await _genericRepository.GetAsync<MeterNumberDto>(sQuery, parameters).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return new List<MeterNumberDto>();
            }
        }

        //public async Task<ConsumerDashboardDto> GetConsumerDashboard()
        //{
        //    ConsumerDashboardDto dashboardDto = new();
        //    try
        //    {

            //        var totalPropertyQuery = @"SELECT COUNT(id)
            //                                FROM pubic.ohd_property
            //                                WHERE status_id=@Active;";


            //        //var totalEstatePropertyCount = @"SELECT COUNT(p.id)
            //        //                                FROM pubic.ohd_property as p
            //        //                                LEFT JOIN pubic.ohd_estate as e on p.estate_id=e.id
            //        //                                WHERE p.status_id=@Active AND p.estate_id=@EstateId";

            //  //      var totalPercentageQuery = @"WITH user_counts AS (SELECT
            //  //                                   (SELECT COUNT(u.ID) 
            //  //                                  FROM pubic.ohd_user as u
            //		//	LEFT JOIN pubic.ohd_property as p on u.id=p.owner_id	
            //		//	LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id
            //  //                                  WHERE u.created_at >= date_trunc('month', current_date) 
            //  //                                  AND u.created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
            //  //                                  AND u.status_id in  (@Active,@Deactive,@Inactive) AND u.role_id=@Customer AND p.estate_id=@EstateId)AS ThisMonthCount,
            //  //                                  (SELECT COUNT(u.ID) 
            //  //                                  FROM pubic.ohd_user  as u
            //  //                                  LEFT JOIN pubic.ohd_property as p on u.id=p.owner_id	
            //		//	LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id
            //  //                                  WHERE u.created_at <= (date_trunc('month', now())::date - 1)
            //  //                                  AND  u.status_id in  (@Active,@Deactive,@Inactive) AND u.role_id=@Customer AND p.estate_id=@EstateId) AS TillLastMonthCount,
            //  //                                  (SELECT COUNT(u.ID) FROM pubic.ohd_user as u
            //  //                                  LEFT JOIN pubic.ohd_property as p on u.id=p.owner_id	
            //		//	LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id
            //  //                                  WHERE  u.status_id in  (@Active) AND u.role_id=@Customer 
            //  //                                  AND p.estate_id=@EstateId) AS TotalUserCount
            //  //                                  )SELECT
            //  //                                   round(100*ThisMonthCount/CASE WHEN TotalUserCount =0 THEN 1 ELSE TotalUserCount END ,2)/100  AS TotalPercentage,ThisMonthCount,TillLastMonthCount,TotalUserCount,
            //  //                                  CASE WHEN ThisMonthCount >= (TotalUserCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalUserPercentageFlag
            //  //                                  FROM
            //  //                                  user_counts";

            //  //      var newPercentageQuery = @"WITH newuser AS ( SELECT
            //  //                                 (SELECT COUNT(u.ID)
            //  //                                  FROM pubic.ohd_user as u
            //  //                                  LEFT JOIN pubic.ohd_property as p on u.id=p.owner_id	
            //		//	LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id
            //  //                                 WHERE u.created_at >= (date_trunc('month', now())::date - 31) 
            //  //                                 AND u. created_at <= (date_trunc('month', now())::date - 1)  AND 
            //  //                                  u.status_id in(@Active,@Pending) 
            //  //                                  AND u.role_id=@Temporary AND p.estate_id=@EstateId) AS LastMonthCount,
            //  //                                 (SELECT COUNT(u.ID)
            //  //                               FROM ohd_user  as u
            //  //                                 LEFT JOIN pubic.ohd_property as p on u.id=p.owner_id	
            //		//	LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id
            //  //                   WHERE u.created_at >= date_trunc('month', current_date) 
            //  //                              AND u.created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
            //  //                              AND u.status_id in(@Active,@Pending) 
            //  //                              AND u.role_id=@Temporary
            //  //                              AND p.estate_id=@EstateId)  as NewUserCount
            //  //                                 )
            //  //                                 SELECT
            //  //                                 round(100 * NewUserCount / CASE WHEN LastMonthCount =0 THEN 1 ELSE LastMonthCount END ,2)/100  AS NewUserPercentage,NewUserCount,LastMonthCount,
            //  //                                 CASE WHEN NewUserCount <= LastMonthCount THEN 'Up' ELSE 'Down' END AS NewUserPercentageFlag
            //  //                                 FROM newuser;";

            //  //      var totalPropertyPercentageQuery = @"WITH property_counts AS (SELECT
            //  //                                          (SELECT COUNT(p.ID) 
            //  //                                          FROM pubic.ohd_property as p
            //  //                                          LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id    
            //  //                                          WHERE p.created_at >= date_trunc('month', current_date) 
            //  //                                          AND p.created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
            //  //                                          AND p.status_id =@Active AND p.estate_id=@EstateId)AS this_month_count,
            //  //                                          (SELECT COUNT(p.ID) 
            //  //                                          FROM pubic.ohd_property as p
            //  //                                          LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id    
            //  //                                          WHERE p.created_at <= (date_trunc('month', now())::date - 1)
            //  //                                          AND  p.status_id =@Active  AND p.estate_id=@EstateId) AS till_last_month_count,
            //  //                                          (SELECT COUNT(p.ID) FROM 
            //  //                                          public.ohd_property as p
            //  //                                           LEFT JOIN pubic.ohd_estate as e  on p.estate_id=e.id    
            //  //                                              WHERE  p.status_id =@Active  AND p.estate_id=@EstateId) AS total_count)
            //  //                                          SELECT
            //  //                                          (round( 100 * this_month_count/CASE WHEN total_count =0 THEN 1 ELSE total_count END )/100)  AS percentage ,
            //  //                                          CASE WHEN this_month_count >= (total_count-till_last_month_count) THEN 'Up' ELSE 'Down' END AS TotalPropertyPercentageFlag
            //  //                                          FROM
            //  //                                          property_counts;";

            //  //      var TotalActiveMeterQuery = @"  (SELECT COUNT(mt.ID) 
            //  //                              FROM ohd_meter as mt
            //		//LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id
            //  //                              WHERE mt.created_at >= date_trunc('month', current_date) 
            //  //                              AND mt.created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
            //  //                              AND mt.status_id =@Active AND p.estate_id=@EstateId)AS ThisMonthCount,
            //  //                              (SELECT COUNT(mt.ID) 
            //  //                              FROM ohd_meter as mt
            //		// LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id
            //  //                              WHERE mt.created_at <= (date_trunc('month', now())::date - 1)
            //  //                              AND  mt.status_id =@Active AND p.estate_id=@EstateId)) AS TillLastMonthCount,
            //  //                              (SELECT COUNT(mt.ID) FROM ohd_meter as mt
            //		// LEFT JOIN public.ohd_property as p on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id
            //		// WHERE  mt.status_id =@Active) AS TotalMeterCount)
            //  //                              SELECT
            //  //                              round(100*ThisMonthCount/CASE WHEN TillLastMonthCount =0 THEN 1 ELSE TillLastMonthCount END)/100  AS TotalPercentage ,ThisMonthCount,TillLastMonthCount,TotalMeterCount,
            //  //                              CASE WHEN ThisMonthCount >= (TotalMeterCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalmeterPercentageFlag
            //  //                              FROM
            //  //                              meter_counts;";


            //  //      var AllMeterCountQuery = @"WITH AllMeterCounts AS (SELECT
            //  //                              (SELECT COUNT(mt.ID) 
            //  //                              FROM public.ohd_meter as mt
            //  //                              LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id
            //  //                              WHERE mt.status_id =@Active AND p.estate_id=@EstateId AND created_at >= date_trunc('month', current_date) 
            //  //                              AND mt.created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
            //  //                              )AS ThisMonthCount,
            //  //                              (SELECT COUNT(mt.ID) 
            //  //                              FROM public.ohd_meter as mt
            //  //                               LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id
            //  //                              WHERE mt.status_id =@Active AND p.estate_id=@EstateId 
            //  //                              AND  mt.created_at <= (date_trunc('month', now())::date - 1)
            //  //                              ) AS TillLastMonthCount,
            //  //                              (SELECT COUNT(mt.ID) FROM
            //  //                              public.ohd_meter as mt
            //  //                                LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		// LEFT JOIN public.ohd_estate as e on p.estate_id=e.id   
            //  //                              WHERE mt.status_id =@Active
            //  //                               AND p.estate_id=@EstateId) AS TotalMeterCount)
            //  //                              SELECT
            //  //                               round(100*ThisMonthCount/CASE WHEN TotalMeterCount =0 THEN 1 ELSE TotalMeterCount END)/100   AS TotalPercentage ,ThisMonthCount,TillLastMonthCount,TotalMeterCount,
            //  //                              CASE WHEN ThisMonthCount >= (TotalMeterCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalmeterPercentageFlag
            //  //                              FROM
            //  //                              AllMeterCounts";




            //        var parameters = new DynamicParameters();
            //        parameters.Add("@Active", (int)StatusEnum.Active);

            //        parameters.Add("@Deactive", (int)StatusEnum.Deactive);
            //        parameters.Add("@Inactive", (int)StatusEnum.Inactive);
            //        parameters.Add("@Pending", (int)StatusEnum.Pending);
            //        parameters.Add("@Customer", (int)RoleMasterEnum.Customer);
            //        parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);


            //        var totalProperty = _genericRepository.ExecuteScalarAsync<int>(totalPropertyQuery, parameters);



            //        var allUtilityConsumerQuery = @" SELECT emt.name AS Type, COUNT(DISTINCT unique_meters.id) AS Count
            //                         FROM public.ohd_enum_meter_type AS emt
            //                         LEFT JOIN (
            //                             SELECT DISTINCT mt.id, mt.meter_type_id
            //                             FROM public.ohd_meter AS mt

            //                             LEFT JOIN public.ohd_property_user_relation AS pur ON pur.property_id = mt.property_id
            //                             LEFT JOIN public.ohd_user AS u ON u.id = pur.user_id
            //                         ) AS unique_meters ON unique_meters.meter_type_id = emt.id
            //                         GROUP BY emt.name;";
            //        var allUtilityConsumer = _genericRepository.GetAsync<Utility>(allUtilityConsumerQuery);

            //        var activeUtilityConsumerQuery = @"SELECT emt.name AS Type, COUNT(DISTINCT unique_meters.id) AS Count
            //                         FROM public.ohd_enum_meter_type AS emt
            //                         LEFT JOIN (
            //                             SELECT DISTINCT mt.id, mt.meter_type_id,mt.status_id
            //                             FROM public.ohd_meter AS mt
            //                                 LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
            //		 LEFT JOIN public.ohd_estate as e on p.estate_id=e.id   
            //                             LEFT JOIN public.ohd_property_user_relation AS pur ON pur.property_id = mt.property_id
            //                             LEFT JOIN public.ohd_user AS u ON u.id = pur.user_id
            //                         ) AS unique_meters ON unique_meters.meter_type_id = emt.id
            //                         GROUP BY emt.name,unique_meters.status_id
            //	HAVING unique_meters.status_id=@Active";

            //        var TotalUsers = _genericRepository.GetAsync<TotalUsersCount>(totalPercentageQuery, parameters);
            //        var NewUsers = _genericRepository.GetAsync<NewUsersCount>(newPercentageQuery, parameters);
            //        var activeUtilityConsumer = _genericRepository.GetAsync<Utility>(activeUtilityConsumerQuery, parameters);
            //        var TotalActiveMeter = _genericRepository.GetAsync<TotalActiveMeterCount>(TotalActiveMeterQuery, parameters);
            //        var AllMeterCount = _genericRepository.GetAsync<AllMeterCount>(AllMeterCountQuery, parameters);
            //        var TotalProperties = _genericRepository.GetAsync<int>(totalPropertyQuery, parameters);
            //        var totalPropertyPercentage = _genericRepository.GetAsync<PropertiesCount>(totalPropertyPercentageQuery, parameters);
            //        try
            //        {
            //            await Task.WhenAll(TotalUsers, NewUsers, TotalProperties, allUtilityConsumer, activeUtilityConsumer, totalPropertyPercentage, TotalActiveMeter, AllMeterCount).ConfigureAwait(false);
            //        }
            //        catch (Exception ex) { }
            //        dashboardDto.AllConsumerUtilities = allUtilityConsumer.Result.ToList();
            //        dashboardDto.ActiveConsumerUtilities = activeUtilityConsumer.Result.ToList();
            //        dashboardDto.TotalUsers = TotalUsers.Result.ToList();
            //        dashboardDto.NewUsers = NewUsers.Result.ToList();
            //        dashboardDto.TotalActiveMeters = TotalActiveMeter.Result.ToList();
            //        dashboardDto.AllMeters = AllMeterCount.Result.ToList();
            //        dashboardDto.Properties = totalPropertyPercentage.Result.ToList();
            //        dashboardDto.TotalPropertyCount = totalProperty.Result;



            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //    return dashboardDto;
            //}


        public async Task<ConsumerDashboardDto> GetConsumerDashboard()
        {
            ConsumerDashboardDto dashboardDto = new();
            try
            {

                var totalPropertyQuery = @"SELECT COUNT(id)
                                        FROM ohd_property
                                        WHERE status_id=@Active;";

                var totalPercentageQuery = @"WITH user_counts AS (SELECT
                                            (SELECT COUNT(ID) 
                                            FROM ohd_user 
                                            WHERE created_at >= date_trunc('month', current_date) 
                                            AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                            AND status_id in  (@Active,@Deactive,@Inactive) AND role_id=@Customer)AS ThisMonthCount,
                                            (SELECT COUNT(ID) 
                                            FROM ohd_user 
                                            WHERE created_at <= (date_trunc('month', now())::date - 1)
                                            AND  status_id in  (@Active,@Deactive,@Inactive) AND role_id=@Customer) AS TillLastMonthCount,
                                            (SELECT COUNT(ID) FROM ohd_user WHERE  status_id in  (@Active) AND role_id=@Customer) AS TotalUserCount
                                            )SELECT
                                             round(100*ThisMonthCount/CASE WHEN TotalUserCount =0 THEN 1 ELSE TotalUserCount END ,2)/100  AS TotalPercentage,ThisMonthCount,TillLastMonthCount,TotalUserCount,
                                            CASE WHEN ThisMonthCount >= (TotalUserCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalUserPercentageFlag
                                            FROM
                                            user_counts";

                var newPercentageQuery = @"WITH newuser AS ( SELECT
                                           (SELECT COUNT(ID)
                                            FROM ohd_user 
                                           WHERE created_at >= (date_trunc('month', now())::date - 31) 
                                           AND created_at <= (date_trunc('month', now())::date - 1)  AND status_id in(@Active,@Pending)AND role_id=@Temporary) AS LastMonthCount,
                                           (SELECT COUNT(ID)
                                         FROM ohd_user 
				                         WHERE created_at >= date_trunc('month', current_date) 
                                        AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                        AND status_id in(@Active,@Pending) AND role_id=@Temporary)  as NewUserCount
                                           )
                                           SELECT
                                           round(100 * NewUserCount / CASE WHEN LastMonthCount =0 THEN 1 ELSE LastMonthCount END ,2)/100  AS NewUserPercentage,NewUserCount,LastMonthCount,
                                           CASE WHEN NewUserCount <= LastMonthCount THEN 'Up' ELSE 'Down' END AS NewUserPercentageFlag
                                           FROM newuser;";

                var totalPropertyPercentageQuery = @"WITH property_counts AS (SELECT
                                                    (SELECT COUNT(ID) 
                                                    FROM ohd_property 
                                                    WHERE created_at >= date_trunc('month', current_date) 
                                                    AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                                    AND status_id =@Active)AS this_month_count,
                                                    (SELECT COUNT(ID) 
                                                    FROM ohd_property 
                                                    WHERE created_at <= (date_trunc('month', now())::date - 1)
                                                    AND  status_id =@Active) AS till_last_month_count,
                                                    (SELECT COUNT(ID) FROM ohd_property WHERE  status_id =@Active) AS total_count)
                                                    SELECT
                                                    (round( 100 * this_month_count/CASE WHEN total_count =0 THEN 1 ELSE total_count END )/100)  AS percentage ,
                                                    CASE WHEN this_month_count >= (total_count-till_last_month_count) THEN 'Up' ELSE 'Down' END AS TotalPropertyPercentageFlag
                                                    FROM
                                                    property_counts;";

                var TotalActiveMeterQuery = @" WITH meter_counts AS (SELECT
                                        (SELECT COUNT(ID) 
                                        FROM ohd_meter 
                                        WHERE created_at >= date_trunc('month', current_date) 
                                        AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                        AND status_id =@Active)AS ThisMonthCount,
                                        (SELECT COUNT(ID) 
                                        FROM ohd_meter 
                                        WHERE created_at <= (date_trunc('month', now())::date - 1)
                                        AND  status_id =@Active) AS TillLastMonthCount,
                                        (SELECT COUNT(ID) FROM ohd_meter WHERE  status_id =@Active) AS TotalMeterCount)
                                        SELECT
                                        round(100*ThisMonthCount/CASE WHEN TillLastMonthCount =0 THEN 1 ELSE TillLastMonthCount END)/100  AS TotalPercentage ,ThisMonthCount,TillLastMonthCount,TotalMeterCount,
                                        CASE WHEN ThisMonthCount >= (TotalMeterCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalmeterPercentageFlag
                                        FROM
                                        meter_counts;";


                var AllMeterCountQuery = @"WITH AllMeterCounts AS (SELECT
                                        (SELECT COUNT(ID) 
                                        FROM ohd_meter 
                                        WHERE status_id =@Active AND created_at >= date_trunc('month', current_date) 
                                        AND created_at <= date_trunc('month', current_date) + interval '1 month' - interval '1 day'
                                        )AS ThisMonthCount,
                                        (SELECT COUNT(ID) 
                                        FROM ohd_meter 
                                        WHERE status_id =@Active AND  created_at <= (date_trunc('month', now())::date - 1)
                                        ) AS TillLastMonthCount,
                                        (SELECT COUNT(ID) FROM ohd_meter WHERE status_id =@Active) AS TotalMeterCount)
                                        SELECT
                                         round(100*ThisMonthCount/CASE WHEN TotalMeterCount =0 THEN 1 ELSE TotalMeterCount END)/100   AS TotalPercentage ,ThisMonthCount,TillLastMonthCount,TotalMeterCount,
                                        CASE WHEN ThisMonthCount >= (TotalMeterCount-TillLastMonthCount) THEN 'Up' ELSE 'Down' END AS TotalmeterPercentageFlag
                                        FROM
                                        AllMeterCounts";


                var parameters = new DynamicParameters();
                parameters.Add("@Active", (int)StatusEnum.Active);
                parameters.Add("@Reject", (int)StatusEnum.Rejected);
                parameters.Add("@Deactive", (int)StatusEnum.Deactive);
                parameters.Add("@Inactive", (int)StatusEnum.Inactive);
                parameters.Add("@Pending", (int)StatusEnum.Pending);
                parameters.Add("@Customer", (int)RoleMasterEnum.Customer);
                parameters.Add("@Temporary", (int)RoleMasterEnum.Temporary);


                var totalProperty = _genericRepository.ExecuteScalarAsync<int>(totalPropertyQuery, parameters);



                var allUtilityConsumerQuery = @"SELECT emt.name AS Type, COUNT(DISTINCT unique_meters.id) AS Count
                                 FROM public.ohd_enum_meter_type AS emt
                                 LEFT JOIN (
                                     SELECT DISTINCT mt.id, mt.meter_type_id,mt.status_id
                                     FROM public.ohd_meter AS mt 									 
                                     LEFT JOIN public.ohd_property_user_relation AS pur ON pur.property_id = mt.property_id
                                     LEFT JOIN public.ohd_user AS u ON u.id = pur.user_id
                                 ) AS unique_meters ON unique_meters.meter_type_id = emt.id AND unique_meters.status_id in(@Active,@Reject,@Pending)
                                 GROUP BY emt.name ;";
                var allUtilityConsumer = _genericRepository.GetAsync<Utility>(allUtilityConsumerQuery,parameters);

                var activeUtilityConsumerQuery = @"SELECT emt.name AS Type, COUNT(DISTINCT unique_meters.id) AS Count
                                 FROM public.ohd_enum_meter_type AS emt
                                 LEFT JOIN (
                                     SELECT DISTINCT mt.id, mt.meter_type_id,mt.status_id
                                     FROM public.ohd_meter AS mt
                                         LEFT JOIN public.ohd_property as p 	on mt.property_id=p.id
										 LEFT JOIN public.ohd_estate as e on p.estate_id=e.id   
                                     LEFT JOIN public.ohd_property_user_relation AS pur ON pur.property_id = mt.property_id
                                     LEFT JOIN public.ohd_user AS u ON u.id = pur.user_id
                                 ) AS unique_meters ON unique_meters.meter_type_id = emt.id
                                 GROUP BY emt.name,unique_meters.status_id
									HAVING unique_meters.status_id=@Active";

                var TotalUsers = _genericRepository.GetAsync<TotalUsersCount>(totalPercentageQuery, parameters);
                var NewUsers = _genericRepository.GetAsync<NewUsersCount>(newPercentageQuery, parameters);
                var activeUtilityConsumer = _genericRepository.GetAsync<Utility>(activeUtilityConsumerQuery, parameters);
                var TotalActiveMeter = _genericRepository.GetAsync<TotalActiveMeterCount>(TotalActiveMeterQuery, parameters);
                var AllMeterCount = _genericRepository.GetAsync<AllMeterCount>(AllMeterCountQuery, parameters);
                var TotalProperties = _genericRepository.GetAsync<int>(totalPropertyQuery, parameters);
                var totalPropertyPercentage = _genericRepository.GetAsync<PropertiesCount>(totalPropertyPercentageQuery, parameters);
                try
                {
                    await Task.WhenAll(TotalUsers, NewUsers, TotalProperties, allUtilityConsumer, activeUtilityConsumer, totalPropertyPercentage, TotalActiveMeter, AllMeterCount).ConfigureAwait(false);
                }
                catch (Exception ex) { }
                dashboardDto.AllConsumerUtilities = allUtilityConsumer.Result.ToList();
                dashboardDto.ActiveConsumerUtilities = activeUtilityConsumer.Result.ToList();
                dashboardDto.TotalUsers = TotalUsers.Result.ToList();
                dashboardDto.NewUsers = NewUsers.Result.ToList();
                dashboardDto.TotalActiveMeters = TotalActiveMeter.Result.ToList();
                dashboardDto.AllMeters = AllMeterCount.Result.ToList();
                dashboardDto.Properties = totalPropertyPercentage.Result.ToList();
                dashboardDto.TotalPropertyCount = totalProperty.Result;



            }
            catch (Exception ex)
            {

            }
            return dashboardDto;
        }

        public async Task<int> UpdateConsumer(UpdateConsumerQuery request)
        {
            try
            {

                var sQuery = @" UPDATE ohd_user
                            SET 
                               first_name = @FirstName
                               ,last_name = @LastName 
                                ,address_line_1=@Address
                               ,status_id = @Status
                               ,modified_at = @ModifiedAt
                             WHERE id = @Id ;
                            Select Id From ohd_user
                             WHERE id = @Id ";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", request.Id);
                parameters.Add("@FirstName", request.FirstName);
                parameters.Add("@LastName", request.LastName);
                parameters.Add("@Address", request.Address);
                parameters.Add("@Status", (request.Status ? (int)StatusEnum.Active : (int)StatusEnum.Inactive));
                parameters.Add("@ModifiedAt", DateTime.UtcNow);

                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<bool> IsConsumerExist(int id)
        {
            var sQuery = @"SELECT count(*)
                            FROM ohd_user  
                          WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            return result > 0;
        }
        public async Task DeleteConsumerById(int userId)
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

        public async Task<MeterAndUserRequestDto> GetMeterAndUserRequestCount()
        {
            MeterAndUserRequestDto requestDto = new();
            var userRequestCountquery = @"SELECT count (id)
                                    FROM ohd_user
                                    WHERE status_id=@StatusId and role_id=@customer";


            var meterRequestCountQuery = @"SELECT count (id)
                                    FROM ohd_meter
                                    WHERE status_id=@StatusId";

            var parameters = new DynamicParameters();
            parameters.Add("@StatusId", (int)StatusEnum.Pending);
            parameters.Add("@customer", (int)RoleMasterEnum.Customer);

            var totalUserRequest = _genericRepository.ExecuteScalarAsync<int>(userRequestCountquery, parameters);
            var totalMeterRequest = _genericRepository.ExecuteScalarAsync<int>(meterRequestCountQuery, parameters);
            await Task.WhenAll(totalUserRequest, totalMeterRequest).ConfigureAwait(false);
            requestDto.UserRequestCount = totalUserRequest.Result;
            requestDto.MeterRequestCount = totalMeterRequest.Result;
            return requestDto;

        }

        public async Task<IEnumerable<ConsumerGroupDto>> GetConsumerGroupsById(int userId)
        {
            var sQuery = @"SELECT 
                            distinct(ng.group_name) AS Group, 
                            ncgl.created_at AS CreatedAt,
                            ncgl.id AS GroupLinkId,
                            ng.id AS GroupId                            
                            FROM public.ohd_notification_custome_group_linking as ncgl
                            LEFT JOIN public.ohd_notificationgroups as ng on ncgl.group_id=ng.id
                            WHERE ncgl.customer_id=@Id and ncgl.status_id=@StatusId and ng.status_id=@StatusId
                            ORDER BY ng.group_name asc";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", userId);
            parameters.Add("@StatusId", (int)StatusEnum.Active);
            return await _genericRepository.GetAsync<ConsumerGroupDto>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> RemoveConsumerFromGroup(int groupLinkId)
        {
            var sQuery = @"UPDATE public.ohd_notification_custome_group_linking
                            SET status_id=@InActive                                                      
                            WHERE id=@Id;
                            SELECT id FROM public.ohd_notification_custome_group_linking
                            WHERE id=@Id";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", groupLinkId);
            parameters.Add("@InActive", (int)StatusEnum.Inactive);
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<int> IsNotifcationGroupLinkIdExist(int groupLinkId)
        {
            var sQuery = @"SELECT id FROM public.ohd_notification_custome_group_linking
                            WHERE id=@Id AND status_id=@Active";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", groupLinkId);
            parameters.Add("@Active", (int)StatusEnum.Active);
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
    }
}
