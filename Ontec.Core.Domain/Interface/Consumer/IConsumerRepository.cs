using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Consumer;
using Ontec.Core.Domain.Models.Dto.Dashboard;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.Consumer.Commands;
using Ontec.Core.Domain.Requests.Consumer.Queries;

namespace Ontec.Core.Domain.Interface.Consumer
{
    public interface IConsumerRepository
    {
        Task<ConsumerMasterDto> GetConsumerMasters();
        Task<DatatableModel<ConsumerDto>> GetConsumers(GetConsumersQuery request);
        Task<ConsumerDashboardDto> GetConsumerDashboard();
        Task<int> UpdateConsumer(UpdateConsumerQuery request);
        Task DeleteConsumerById(int userId);
        Task<bool> IsConsumerExist(int userId);
        Task<MeterAndUserRequestDto> GetMeterAndUserRequestCount();
        Task<IEnumerable<ConsumerGroupDto>> GetConsumerGroupsById(int userId);
        Task<int> RemoveConsumerFromGroup(int groupLinkId);
        Task<int> IsNotifcationGroupLinkIdExist(int groupLinkId);
        Task<ConsumerMasterDto> NewGetConsumerMaster(int estateId);
        Task<IEnumerable<PropertyCountDto>> ConsumerWisePropertyCount();
        Task<IEnumerable<MeterCountDto>> ConsumerWiseMeterCount();
        Task<IEnumerable<MeterNumberDto>> ConsumerWiseMeters();
        Task<DatatableModel<ConsumerDto>> GetConsumersNew(GetConsumersQuery request);
    }
        
}
