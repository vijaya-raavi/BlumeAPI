using System.Text.RegularExpressions;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.MasterApiService;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Meter.Command;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Domain.Requests.Dashboard.Command
{
    public class CaptureUsersCreditsToSaveCommandReuqestValidator : AbstractValidator<CaptureUsersCreditsToSaveCommandReuqest>
    {
        public CaptureUsersCreditsToSaveCommandReuqestValidator(IUserRepository _userRepository,IMeterRepository _meterRepository)
        {


            RuleFor(x => x.MeterId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(CaptureUsersCreditsToSaveCommandReuqest.MeterId).SplitPascalCase(), 1);
            RuleFor(x => x.UserId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(CaptureUsersCreditsToSaveCommandReuqest.UserId).SplitPascalCase(), 1);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("Amount must be >= 0.").Must(x => !double.IsNaN(x) && !double.IsInfinity(x)).WithMessage("Amount must be a valid finite number.");
            RuleFor(x => x.Image).IsImageValid(nameof(CaptureUsersCreditsToSaveCommandReuqest.Image).SplitPascalCase(), 5);// 1MB 
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(CaptureUsersCreditsToSaveCommandReuqest.UserId), "User id does not exist");

                var isMeterExist = await _meterRepository.IsMeterIdExist(model.MeterId).ConfigureAwait(false);
                if (!isMeterExist)
                    context.AddFailure(nameof(CaptureUsersCreditsToSaveCommandReuqest.MeterId), string.Format(CommonConstants.NotExist, nameof(CaptureUsersCreditsToSaveCommandReuqest.MeterId)));

                if (model.Image == null)
                {
                    context.AddFailure(nameof(CaptureUsersCreditsToSaveCommandReuqest.Image), string.Format(CommonConstants.IsRequired, nameof(CaptureUsersCreditsToSaveCommandReuqest.Image).SplitPascalCase()));
                }
            });


        }
    }
}
