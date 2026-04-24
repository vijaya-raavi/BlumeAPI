using FluentValidation;
using FluentValidation.Validators;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Company.Command;
using Ontec.Core.Domain.Requests.Configuration.Command;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateBusinessHoursConfigurationsValidator : AbstractValidator<UpdateBusinessHoursConfigurations>
    {
        public UpdateBusinessHoursConfigurationsValidator(IWorkContext workContext,
                                                IUserRepository userRepository,
                                                ICompanyRepository companyRepository,
                                                IConfigurationRepository configRepository)
        {

            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);
                var configurations = await configRepository.GetBusinessHoursConfigurations().ConfigureAwait(false);

                
                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId)));
                }
                foreach (var item in model.businessConfigurations)
                {
                    if (item != null)
                    {
                        TimeSpan fromTime = TimeSpan.Zero;
                        TimeSpan toTime = TimeSpan.Zero;

                        if (item.Id == 0)
                        {
                            context.AddFailure(nameof(item.Id), string.Format(CommonConstants.NotExist, nameof(item.Id)));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(item.From))
                            {
                                fromTime = Convert.ToDateTime(item.From).TimeOfDay;
                                if (toTime > TimeSpan.Zero && fromTime > toTime)
                                {
                                    context.AddFailure(nameof(item.From), "From business hours should less than to business hours");
                                }
                            }
                          
                            if (!string.IsNullOrEmpty(item.To))
                            {
                                toTime = Convert.ToDateTime(item.To).TimeOfDay;
                                if (fromTime !=TimeSpan.Zero && toTime != DateTime.Now.TimeOfDay && toTime <= fromTime)
                                {
                                    context.AddFailure(nameof(item.To), "To business hours should greater than from business hours");
                                }
                            }
                            
                        }
                    }
                }

            });
        }
    }
}
