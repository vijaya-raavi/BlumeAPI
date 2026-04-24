using MediatR;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public  class LogOutRequestQuery:IRequest<string>
    {
        public int UserId {  get; set; }
        public string SessionKey {  get; set; }
    }
}
