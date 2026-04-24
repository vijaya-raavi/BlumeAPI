using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.Login.Command
{
    public class LogOutRequestQueryValidator : AbstractValidator<LogOutRequestQuery>
    {
        public LogOutRequestQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.UserId).NotNullAndEmptyAsync();
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int count =await _userRepository.IsUserExist(model.UserId).ConfigureAwait(false);
                if(count == 0)
                {
                    context.AddFailure(nameof(LogOutRequestQuery.UserId), string.Format(CommonConstants.InActiveUserExist));
                }
            });
        }

    }
}
