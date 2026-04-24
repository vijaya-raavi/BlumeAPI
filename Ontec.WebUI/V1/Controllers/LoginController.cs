using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Models.Dto.Otp;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.Otp.Command;
using Ontec.WebUI.Infrastructure;
using System.Security.Claims;

namespace Ontec.WebUI.V1.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LoginController : ApiBaseController
    {
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly IUserRepository _userRepository;
        private readonly ILiveUserService _liveUserService;
        private readonly IUserNotificationConnection _userNotificationConnection;

        public LoginController(IJwtAuthManager jwtAuthManager, IUserRepository userRepository,ILiveUserService liveUserService, IUserNotificationConnection userNotificationConnection)
        {
            _jwtAuthManager = jwtAuthManager;
            _userRepository = userRepository;
            _liveUserService = liveUserService;
            _userNotificationConnection = userNotificationConnection;
        }
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUser([FromBody] GetUserByEmailQuery request)
        {
                    var loginResult = new LoginResult();
                    loginResult = await Mediator.Send(request).ConfigureAwait(false);

                if (loginResult != null && !string.IsNullOrEmpty(loginResult.EmailId))
                {
                    var claims = new List<Claim>
                {
                    new("email", request.Email),
                    new("company", request.CompanyId.ToString()),
                };

                    var jwtResut = _jwtAuthManager.GenerateToken(request.Email, claims, DateTime.Now);
                    loginResult.AccessToken = jwtResut.AccessToken;
                    loginResult.RefreshToken = jwtResut.RefreshToken.TokenString;
                    loginResult.Message = "User logged in successfully!";
                    var sessionKey = "";

                    sessionKey = await _userRepository.GenerateSessionKey(loginResult.AccessToken.Substring(0, 5)).ConfigureAwait(false);
                    await _userRepository.SaveUserLogInDeatils(loginResult.UserId, sessionKey).ConfigureAwait(false);
                    loginResult.sessionKey = sessionKey;
                    return Ok(loginResult);
                }

            
            return BadRequest(loginResult);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand request)
        {

            var loginResult = await Mediator.Send(request).ConfigureAwait(false);
            if (loginResult.UserId > 0)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("email", request.EmailId));
                claims.Add(new Claim("company", request.CompanyId.ToString()));
                var jwtResut = _jwtAuthManager.GenerateToken(request.EmailId, claims, DateTime.Now);
                loginResult.AccessToken = jwtResut.AccessToken;
                loginResult.RefreshToken = jwtResut.RefreshToken.TokenString;
                var sessionKey = "";

                sessionKey = await _userRepository.GenerateSessionKey(loginResult.AccessToken.Substring(0, 5)).ConfigureAwait(false);
                await _userRepository.SaveUserLogInDeatils(loginResult.UserId, sessionKey).ConfigureAwait(false);
                loginResult.sessionKey = sessionKey;
                return Ok(loginResult);
            }
            else
            {
                return BadRequest(loginResult.Message);
            }
        }

        [HttpPost("get_register_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResgisterOtp([FromBody] GetRegisterOtpQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [HttpPost("reset_password")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        #region ResetPassword
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        #endregion

        [HttpPost("verify_forgotpass_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyOTPDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpQuery request)
        {
          return Ok(await Mediator.Send(request).ConfigureAwait(false));
           
        }

        [HttpPost("get_forgotpass_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OtpResponseModel))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetForgotPasswordOtp([FromBody] GetForgotPasswordOtpQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }


        [HttpPost("refersh-token")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRefreshTokenQuery))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] GetRefreshTokenQuery request)
        {
            var principal = _jwtAuthManager.GetPrincipalFromExpiredToken(request.accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            else
            {
                var claims = principal.Claims.ToList();
                var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

                string emailid = "";
                int companyid = 0;
                if (email != null)
                {
                    emailid = email.Value;
                    var company = principal.Claims.FirstOrDefault(t => t.Type == "company").Value;
                    companyid = int.Parse(company);
                }

                int isExist = await _userRepository.IsEmailExist(emailid, companyid);
                if (isExist == 0)
                {
                    return BadRequest("User email is not registered in system");
                }
                else
                {
                    var newAccessToken = _jwtAuthManager.GenerateToken(emailid, claims, DateTime.Now);
                    var results = new GetRefreshTokenQuery()
                    {
                        accessToken = newAccessToken.AccessToken,
                        refreshToken = newAccessToken.RefreshToken.TokenString
                    };
                    return Ok(results);
                }
            }
        }
        
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogOut([FromBody] LogOutRequestQuery request)
        {
            var connectionId = await _userNotificationConnection.GetUserConnectionId(request.UserId).ConfigureAwait(false);
            if(!string.IsNullOrEmpty(connectionId))
                _liveUserService.RemoveUser(connectionId);
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

    }
}
