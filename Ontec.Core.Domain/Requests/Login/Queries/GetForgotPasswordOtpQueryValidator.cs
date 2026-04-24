using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Domain.Requests.Login.Queries
{
    public class GetForgotPasswordOtpQueryValidator : AbstractValidator<GetForgotPasswordOtpQuery>
    {
        public GetForgotPasswordOtpQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(GetUserByEmailQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.EmailMobile).IsValidEmailMobile().LengthShouldBeLessOrEqualToAsync(nameof(GetUserByEmailQuery.Email).SplitPascalCase(), 100);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
           {
               int userStatus = await _userRepository.GetUserStatus(model.EmailMobile.ToLower(), model.CompanyId).ConfigureAwait(false);
               var user = await _userRepository.IsUserPending(model.EmailMobile, model.CompanyId).ConfigureAwait(false);
               if (userStatus.Equals((int)StatusEnum.Pending))
               {
                   context.AddFailure(nameof(ResetPassword.EmailMobile), "Your registration request is sent to admin for approval");
               }
               if (userStatus.Equals((int)StatusEnum.Inactive))
               {
                   context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), string.Format(CommonConstants.InActiveUserExist));
               }
               //if (userStatus.Equals((int)StatusEnum.Rejected))
               //{
               //    context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), string.Format(CommonConstants.InRejectedUserExist));
               //}
               else
               {
                   var getUserByEmailMobile = new GetUserByEmailComapnyId
                   {
                       CompanyId = model.CompanyId,
                       Email = model.EmailMobile
                   };

                   var data = await _userRepository.GetUserByEmailComapnyId(getUserByEmailMobile).ConfigureAwait(false);
                   if (data == null)
                   {
                       context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), " User does not exist in the system.");
                   }
                   else
                   {
                       if (!model.IsAdmin)
                       {
                           //// customer and temporary
                           //if (data.RoleName == RoleMasterEnum.Admin.ToString() || data.RoleName == RoleMasterEnum.Operator.ToString())
                           //{
                           //    context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), string.Format(CommonConstants.Unauthorized));
                           //}
                           
                       }
                       //else
                       //{
                       //    // Admin or operator 
                       //    if ((data.RoleName == RoleMasterEnum.Admin.ToString() || data.RoleName == RoleMasterEnum.Operator.ToString()))
                       //    {
                       //        context.AddFailure(nameof(GetForgotPasswordOtpQuery.EmailMobile), string.Format(CommonConstants.Unauthorized));
                       //    }

                       //}
                   }
               }
           });
        }
    }
}
