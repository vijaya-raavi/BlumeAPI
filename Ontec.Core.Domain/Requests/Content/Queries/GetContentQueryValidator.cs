using FluentValidation;
using Ontec.Core.Domain.Interface.Content;

namespace Ontec.Core.Domain.Requests.Content.Queries
{
    public class GetContentQueryValidator : AbstractValidator<GetContentQuery>
    {
        public GetContentQueryValidator(IContentRepository contentRepository)
        {
            RuleFor(m => m.ContentName).NotNullAndEmptyAsync();
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await contentRepository.IsExist(model).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(GetContentQuery.ContentName), "content does not exist");
                }
            });
        }
    }
}
