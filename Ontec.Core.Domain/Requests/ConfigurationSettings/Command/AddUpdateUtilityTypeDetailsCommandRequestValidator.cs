using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Configuration.Command;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class AddUpdateUtilityTypeDetailsCommandRequestValidator:AbstractValidator<AddUpdateUtilityTypeDetailsCommandRequest>
    {
        public AddUpdateUtilityTypeDetailsCommandRequestValidator(IMeterRepository meterRepository,IWorkContext workContext,IUserRepository userRepository)
        {
           
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);


                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId)));
                }
                foreach (var item in model.UtilityDailyTarget)
                {
                    var meterType = await meterRepository.GetMeterType().ConfigureAwait(false);
                    bool isTypeExist = meterType.ToList().Any(x => x.id == item.Id);
                    var name = meterType.ToList().Where(s => s.id == item.Id).Select(s => s.name).FirstOrDefault();
                    if (!isTypeExist)
                    {
                        context.AddFailure(nameof(item.Id), string.Format(CommonConstants.NotExist, nameof(item.Id)));
                    }
                    if (item.Name.Trim().ToLower() != name.Trim().ToLower())
                    {
                        context.AddFailure(nameof(item.Name), "Utiltiy type mismatch");
                    }
                    if (!Regex.IsMatch(item.MinDailyTarget.ToString(), @"^\d+(\.\d+)?$"))
                    {
                        context.AddFailure(nameof(item.MinDailyTarget), "Invalid minimum daily target");
                    }
                    if (!Regex.IsMatch(item.MaxDailyTarget.ToString(), @"^\d+(\.\d+)?$"))
                    {
                        context.AddFailure(nameof(item.MaxDailyTarget), "Invalid maximum daily target");
                    }
                    if (item.MaxDailyTarget > 0 && item.MinDailyTarget > item.MaxDailyTarget)
                    {
                        context.AddFailure(nameof(item.MinDailyTarget), "Minimum daily target is above maximum daily target");
                    }
                    if (item.MaxDailyTarget < item.MinDailyTarget)
                    {
                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Maximum daily target is below minimum daily target");
                    }
                }
            });
        }
    }
}
