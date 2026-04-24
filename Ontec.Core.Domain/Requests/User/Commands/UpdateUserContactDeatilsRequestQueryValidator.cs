using System.Text.RegularExpressions;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserContactDeatilsRequestQueryValidator : AbstractValidator<UpdateUserContactDeatilsRequestQuery>
    {
        public UpdateUserContactDeatilsRequestQueryValidator(IWorkContext _workContext, IUserRepository _userRepository)
        {
            RuleFor(m => m.UserId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(x => x.Otp).NotNullAndEmptyAsyncOTP().IsValidNumber().LengthShouldBeEqualAsync(nameof(UpdateUserContactDeatilsRequestQuery.Otp).ToUpper(), 6);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.UserId), string.Format(CommonConstants.NotExist, nameof(UpdateUserContactDeatilsRequestQuery.UserId).SplitPascalCase()));
                else
                {
                    if (_workContext.CurrentRoleId != (int)RoleMasterEnum.Admin && _workContext.CurrentRoleId != (int)RoleMasterEnum.Operator)
                    {
                        context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.UserId), "You are not authorize.");
                    }
                    if (model.IsEmail)
                    {
                        if (string.IsNullOrEmpty(model.EmailId))
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.EmailId), string.Format(CommonConstants.IsRequired, nameof(UpdateUserContactDeatilsRequestQuery.EmailId).SplitPascalCase()));
                        }
                        if (!string.IsNullOrEmpty(model.EmailId) && !Regex.IsMatch(model.EmailId, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z._]+\.[a-zA-Z.]{2,}$"))
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.EmailId), string.Format(CommonConstants.InValid, nameof(UpdateUserContactDeatilsRequestQuery.EmailId).SplitPascalCase()));
                        }
                        int emailUser = await _userRepository.IsEmailExist(model.EmailId, _workContext.CurrentCompanyId, false).ConfigureAwait(false);
                        if (emailUser > 0)
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.EmailId), string.Format(CommonConstants.AlreadyExist, nameof(UpdateUserContactDeatilsRequestQuery.EmailId).SplitPascalCase()));
                        }
                    }
                    if (!model.IsEmail)
                    {
                        if (string.IsNullOrEmpty(model.MobileNumber))
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber), string.Format(CommonConstants.IsRequired, nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber).SplitPascalCase()));
                        }
                        if (!string.IsNullOrEmpty(model.MobileNumber) && !Regex.IsMatch(model.MobileNumber, "^[0-9A-Z]+$"))
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber), string.Format(CommonConstants.InValid, nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber)));
                        }
                        int mobileUser = await _userRepository.IsEmailExist(model.MobileNumber, _workContext.CurrentCompanyId, false).ConfigureAwait(false);
                        if (mobileUser > 0)
                        {
                            context.AddFailure(nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber), string.Format(CommonConstants.AlreadyExist, nameof(UpdateUserContactDeatilsRequestQuery.MobileNumber).SplitPascalCase()));
                        }
                    }
                }

            });

        }
    }
}
