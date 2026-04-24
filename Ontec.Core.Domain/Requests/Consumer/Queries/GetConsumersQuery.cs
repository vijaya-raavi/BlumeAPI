using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models;
using MediatR;

namespace Ontec.Core.Domain.Requests.Consumer.Queries
{
    public class GetConsumersQuery : BaseDatatableQuery<int?>, IRequest<DatatableModel<ConsumerDto>>
    {
        public int EstateId { get; set; }
        public int RoleId { get; set; }
        public string? SearchText { get; set; }
        public int MeterTypeId { get; set; }
        public int StatusId { get; set; }
        public bool IsVerified {  get; set; }
    }
}