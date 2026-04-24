using FluentValidation;
using Ontec.Core.Domain.Interface.Estate;

namespace Ontec.Core.Domain.Requests.Estate.Command
{
    public class AddEstateRequestCommandValidator : AbstractValidator<AddEstateRequestCommand>
    {
        public AddEstateRequestCommandValidator(IEstateRepository estateInterfaceRepository)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(m => m.Estate).NotNullAndEmptyAsyncForProperty().LengthShouldBeLessOrEqualToAsync("Estate name should be less than or equal to ", 55);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {

                if (model.Id > 0)
                {
                    int count = await estateInterfaceRepository.IsEstateIdExist(model.Id).ConfigureAwait(false);
                    if (count == 0)
                        context.AddFailure(nameof(AddEstateRequestCommand.Id), "Id does not exist");


                }
                else if (model.Id == 0)
                {
                    int estateCount = await estateInterfaceRepository.IsActiveEstateExist(model.Estate).ConfigureAwait(false);
                    if (estateCount > 0)
                        context.AddFailure(nameof(AddEstateRequestCommand.Estate), "Estate already exists");
                }
            });
        }
    }
}
