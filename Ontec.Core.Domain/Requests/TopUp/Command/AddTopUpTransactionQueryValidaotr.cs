using FluentValidation;
using MediatR;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Meter.Command;

namespace Ontec.Core.Domain.Requests.TopUp.Command
{
    public class AddTopUpTransactionQueryValidaotr : AbstractValidator<AddTopUpTransactionQuery>
    {
        public AddTopUpTransactionQueryValidaotr(IUserRepository _userRepository
                                                , IWorkContext _workContext
                                                , IMeterRepository _meterRepository
                                                , IConfigurationRepository _configurationRepository
                                                , ITopUpRepository topUpRepository)
        {
            RuleFor(x => x.UserId).NotNull().GreaterThanOrEqualToAsync(nameof(AddTopUpTransactionQuery.UserId).SplitPascalCase(), 1);
            RuleFor(x => x.MeterId).NotNull().GreaterThanOrEqualToAsync(nameof(AddTopUpTransactionQuery.MeterId).SplitPascalCase(), 1);
            RuleFor(x => x.PaymentMethodId).NotNull().GreaterThanOrEqualToAsync(nameof(AddTopUpTransactionQuery.PaymentMethodId).SplitPascalCase(), 1);
            RuleFor(x => x.Amount).NotEmpty().IsValidTopUpAmount();
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                var payMethods=await topUpRepository.GetAllPaymentMethods().ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(AddTopUpTransactionQuery.UserId), string.Format(CommonConstants.NotExist, nameof(AddTopUpTransactionQuery.UserId)));
                }
                var isPropertyExist = await _meterRepository.IsMeterIdExist(model.MeterId).ConfigureAwait(false);
                if (!isPropertyExist)
                {
                    context.AddFailure(nameof(AddTopUpTransactionQuery.MeterId), string.Format(CommonConstants.NotExist, nameof(AddTopUpTransactionQuery.MeterId)));
                }
                if (_workContext.CurrentUserId != model.UserId)
                {
                    context.AddFailure(nameof(AddTopUpTransactionQuery.UserId), string.Format(CommonConstants.Unauthorized, nameof(AddTopUpTransactionQuery.UserId)));
                }

                if (model.PaymentMethodId > 0)
                {
                    bool exists = payMethods.Any(x => x.Id == model.PaymentMethodId);
                    var paymentMethod = payMethods.FirstOrDefault(x => x.Id == model.PaymentMethodId);
                   
                    if (!exists)
                    {
                        context.AddFailure(nameof(AddTopUpTransactionQuery.PaymentMethodId), string.Format(CommonConstants.NotExist, nameof(AddTopUpTransactionQuery.PaymentMethodId)));
                    }
                    if (paymentMethod != null && paymentMethod.Name.Equals("bank-transfer", StringComparison.OrdinalIgnoreCase))
                    {
                        context.AddFailure( nameof(AddTopUpTransactionQuery.PaymentMethodId),string.Format(CommonConstants.PaymentIdNotAllowed, nameof(AddTopUpTransactionQuery.PaymentMethodId)));
                    }

                }
                double topup = model.Amount;
                var configurations = await _configurationRepository.GetConfigurations().ConfigureAwait(false);
                if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("mintopupamount")) && configurations.Any(t => t.Name.ToLower().Equals("maxtopupamount")))
                {
                    var minAmount = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("mintopupamount"));
                    var maxAmount = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("maxtopupamount"));
                    if (minAmount != null && !string.IsNullOrEmpty(minAmount.Value) && maxAmount != null && !string.IsNullOrEmpty(maxAmount.Value))
                    {
                        if (topup < Convert.ToDouble(minAmount.Value))
                        {
                            context.AddFailure(nameof(AddTopUpTransactionQuery.Amount), string.Format(CommonConstants.TopUpMinLimit, nameof(AddTopUpTransactionQuery.Amount)));
                        }

                        if (topup > Convert.ToDouble(maxAmount.Value))
                        {
                            context.AddFailure(nameof(AddTopUpTransactionQuery.Amount), string.Format(CommonConstants.TopUpMaxLimit, nameof(AddTopUpTransactionQuery.Amount)));
                        }
                    }
                }

            });
        }
    }
}
