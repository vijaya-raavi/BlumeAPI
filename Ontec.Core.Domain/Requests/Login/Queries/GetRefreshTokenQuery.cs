using MediatR;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetRefreshTokenQuery: IRequest<GetRefreshTokenQuery>
    {
        public string accessToken {  get; set; }
        public string refreshToken { get; set; }
    }
}
