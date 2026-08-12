using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.BiometricVerification.Command
{
    public class RegisterBiometricRequest : IRequest<AddUpdateResultDto>
    {
        public int UserId {  get; set; }
        public string DeviceId { get; set; }
        public string PublicKey { get; set; }
    }
}
