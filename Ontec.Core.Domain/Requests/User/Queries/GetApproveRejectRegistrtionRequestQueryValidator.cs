using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Queries
{
    public class GetApproveRejectRegistrtionRequestQueryValidator : AbstractValidator<ApproveRejectRegistrtionRequestQuery>
    {
        public GetApproveRejectRegistrtionRequestQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.ID).NotNull().GreaterThanOrEqualTo(1);

            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isValid = await _userRepository.IsUserIdExist(model.ID);

                if (!isValid)
                {
                    context.AddFailure(nameof(ApproveRejectRegistrtionRequestQuery.ID), string.Format(CommonConstants.NotExist, nameof(ApproveRejectRegistrtionRequestQuery.ID)));
                }
               
                var isExist = await _userRepository.IsPendingUserIdExist(model.ID).ConfigureAwait(false);
                if (isValid && !isExist)
                {
                    context.AddFailure(nameof(ApproveRejectRegistrtionRequestQuery.ID), string.Format(CommonConstants.UserIsNotPending, nameof(ApproveRejectRegistrtionRequestQuery.ID)));
                }
                if(!model.IsApproved && string.IsNullOrEmpty(model.Comments))
                {
                    context.AddFailure(nameof(ApproveRejectRegistrtionRequestQuery.Comments), string.Format(CommonConstants.AreRequired, nameof(ApproveRejectRegistrtionRequestQuery.Comments)));
                }
            });

        }
    }
}
