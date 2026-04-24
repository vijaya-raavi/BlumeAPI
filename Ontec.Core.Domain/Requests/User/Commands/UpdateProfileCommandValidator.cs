using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator(IUserRepository _userRepository
                                            , IWorkContext _workContext)
        {
            RuleFor(x => x.ProfilePicture).IsImageValid(nameof(UpdateProfileCommand.ProfilePicture).SplitPascalCase(), 1);// 1MB 
            RuleFor(x => x.Id).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(UpdateProfileCommand.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isxist = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                
                if (!isxist)
                {
                    context.AddFailure(nameof(UpdateProfileCommand.Id), string.Format(CommonConstants.NotExist, nameof(UpdateProfileCommand.Id)));
                }
                if (model.Id != _workContext.CurrentUserId)
                {
                    context.AddFailure(nameof(UpdateProfileCommand.Id), string.Format(CommonConstants.InValid, nameof(UpdateProfileCommand.Id)));
                }
                if (model.ProfilePicture == null)
                {
                    context.AddFailure(nameof(UpdateProfileCommand.ProfilePicture), string.Format(CommonConstants.IsRequired, nameof(UpdateProfileCommand.ProfilePicture).SplitPascalCase()));
                }
            });
        }
    }
}
