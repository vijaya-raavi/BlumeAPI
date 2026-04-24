using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Ontec.Core.Cache;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Requests.User.Queries;
using Ontec.WebUI.Helper;
using Ontec.WebUI.Models;

namespace Ontec.WebUI.Filters
{
    public class ReuestHeaderFilterAttribute : ActionFilterAttribute
    {
        private readonly IWorkContext _workContext;
        private readonly ICacheService _cacheService;
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;
        public ReuestHeaderFilterAttribute(IWorkContext workContext, ICacheService cacheService, IMediator mediator, IUserRepository userRepository)
        {
            _workContext = workContext;
            _cacheService = cacheService;
            _mediator = mediator;
            _userRepository = userRepository;
        }
        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                if (context.HttpContext.User.Claims.Any())
                {
                    var firstClaim = context?.HttpContext?.User?.Claims.First()?.Value;
                    if (RegexUtilities.IsValidMobileEmail(firstClaim))
                    {
                        var claims = context?.HttpContext?.User?.Claims.ToList();
                        var company = claims.FirstOrDefault(t => t.Type == "company").Value;
                        var companyId = int.Parse(company);
                        SetContextAsync(firstClaim, companyId, context).GetAwaiter().GetResult();
                    }
                    else
                    {
                        SetUnauthorized(context);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                context.Result = new BadRequestResult();
                return;
            }
        }
        private async Task SetContextAsync(string email, int companyId, ActionExecutingContext context)
        {
            UserDto user;
            if (_cacheService.TryGetValue(email, out UserDto userDto))
            {
                user = userDto;
                _workContext.SetCurrentUserId(user.Id);
                _workContext.SetCurrentRoleId(user.RoleId);
                _workContext.SetStatusId(user.RoleId);
                _workContext.SetCurrentCompanyId(user.CompanyId);
            }
            else
            {
                var req = new GetUserByEmailComapnyId { Email = email, CompanyId = companyId };
                //user = await _userRepository.GetUserByEmailComapnyId(req);
                user = await _mediator.Send(new GetUserByEmailComapnyId { Email = email, CompanyId = companyId }).ConfigureAwait(false);
                if (user == null)
                {
                    SetUnauthorized(context);
                  
                }
                else
                {
                    _cacheService.Set(email, user);
                    _workContext.SetCurrentUserId(user.Id);
                    _workContext.SetCurrentRoleId(user.RoleId);
                    _workContext.SetStatusId(user.RoleId);
                    _workContext.SetCurrentCompanyId(user.CompanyId);
                    return;
                }
            }
           
        }

        private static void SetUnauthorized(ActionExecutingContext context)
        {
            context.Result = new BadRequestObjectResult(new ApiResult
            {
                Result = "UnAuthorized Activity",
                Error = true
            });
        }
    }
}
