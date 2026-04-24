using MediatR;
using Ontec.Core.Domain.Models.Dto.ManagePermissions;

namespace Ontec.Core.Domain.Requests.ManagePermissions.Queries
{
    public  class GetPermissionsQuery :IRequest<IEnumerable<SettingTypeDto>>
    {
        public int UserId {  get; set; }

    }
}
