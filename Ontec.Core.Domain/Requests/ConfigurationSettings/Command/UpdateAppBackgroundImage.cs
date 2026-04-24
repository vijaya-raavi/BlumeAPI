using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateAppBackgroundImage : IRequest<string>
    {
        public int Id { get; set; }
        public required IFormFile AppBackgroundImage { get; set; }
    }
}
