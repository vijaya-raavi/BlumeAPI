using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Models.Dto.TopUp;

namespace Ontec.Core.Domain.Requests.Notification.Command
{
    public class AddMasterNotificationValidator : AbstractValidator<AddMasterNotifications>
    {
        public AddMasterNotificationValidator(IMeterRepository meterRepository) 
        {
            
            RuleFor(m => m.MeterNumber).NotEmpty();
            RuleFor(x => x).CustomAsync(async (model, context, CancellationToken) =>
            {
                int id = await meterRepository.IsMeterNumberExist(model.MeterNumber).ConfigureAwait(false);
                if (id == 0)
                {
                    context.AddFailure(nameof(AddMasterNotifications.MeterNumber), string.Format(CommonConstants.NotExist, nameof(AddMasterNotifications.MeterNumber)));
                }

            });
        }
    }
}
