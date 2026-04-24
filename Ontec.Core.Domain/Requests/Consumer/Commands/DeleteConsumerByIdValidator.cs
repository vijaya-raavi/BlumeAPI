using FluentValidation;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Consumer.Commands
{
    public class DeleteConsumerByIdValidator : AbstractValidator<DeleteConsumerById>
    {
        public DeleteConsumerByIdValidator(IUserRepository _userRepository)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(DeleteConsumerById.Id), "User id does not exist.");
            });
        }
    }
}
