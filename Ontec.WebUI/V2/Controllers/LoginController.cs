using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Models.Dto.Otp;
using Ontec.Core.Domain.Requests.BiometricVerification.Command;
using Ontec.Core.Domain.Requests.BiometricVerification.Queries;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.Login.Queries;
using Ontec.Core.Domain.Requests.Otp.Command;
using Ontec.WebUI.Infrastructure;

namespace Ontec.WebUI.V2.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LoginController : ApiBaseController
    {
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly IUserRepository _userRepository;


        public LoginController(IJwtAuthManager jwtAuthManager, IUserRepository userRepository)
        {
            _jwtAuthManager = jwtAuthManager;
            _userRepository = userRepository;

        }
        [MapToApiVersion("2.0")]
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
        [MapToApiVersion("2.0")]
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
        [MapToApiVersion("2.0")]
        [HttpPost("get_register_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResgisterOtp([FromBody] GetRegisterOtpQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
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
        [MapToApiVersion("2.0")]
        [HttpPost("verify_forgotpass_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyOTPDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpQuery request)
        {
          return Ok(await Mediator.Send(request).ConfigureAwait(false));
           
        }
        [MapToApiVersion("2.0")]
        [HttpPost("get_forgotpass_otp")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OtpResponseModel))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetForgotPasswordOtp([FromBody] GetForgotPasswordOtpQuery request)
        {
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
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
        [MapToApiVersion("2.0")]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogOut([FromBody] LogOutRequestQuery request)
        {

            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [Authorize]
        [HttpPost("register-biometric")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddUpdateResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterBiometric([FromBody] RegisterBiometricRequest command)
        {
            //command.UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await Mediator.Send(command).ConfigureAwait(false));
        }
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [HttpGet("get-biometric-challenge/{deviceId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChallenge([FromRoute] string deviceId, int userId)
        {
            var request = new GetBiometricChallengeQuery()
            {
                DeviceId = deviceId,
                UserId = userId
            };
            return Ok(await Mediator.Send(request).ConfigureAwait(false));
        }

        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("verify-biometric-login")]
        public async Task<IActionResult> VerifyBiometric([FromBody] VerifyBiometricRequest command)
        {

            var loginResult = new LoginResult();
            loginResult = await Mediator.Send(command).ConfigureAwait(false);

            if (loginResult != null && !string.IsNullOrEmpty(loginResult.EmailId))
            {
                var claims = new List<Claim>
                {
                    new("email", loginResult.EmailId),
                    new("company", loginResult.CompanyId.ToString()),
                };

                var jwtResut = _jwtAuthManager.GenerateToken(loginResult.EmailId, claims, DateTime.Now);
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
    }
}
