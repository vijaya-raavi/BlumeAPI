using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Content;
using Ontec.Core.Domain.Models.Dto.Content;
using Ontec.Core.Domain.Requests.Company.Command;
using Ontec.Core.Domain.Requests.Content;
using Ontec.Core.Domain.Requests.Content.Queries;

namespace Ontec.Core.Application.Content.Handler.Queries
{
    public class GetContentQueryHandler : IRequestHandler<GetContentQuery,ContentMaster>
    {
        private readonly IContentRepository _contentRepository;
        public GetContentQueryHandler(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }
        public async Task<ContentMaster> Handle(GetContentQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetContentQueryValidator(_contentRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _contentRepository.GetContent(request).ConfigureAwait(false);
           
        }

    }
}
