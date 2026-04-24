using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Company.Command;

namespace Ontec.Core.Domain.Requests.Configuration.Command
{
    public class AddOrUpdateConfigurationQueryValidator : AbstractValidator<AddOrUpdateConfigurationQuery>
    {
        public AddOrUpdateConfigurationQueryValidator(IWorkContext workContext,
                                                IUserRepository userRepository,
                                                ICompanyRepository companyRepository,
                                                IConfigurationRepository configRepository)
        {

            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await userRepository.IsUserIdExist(workContext.CurrentUserId).ConfigureAwait(false);
                var configurations = await configRepository.GetConfigurations().ConfigureAwait(false);
                double minTopup = 0;
                double maxTopup = 0;
                TimeSpan fromTime = TimeSpan.Zero;
                TimeSpan toTime = TimeSpan.Zero;
                //if (!isExist)
                //{
                //    context.AddFailure(nameof(AddUpdateCompanyQuery.Id), string.Format(CommonConstants.NotExist, nameof(AddUpdateCompanyQuery.Id)));
                //}

                if (!isExist)
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.NotExist, nameof(workContext.CurrentUserId)));
                if (workContext.CurrentRoleId != (int)RoleMasterEnum.Admin)
                {
                    context.AddFailure(nameof(workContext.CurrentUserId), string.Format(CommonConstants.Unauthorized, nameof(workContext.CurrentUserId)));
                }
                foreach (var item in model.Configurations)
                {
                    if (item != null)
                    {

                        if (item.Id == 0)
                        {
                            context.AddFailure(nameof(item.Id), string.Format(CommonConstants.NotExist, nameof(item.Id)));
                        }
                        else
                        {
                            var name = configurations.Where(a => a.Id.Equals(item.Id)).Select(a => a.Name).FirstOrDefault();
                            if (name != null && name.Equals("mintopupamount"))
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    minTopup = Convert.ToDouble(item.Value);
                                    maxTopup = Convert.ToDouble(item.Value);
                                    if (maxTopup > 0 && minTopup > maxTopup)
                                    {
                                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Minimum topup amount is above maximum topup amount");
                                    }
                                }
                                else
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Minimum topup amount is required.");
                                }
                            }
                            if (name != null && name.Equals("maxtopupamount"))
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    maxTopup = Convert.ToDouble(item.Value);
                                    if (maxTopup < minTopup)
                                    {
                                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Maximum topup amount is below minimum topup amount");
                                    }
                                }
                                else
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Maximum topup amount is required");
                                }
                            }

                            if (name != null && name.Equals("frombusinesshours"))
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {

                                     fromTime = Convert.ToDateTime(item.Value).TimeOfDay;
                                    if (toTime >TimeSpan.Zero && fromTime > toTime)
                                    {
                                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "From business hours should less than to business hours");
                                    }
                                }
                                else
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "From business hours are required.");
                                }
                            }
                            if (name != null && name.Equals("tobusinesshours"))
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                     toTime = Convert.ToDateTime(item.Value).TimeOfDay;
                                    if (fromTime != DateTime.Now.TimeOfDay && toTime != DateTime.Now.TimeOfDay && toTime <= fromTime)
                                    {
                                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "To business hours should greater than from business hours");
                                    }
                                }
                                else
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "To business hours are required.");
                                }
                            }
                            if (name != null && name.Equals("eftReferenceRandomDigitNumberLength"))
                            {
                                if (!string.IsNullOrEmpty(item.Value)&& item.Value.Contains("."))
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Decimal value not allowed.");
                                }
                                if (!item.Value.Contains(".") && !string.IsNullOrEmpty(item.Value))
                                {
                                   int value = Convert.ToInt32(item.Value);
                                    if (value == 0)
                                    {
                                        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "0 not allowed.");
                                    }
                                   
                                }
                                if (string.IsNullOrEmpty(item.Value))
                                {
                                    context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Random Digit Length (In EFT reference format) is required.");
                                }
                            }
                            //if (name != null && name.Equals("ContractDocumentSizeInMB"))
                            //{
                            //    if (!string.IsNullOrEmpty(item.Value) && item.Value.Contains("."))
                            //    {
                            //        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Decimal value not allowed.");
                            //    }
                            //    if (!item.Value.Contains(".") && !string.IsNullOrEmpty(item.Value))
                            //    {
                            //        int value = Convert.ToInt32(item.Value);
                            //        if (value == 0)
                            //        {
                            //            context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "0 not allowed.");
                            //        }

                            //    }
                            //    if (string.IsNullOrEmpty(item.Value))
                            //    {
                            //        context.AddFailure(nameof(AddOrUpdateConfigurationQuery.Configuration.Value), "Random Digit Length (In EFT reference format) is required.");
                            //    }
                            //}

                        }
                    }
                }



            });
        }
    }
}
