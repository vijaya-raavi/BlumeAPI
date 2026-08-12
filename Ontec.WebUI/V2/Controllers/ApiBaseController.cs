using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ontec.WebUI.V2.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    // Declare the versions supported by this base controller's derivatives
    [ApiVersion("2.0")]
    [ApiVersion("1.0")]
    public abstract class ApiBaseController:Controller
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}
