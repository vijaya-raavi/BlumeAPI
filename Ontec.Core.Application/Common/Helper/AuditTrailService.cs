using Dapper;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Requests.Common.Queries;

namespace Ontec.Core.Application.Common.Helper
{
    public class AuditTrailService : IAuditTrail
    {
        private readonly IGenericRepository _genericRepository;
        private readonly ILogger<AuditTrailService> _logger;
        private readonly IWorkContext _workContext;
        public AuditTrailService(IGenericRepository genericRepository,
                                ILogger<AuditTrailService> logger,IWorkContext workContext)
        {
            _genericRepository = genericRepository;
            _logger = logger;
            _workContext = workContext;
        }
        public async Task<int> AuditTrail(AuditHelper auditHelper)
        {
            var sQuery = @"INSERT INTO public.ohd_audit_trail 
                          (updated_id,
                            action,
                            action_table,
                            added_on,
                            module,
                            added_by,
                            entity_name,
                            modified_by,status_id)
                            VALUES
                            (@UpdatedId,
                            @Action,
                            @ActionTable,
                            @AddedOn,
                            @Module,
                            @AddedBy,@EntityName,
                             @ModifiedBy,@StatusId)
                            RETURNING lastval()";

            var parameters = new DynamicParameters();
            parameters.Add("@Action", auditHelper.Action);
            parameters.Add("@ActionTable", auditHelper.ActionTable);
            parameters.Add("@AddedOn", DateTime.UtcNow);
            parameters.Add("@Module", auditHelper.ModuleName);
            parameters.Add("@EntityName", auditHelper.EntityName);
            if (auditHelper.ModifiedBy.HasValue)
            {
                parameters.Add("@ModifiedBy", auditHelper.ModifiedBy);
            }
            else
            {
                parameters.Add("@ModifiedBy", 0);
            }
            parameters.Add("@AddedBy", auditHelper.AddedBy);
            parameters.Add("@UpdatedId", auditHelper.UpdatedId);
            if (auditHelper.StatusId.HasValue)
            {
                parameters.Add("@StatusId", auditHelper.StatusId);
            }
            else
            {
                parameters.Add("@StatusId", 0);
            }
            try
            {
                var id = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }

        public async Task<DatatableModel<AuditTrailDto>> GetAuditTrail(GetAuditTrailQuery request)
        {
            bool whereAdded = false;
            var dt = new DatatableModel<AuditTrailDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                SearchText = request.SearchText,
            };
            string orderQuery = "";

            var parameters = new DynamicParameters();
            
            
            DateTime fromDate;
            DateTime toDate;

            fromDate = Convert.ToDateTime(request.FromDate).Date;
            toDate = Convert.ToDateTime(request.ToDate).Date;
            parameters.Add("@FromDate", fromDate.AddHours(00).AddMinutes(00).AddSeconds(00));
            parameters.Add("@ToDate", toDate.AddHours(23).AddMinutes(59).AddSeconds(59));
            var baseQuery = @" FROM public.ohd_audit_trail at
                                    LEFT JOIN public.ohd_user u
                                        ON u.id = CASE
                                                    WHEN at.added_by > 0 THEN at.added_by
                                                    ELSE at.modified_by
                                                  END
                                    WHERE (at.added_on >=  @FromDate AND at.added_on <= @ToDate OR 
                                     at.modified_at>= @FromDate AND at.modified_at <= @ToDate )       
                                    
                                ";

            if (request.UserId > 0)
            {
                baseQuery += @" AND (at.added_by = @UserId OR at.modified_by = @UserId)";
                parameters.Add("@UserId", request.UserId);

            }
            try
            {
               
                var dataQuery = @"SELECT 
                                at.id AS Id,
                                CASE
                                 WHEN at.added_by > 0 THEN at.added_by ELSE at.modified_by END AS UserId,
                                CONCAT( u.first_name,' ' , u.last_name) AS UserName,
                                at.entity_name AS EntityName,
                                at.action AS Action,
                                at.action_table AS ActionTable,
                                at.module AS Module,
                                at.added_on AS AddedOn " + baseQuery;

              

               
                if (!string.IsNullOrWhiteSpace(request.SearchText) && request.SearchText.Trim() != "string")
                {
                    var terms = request.SearchText.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    int index = 0;
                    foreach (var t in terms)
                    {
                        string param = "@Search" + index;
                        parameters.Add(param, "%" + t + "%");

                        baseQuery += $@" AND (
                                         LOWER(u.first_name) ILIKE {param}
                                            LOWER(u.last_name) ILIKE {param}
                                         OR LOWER(at.module) ILIKE {param}
                                         OR LOWER(at.action) ILIKE {param}
                                         OR LOWER(at.action_table) ILIKE {param}
                                       )";

                        index++;
                    }
                }

                string countQuery = $"SELECT COUNT(DISTINCT at.id) {baseQuery}";
                int total = await _genericRepository.ExecuteScalarAsync<int>(countQuery, parameters);

                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@Offset", request.Page * request.PageSize);
                orderQuery = " ORDER BY at.added_on asc";
                if (request.PageSize > 0)
                {
                    orderQuery += @" LIMIT @PageSize OFFSET @Offset";
                }
                baseQuery += orderQuery;
                var rows = await _genericRepository.GetAsync<AuditTrailDto>(dataQuery, parameters);

                dt.TotalRecords = total;
                dt.Data = rows.ToList();
                return dt;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAuditTrail error: " + ex.Message);
                dt.Data = new List<AuditTrailDto>();
                dt.TotalRecords = 0;
            }

            return dt;
        }

    }
}
