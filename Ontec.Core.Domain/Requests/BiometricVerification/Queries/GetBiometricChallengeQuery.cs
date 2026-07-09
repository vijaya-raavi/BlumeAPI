using MediatR;

namespace Ontec.Core.Domain.Requests.BiometricVerification.Queries
{
    public class GetBiometricChallengeQuery :IRequest<string>
    {
        public int UserId {  get; set; }
        public string DeviceId { get; set; }
    }
}
