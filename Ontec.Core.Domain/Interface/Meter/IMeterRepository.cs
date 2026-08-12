using Ontec.Core.Application.Common.Helper;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.Meter;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Meter.Queries;

namespace Ontec.Core.Domain.Interface.Meter
{
    public interface IMeterRepository
    {
        Task<int> GetMeterCountByPropertyId(int propertyId);
        Task<IEnumerable<MeterDto>> GetMetersByPropertyId(int propertyId);
        Task<bool> IsMeterIdExist(int meterId);
        Task DeleteMeterById(int meterId);
        Task<IEnumerable<string>> GetMeterTypesByPropertyId(int propertyId);
        Task<UpdateMeterDto> GetMeterById(int id);
        Task<UpdateMeterDto> GetMeterByMeterNumber(string meterNumber);
        Task<bool> IsMeterNumberDeactivated(string meter);
        Task<UpdateMeterDto> GetMeterDetails(string meterNumber);

        Task<EditMeterMasters> GetEditMeterMasters(int userId);
        Task<bool> IsMeterTypeIdExist(int id);
        
        Task<int> UpdateMeter(UpdateMeterDto request);
        Task<int> AddMeter(AddUpdateMeterQuery request, int meterMasterTypeId,string eftNo,bool isverified);
        Task<MeterConsumptionUnitDto> GetTargetConsumptionByMeterNumber(string meterNumber);

        #region GetMeterRequest
        Task<DatatableModel<MeterRequestDto>> GetMeter(GetMeterRequestQuery request);
        #endregion

        #region ApproveRejectMeterByID
        Task<bool> IsPendingMeterIdExist(int meterId);
        Task<int> ApproveRejectMeterById(GetApproveRejectMeterQuery request);

        #endregion
        
        Task<IEnumerable<DashboardMeterDayConsumptionDto>> GetMeterMasterByPropertyId(int propertyId);
        Task<int> AddMeterMasterType(string meterMasterType, int MeterTypeId);
        Task<bool> IsExistMeterMasterType(string meterMasterType);
        
        Task<MeterIdWithNumberDto> GetOwnerIdByMeterId(int meterId);
        Task<IEnumerable<MeterExpiringDto>> GetMeterExpiringDtos(int userId);
        Task<IEnumerable<Utilities>> GetMeterTypes();
        Task<int> IsMeterExist(int meterId);
        Task<int> AddMetersFromMeterList(AddMetersFromListHelperClass obj);
        
        Task<IEnumerable<MeterDto>> GetAllMetersByPropertyId(int propertyId);
        Task<IEnumerable<MeterDto>> GetAllMeters();
        
        Task<IEnumerable<MeterTypes>> GetMeterType();
        Task<MeterDto> GetMetersById(int meterId);

        Task<int> IsEftNoExist(string eftNumber);
        Task<int> IsActivePendingRejectedMeterNumberExist(string meterNumber, int meterId);

        Task<int> IsMeterNumberExist(string meterNumber);
        Task<int> GetPropertyIdByMeterNumber(string  meterNumber);
        Task<int> IsMeterSolar(int propertyId);
        Task<int> GetPropertyIdByEFTNumber(string eftNumber);
        Task<int> GetMeterIdByEFTNumber(string eftNumber);
        Task<int> UpdateEFTNumberByMeterId(int meterId, string eftNo);
        Task<string> GetUnitOfMeasure(string meter);
        Task<int> UpdateMeterMasterType(int id, string masterMeterType);
    }
}
