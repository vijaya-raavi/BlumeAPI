using MediatR;
using Ontec.Core.Domain.Models.Dto.Dashboard;

namespace Ontec.Core.Domain.Requests.Dashboard
{
    public class GetMeterAndUserRequestCount: IRequest<MeterAndUserRequestDto>
    {
    }
}
