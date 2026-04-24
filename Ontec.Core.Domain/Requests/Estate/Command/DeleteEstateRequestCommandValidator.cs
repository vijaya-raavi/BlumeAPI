using FluentValidation;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Requests.Property.Command;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public class DeleteEstateRequestCommandValidator:AbstractValidator<DeleteEstateRequestCommand>
    {
        public DeleteEstateRequestCommandValidator(IEstateRepository estateInterfaceRepository)
        {
            RuleFor(m => m.Id).NotNull();

            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int count = await estateInterfaceRepository.IsEstateIdExist(model.Id).ConfigureAwait(false);
                if (count == 0)
                    context.AddFailure(nameof(DeleteEstateRequestCommand.Id), "Id does not exist");

            });
        }
    }
}
