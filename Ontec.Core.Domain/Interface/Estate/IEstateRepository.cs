using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Estate;
using Ontec.Core.Domain.Requests.Estate.Command;

namespace Ontec.Core.Domain.Interface.Estate
{
    public interface IEstateRepository
    {
        Task<int> AddEstate(AddEstateRequestCommand request);
        Task<int> UpdateEstate(AddEstateRequestCommand request);
        Task<int> DeleteEstate(int id);
        Task<int> IsEstateIdExist(int id);
        Task<IEnumerable<EstateDto>> GetEstateList();
        Task<EstateDto> GetEstateById(int id);
        Task<IEnumerable<int>> GetPropertiesByEstateId(int estateId);
        Task<IEnumerable<int>> GetPropertiesOwnerById(List<int> propertyIds);
        Task<IEnumerable<int>> GetPropertyUserIdById(List<int> propertyIds);
        Task<int> IsActiveEstateIdExist(int id);
        Task<int> IsActiveEstateExist(string estate);
    }
}
