using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Ontec.WebUI.Infrastructure
{
    public interface IJwtAuthManager
    {
        IImmutableDictionary<string, RefreshToken> UsersRefreshReadonlyDictionary { get; }
        JwtAuthResult GenerateToken(string emailId, IEnumerable<Claim> claims, DateTime now);
        JwtAuthResult Refresh(string refreshToken, string accessToken, string emailId, DateTime now);
        void RemoveExiredRefreshToken(DateTime now);
        void RemoveRefreshTokenByEmailId(string emailId);
        (ClaimsPrincipal, JwtSecurityToken) DecodeJwtToken(string token);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }

    public class JwtAuthManager : IJwtAuthManager
    {
        private readonly ConcurrentDictionary<string, RefreshToken> _usersRefreshToken;
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly byte[] _secret;
        public IImmutableDictionary<string, RefreshToken> UsersRefreshReadonlyDictionary => _usersRefreshToken.ToImmutableDictionary();

        public JwtAuthManager(JwtTokenConfig jwtTokenConfig)
        {
            _jwtTokenConfig = jwtTokenConfig;
            _usersRefreshToken = new ConcurrentDictionary<string, RefreshToken>();
            _secret = Encoding.ASCII.GetBytes(jwtTokenConfig.Secret);
        }

        public (ClaimsPrincipal, JwtSecurityToken) DecodeJwtToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SecurityTokenException("Invalid token");
            }
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, new TokenValidationParameters
                {
                    ValidIssuer = _jwtTokenConfig.Issuer,
                    ValidAudience = _jwtTokenConfig.Audience,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_secret),
                    ValidateLifetime = false
                },
                out var validateToken);
            
            return (principal, validateToken as JwtSecurityToken);

        }

        public JwtAuthResult GenerateToken(string emailId, IEnumerable<Claim> claims, DateTime now)
        {
            var usersClaims = string.IsNullOrWhiteSpace(claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)?.Value);
            var jwtToken = new JwtSecurityToken(
                _jwtTokenConfig.Issuer,
                usersClaims ? _jwtTokenConfig.Audience : string.Empty,
                claims,
                expires: now.AddMinutes(_jwtTokenConfig.AccessTokenExpiration),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_secret), SecurityAlgorithms.HmacSha256Signature));
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var refreshToken = new RefreshToken
            {
                EmailId = emailId,
                TokenString = GenerateRefreshTokenString(),
                ExpireAt = DateTime.UtcNow.AddYears(10)// now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration)
            };
            _usersRefreshToken.AddOrUpdate(refreshToken.TokenString, refreshToken, (_, _) => refreshToken);
            return new JwtAuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
        private static string GenerateRefreshTokenString()
        {
            var reandomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(reandomNumber);
            return Convert.ToBase64String(reandomNumber);
        }

        public JwtAuthResult Refresh(string refreshToken, string accessToken, string emailId, DateTime now)
        {
            var (principal, jwtToken) = DecodeJwtToken(accessToken);
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature))
            {
                throw new SecurityTokenException("Invalid token");
            }

            if (!_usersRefreshToken.TryGetValue(refreshToken, out var existingRefreshToken))
            {
                throw new SecurityTokenException("Invalid token");
            }
            if (existingRefreshToken.EmailId != emailId || existingRefreshToken.ExpireAt < now)
            {
                throw new SecurityTokenException("Invalid token");
            }

            return GenerateToken(emailId, principal.Claims, now);// to recover original claims 
        }

        public void RemoveExiredRefreshToken(DateTime now)
        {
            var expiredTokens = _usersRefreshToken.Where(x => x.Value.ExpireAt < now).ToList();
            foreach (var expiredToken in expiredTokens)
            {
                _usersRefreshToken.TryRemove(expiredToken.Key, out _);
            }
        }

        public void RemoveRefreshTokenByEmailId(string emailId)
        {
            var refreshTokens = _usersRefreshToken.Where(x => x.Value.EmailId == emailId).ToList();
            foreach (var refreshToken in refreshTokens)
            {
                _usersRefreshToken.TryRemove(refreshToken.Key, out _);
            }
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_secret),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }
        public  string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
    public class RefreshToken
    {
        [JsonPropertyName("emailId")]
        public string EmailId { get; set; } //Can be use for tracking

        [JsonPropertyName("tokenString")]
        public string TokenString { get; set; }

        [JsonPropertyName("expireAt")]
        public DateTime ExpireAt { get; set; }
    }

    public class JwtAuthResult
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public RefreshToken RefreshToken { get; set; }
    }

}
