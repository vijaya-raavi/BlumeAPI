using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateStatusQueryValidator:AbstractValidator<UpdateStatusQuery>
    {
        public UpdateStatusQueryValidator(IWorkContext workContext
                                    ,IUserRepository userRepository
                                    ,IMeterRepository meterRepository
                                    ,IConfigurationRepository configurationRepository)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(m => m.StatusId).NotNull();
            RuleFor(m => m).CustomAsync(async (model, context, CancellationToken) =>
            {
                if (model.UpdateTo == (int)UpdateStatusEnum.User)
                { 
                     int usercount= await userRepository.IsUserExist(model.Id).ConfigureAwait(false);
                     if (usercount <= 0)
                        {
                            context.AddFailure(nameof(UpdateStatusQuery.Id),string.Format(CommonConstants.NotExist,nameof(UpdateStatusQuery.Id)));
                        }
                }
                if (model.UpdateTo == (int)UpdateStatusEnum.Meter)
                {
                    int metercount = await meterRepository.IsMeterExist(model.Id).ConfigureAwait(false);
                    if (metercount <= 0)
                    {
                        context.AddFailure(nameof(UpdateStatusQuery.Id), string.Format(CommonConstants.NotExist, nameof(UpdateStatusQuery.Id)));
                    }
                }
                if (model.UpdateTo == (int)UpdateStatusEnum.Reason)
                {
                    
                    int reasonCount = await configurationRepository.IsReasonIdExist(model.Id).ConfigureAwait(false);
                    if (reasonCount <= 0)
                    {
                        context.AddFailure(nameof(UpdateStatusQuery.Id), string.Format(CommonConstants.NotExist, nameof(UpdateStatusQuery.Id)));
                    }
                }
                if (model.UpdateTo == (int)UpdateStatusEnum.Operator)
                {

                   var user=await userRepository.GetUserById(model.Id).ConfigureAwait(false);
                    if (user.RoleId!=(int)RoleMasterEnum.Operator)
                    {
                        context.AddFailure(nameof(UpdateStatusQuery.Id), "Update the status of operator only");
                    }
                }
                //if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                //{
                //    context.AddFailure(nameof(workContext.CurrentRoleId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentRoleId)));
                //}
            });
        }
    }
}
