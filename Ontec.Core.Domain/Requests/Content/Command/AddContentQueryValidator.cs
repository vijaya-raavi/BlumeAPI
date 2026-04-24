using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Content;

namespace Ontec.Core.Domain.Requests.Content.Command
{
    public  class AddContentQueryValidator:AbstractValidator<AddContentQuery>
    {
        public AddContentQueryValidator(IContentRepository contentRepository)
        {
            RuleFor(m => m.Content).NotNullAndEmptyAsync();
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
                {
                    var isExist = await contentRepository.IsIdExist(model).ConfigureAwait(false);
                    if (!isExist)
                    {
                        context.AddFailure(nameof(AddContentQuery.Id), string.Format(CommonConstants.NotExist,nameof(AddContentQuery.Id)));
                    }
                });
        }
    }
}
