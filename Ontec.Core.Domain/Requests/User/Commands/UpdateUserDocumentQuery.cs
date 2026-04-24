using MediatR;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserDocumentQuery : IRequest<AddUpdateResultDto>
    {
        public int UserId { get; set; }
        public int ProofDocumentTypeId { get; set; }

        public string? DocumentNumber { get; set; }
        public required IFormFile ProofDocument { get; set; }
    }
}
