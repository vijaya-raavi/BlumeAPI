using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateProfileCommand : IRequest<string>
    {
        public int Id { get; set; }
        public required IFormFile ProfilePicture { get; set; }
    }
}
