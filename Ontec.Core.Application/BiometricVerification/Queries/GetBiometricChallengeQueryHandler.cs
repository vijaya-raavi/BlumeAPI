using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Ontec.Core.Domain.Requests.BiometricVerification.Queries;

namespace Ontec.Core.Application.BiometricVerification.Queries
{
    public class GetBiometricChallengeQueryHandler : IRequestHandler<GetBiometricChallengeQuery, string>
    {
        private readonly IMemoryCache _memoryCache;

        public GetBiometricChallengeQueryHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<string> Handle(GetBiometricChallengeQuery request, CancellationToken cancellationToken)
        {
            var challenge = Guid.NewGuid().ToString();

            var cacheKey = $"BIO_{request.DeviceId}_{request.UserId}";


            Console.WriteLine($"SET KEY: {cacheKey}");

            _memoryCache.Set(cacheKey, challenge, TimeSpan.FromMinutes(5));

            return await Task.FromResult(challenge);
        }

        //public async Task<string> Handle(GetBiometricChallengeQuery request,
        //                          CancellationToken cancellationToken)
        //{
        //    var challenge = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        //    var cacheKey = BuildCacheKey(request.DeviceId, request.UserId);

        //    Console.WriteLine($"[Biometric] SET cache key : {cacheKey}");
        //    Console.WriteLine($"[Biometric] Challenge      : {challenge}");

        //    _memoryCache.Set(cacheKey, challenge, TimeSpan.FromMinutes(5));

        //    return await Task.FromResult(challenge);
        //}
        private string BuildCacheKey(string deviceId, int userId)
        {
            return $"BIO_{deviceId.Trim().ToLower()}_{userId}";
        }
    }
}