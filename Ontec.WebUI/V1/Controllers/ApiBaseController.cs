using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ontec.WebUI.V1.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ApiBaseController:Controller
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}
