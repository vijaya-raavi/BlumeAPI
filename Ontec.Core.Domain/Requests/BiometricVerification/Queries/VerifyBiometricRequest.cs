using MediatR;
using Ontec.Core.Domain.Models.Dto.Login;

namespace Ontec.Core.Domain.Requests.BiometricVerification.Queries
{
    public class VerifyBiometricRequest :IRequest<LoginResult>
    {
        public int UserId {  get; set; }
        public string DeviceId { get; set; }
        public string Challenge { get; set; }
        public string Signature { get; set; }
        public string Platform { get; set; }
    }
}
