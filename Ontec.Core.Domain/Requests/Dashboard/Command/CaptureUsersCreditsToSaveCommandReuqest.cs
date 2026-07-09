using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.Dashboard.Command
{
    public class CaptureUsersCreditsToSaveCommandReuqest : IRequest<AddUpdateResultDto>
    {
        public int UserId { get; set; }
        public int MeterId {  get; set; }
        public double Amount {  get; set; }      
        public IFormFile Image { get; set; }
    }
}
