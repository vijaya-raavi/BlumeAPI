using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Content;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Content.Command;

namespace Ontec.Core.Application.Content.Handler.Command
{
    public class ContentCommandHandler : IRequestHandler<AddContentQuery, AddUpdateResultDto>
    {
        private readonly IContentRepository _contentRepository;
        public ContentCommandHandler(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }
        public async Task<AddUpdateResultDto> Handle(AddContentQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddContentQueryValidator(_contentRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();

            if (request.Id > 0)
            {
                response.Id = await _contentRepository.UpdateContnet(request).ConfigureAwait(false);
                response.Message = "Content updated successfully!";
            }
            return response;
        }
    }
}
