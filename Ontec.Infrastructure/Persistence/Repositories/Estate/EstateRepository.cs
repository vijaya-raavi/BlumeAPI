using Dapper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Models.Dto.Estate;
using Ontec.Core.Domain.Requests.Estate.Command;

namespace Ontec.Infrastructure.Persistence.Repositories.Estate
{
    public class EstateRepository : IEstateRepository
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IPropertyRepository _propertyRepository;
        public EstateRepository(IGenericRepository genericRepository
                                 , IMeterRepository meterRepository, IPropertyRepository propertyRepository)
        {
            _genericRepository = genericRepository;
            _propertyRepository = propertyRepository;
        }
        public async Task<int> AddEstate(AddEstateRequestCommand request)
        {
            try
            {
                var sQuery = @"INSERT INTO public.ohd_estate(
	                        estate, 
                            status_id, 
                            created_at)
	                        VALUES (@Estate,
                            @StatusId,
                            @CreatedAt
                            )RETURNING lastval();";
                var parameters = new DynamicParameters();

                parameters.Add("@Estate", request.Estate);
                parameters.Add("@StatusId", request.StatusId);
                parameters.Add("@CreatedAt", DateTime.UtcNow);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }

        }
        public async Task<int> UpdateEstate(AddEstateRequestCommand request)
        {
            try
            {
                var sQuery = @"UPDATE  public.ohd_estate
	                        SET estate=@Estate,
                            status_id=@StatusId,
                            modified_at=@ModifiedAt
                            WHERE id=@Id;
                            SELECT id FROM public.ohd_estate
                            WHERE id=@Id;";
                var parameters = new DynamicParameters();

                parameters.Add("@Estate", request.Estate);
                parameters.Add("@StatusId", request.StatusId);
                parameters.Add("@Id", request.Id);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }
        }
        public async Task<int> DeleteEstate(int id)
        {
            try
            {
                var sQuery = @"UPDATE  public.ohd_estate
	                        SET
                            status_id=@StatusId,
                            modified_at=@ModifiedAt
                            WHERE id=@Id;
                            SELECT id FROM public.ohd_estate
                            WHERE id=@Id;";
                var parameters = new DynamicParameters();

                parameters.Add("@StatusId", (int)StatusEnum.Inactive);
                parameters.Add("@ModifiedAt", DateTime.UtcNow);
                parameters.Add("@Id", id);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }
        }

        public async Task<int> IsEstateIdExist(int id)
        {
            try
            {
                var sQuery = @"SELECT count(id) FROM public.ohd_estate
                               WHERE id=@Id ";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@Active", (int)StatusEnum.Active);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }

        }
        public async Task<int> IsActiveEstateIdExist(int id)
        {
            try
            {
                var sQuery = @"SELECT count(id) FROM public.ohd_estate
                               WHERE id=@Id AND status_id=@Active";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@Active", (int)StatusEnum.Active);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }

        }
        public async Task<int> IsActiveEstateExist(string  estate)
        {
            try
            {
                var sQuery = @"SELECT count(id) FROM public.ohd_estate
                               WHERE estate=@Estate AND status_id=@Active";
                var parameters = new DynamicParameters();
                parameters.Add("@Estate", estate);
                parameters.Add("@Active", (int)StatusEnum.Active);


                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return 0;

            }

        }
        public async Task<IEnumerable<EstateDto>> GetEstateList()
        {
            try
            {
                var sQuery = @"SELECT e.id AS Id, 
                                e.estate AS Estate, 
                                e.status_id AS StatusId, 
                                 to_char(e.created_at,'dd-MM-yyyy') AS CreatedAt,
                                e.modified_at As ModifiedAt,
							    es.name AS Status,
                                ng.id as GroupId,
								ng.topic_name as topic
                         FROM public.ohd_estate as e
						 LEFT JOIN public.ohd_enum_status as es on e.status_id=es.id
                        LEFT JOIN public.ohd_notificationgroups as ng on e.id=ng.estate_id
                        WHERE e.status_id=@StatusId order by e.estate asc";
                var parameters = new DynamicParameters();
                parameters.Add("@StatusId", (int)StatusEnum.Active);
                var result = await _genericRepository.GetAsync<EstateDto>(sQuery, parameters).ConfigureAwait(false);

                //var estates= result.ToList();
                //if (estates.Any())
                //{
                //    estates.Insert(0, new EstateDto
                //    {
                //        Id = 0,
                //        Estate = "All",
                //        StatusId = 0,
                //        CreatedAt = null,                      
                //        Status = null
                //    });
                //}
                //return estates;
                return result;
            }
            catch (Exception e)
            {
                return new List<EstateDto>();

            }

        }
        public async Task<EstateDto> GetEstateById(int id)
        {
            try
            {
                var sQuery = @"SELECT e.id AS Id, 
                                e.estate AS Estate, 
                                e.status_id AS StatusId, 
                                 to_char(e.created_at,'dd-MM-yyyy') AS CreatedAt,
                                e.modified_at As ModifiedAt,
							    es.name AS Status
                         FROM public.ohd_estate as e
						 LEFT JOIN public.ohd_enum_status as es on e.status_id=es.id
                            WHERE e.id=@Id;";
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                var result = await _genericRepository.GetFirstOrDefaultAsync<EstateDto>(sQuery, parameters).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                return new EstateDto();

            }

        }
        public async Task<IEnumerable<int>> GetPropertiesByEstateId(int estateId)
        {
            try
            {
                var sQuery = @"SELECT id FROM public.ohd_property
                              WHERE estate_id=@EstateId and status_id=@Active";
                var parameters = new DynamicParameters();
                parameters.Add("@EstateId", estateId);
                parameters.Add("@Active", (int)StatusEnum.Active);
                List<int> ids = (List<int>)await _genericRepository.GetAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return ids;
            }
            catch (Exception e)
            {
                return new List<int>();

            }
        }
        public async Task<IEnumerable<int>> GetPropertiesOwnerById(List<int> propertyIds)
        {
            try
            {
                var sQuery = @"SELECT owner_id FROM public.ohd_property
                              WHERE id=ANY(@PropertyId) and status_id=@Active";
                var parameters = new DynamicParameters();
                parameters.Add("@PropertyId", propertyIds);
                parameters.Add("@Active", (int)StatusEnum.Active);
                List<int> ids = (List<int>)await _genericRepository.GetAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return ids;
            }
            catch (Exception e)
            {
                return new List<int>();

            }
        }
        public async Task<IEnumerable<int>> GetPropertyUserIdById(List<int> propertyIds)
        {
            try
            {
                var sQuery = @"SELECT user_id FROM public.ohd_property_user_relation
                                WHERE property_id=ANY(@PropertyId) and status_id=@Active";
                var parameters = new DynamicParameters();
                parameters.Add("@PropertyId", propertyIds);
                parameters.Add("@Active", (int)StatusEnum.Active);
                List<int> ids = (List<int>)await _genericRepository.GetAsync<int>(sQuery, parameters).ConfigureAwait(false);
                return ids;
            }
            catch (Exception e)
            {
                return new List<int>();

            }
        }
    }
}
