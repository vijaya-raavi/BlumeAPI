using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Models.Dto.Transaction;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.Property.Handler;
using Ontec.Core.Domain.Requests.PropertyUser.Command;
using Ontec.Core.Domain.Requests.Transaction.Queries;

namespace Ontec.Core.Domain.Interface.Property
{
    public interface IPropertyRepository
    {
        Task<IEnumerable<PropertyDto>> GetPropertiesByOwnerId(GetPropertiesByOwnerIdQuery request);
        Task<int> AddProperty(AddOrUpdatePropertyQuery request);
        Task<PropertyModelDto> GetPropertyById(int propertyId);
        Task<int> UpdateProperty(PropertyModelDto request);
        Task<bool> IsPropertyIdExist(int propertyId);
       // Task DeletePropertyById(int propertyId);
        Task<bool> IsUnitNumberExist(string unitNumber, int propertyId);
        Task<int> GetPropertyUsersCountByPropertyId(int propertyId, int propertyUserTypeId);
        Task<IEnumerable<ConsumptionPropertyList>> GetConsumptionPropertyList(int userId, bool isAdmin,int consumerId);
        Task<IEnumerable<string>> GetMeterNumbersByPropertyId(int propertyId);
        Task<PropertyDetailsDto> GetPropertyDashboard(int userId);
        Task<IEnumerable<DashboardMastersDto>> GetDashboardMastersDtoByUserId(int userId);
        Task<IEnumerable<TransactionPropertyList>> GetTransactionPropertyList(int userId, bool isAdmin);
        Task<PropertyUserDetail> GetPropertyUserByMeterId(int propertyId);
        Task<int> UpdatePropertyCustomerAgreementId(int propertyId, string CustomerAgreementId);
        Task<string> GetCustomerAgreementId(int propertyId);
        Task<PropertyCustomerAgreementDto> IsCustomerAgreementIdExist(string CustAgrrementId);
        Task<int> UpdateCustomerAgreementId(int propertyId);
        Task<DatatableModel<PropertyDto>> GetAllProperties(GetAllPropertiesRequestQuery request);
        Task<int> DeletePropertyById(DeletePropertyById request);
        Task<bool> IsPropertyExist(int propertyId);
        Task<PropertyModelDto> GetAllPropertyById(int propertyId);
        Task<DatatableModel<PropertyDto>> GetAllPropertiesNew(GetAllPropertiesRequestQuery request);
        Task<IEnumerable<PropertyMeter>> GetAllMeterNumbersByPropertyId(int propertyId);
        Task<IEnumerable<LinkedProperty>> GetMeterLinkedProperty(List<string> meterNumbers);
        Task<int> UpdateMeterStatus(List<int> ids);
        Task<int> UpdateCustomerAgreementValueByPropertyId(int propertyId, string value);
    }
}
