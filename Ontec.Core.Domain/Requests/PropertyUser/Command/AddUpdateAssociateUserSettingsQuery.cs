using FluentValidation;
using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class AddUpdateAssociateUserSettingsQuery : IRequest<AddUpdateResultDto>
    {
        public int Id { get; set; }
        public int PropertyUserId { get; set; }
        public int PropertyId { get; set; }
        public bool IsAllowTopUp {  get; set; }
    }
}
