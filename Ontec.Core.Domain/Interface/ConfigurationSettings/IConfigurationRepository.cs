using Dapper;
using Ontec.Core.Domain.Models.Dto.Configuration;
using Ontec.Core.Domain.Requests.Configuration.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Command;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Queries;
using Ontec.Core.Domain.Requests.Content.Command;

namespace Ontec.Core.Domain.Interface.Configuration
{
    public  interface IConfigurationRepository
    {
        Task UpdateConfiguration(AddOrUpdateConfigurationQuery request);
        Task<IEnumerable<ConfigurationDto>> GetConfigurations();
        Task DeleteConfigurationById(int Id);
        Task<int> IsIdExist(int id);
        Task<int> UpdateAppBackGroundImage(string AppBackGroundImage, int id);
        Task<int> IsReasonExist(AddUpdateRejectionReasonQuery request);
        Task<int> AddRejectionReason(AddUpdateRejectionReasonQuery request);
        Task<int> UpdateRejectionReason(AddUpdateRejectionReasonQuery request);
        Task<IEnumerable<RejectionReasonDto>> GetRejectionReason(GetRejectionReasonQuery request);
        Task<int> UpdateStatus(UpdateStatusQuery request);
        Task<int> IsReasonIdExist(int id);
        Task<IEnumerable<BusinessHoursConfigurationsDto>> GetBusinessHoursConfigurations();
        Task UpdateBusinessConfiguration(UpdateBusinessHoursConfigurations request);
        Task<int> IsConfigIdExist(int id);
        Task<IEnumerable<ConfigurationDto>> GetNotEditableConfigurations();
        Task UpdateMeterUtilityTypeDailyTarget(AddUpdateUtilityTypeDetailsCommandRequest request);

    }
}
