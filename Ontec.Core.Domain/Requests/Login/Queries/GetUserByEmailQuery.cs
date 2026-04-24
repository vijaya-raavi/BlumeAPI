using System.ComponentModel;
using MediatR;
using Ontec.Core.Domain.Models.Dto.Login;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetUserByEmailQuery : IRequest<LoginResult>
    {

        public string? Email { get; set; }
        public string? Password { get; set; }
        [DisplayName("Company id")]
        public int CompanyId { get; set; }
        public bool IsAdmin { get; set; }=false;

        public string? Devicetoken { get; set; }
    }
}
