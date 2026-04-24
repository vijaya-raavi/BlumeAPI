using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class DeletePropertyUserByIdValidator : AbstractValidator<DeletePropertyUserById>
    {
        public DeletePropertyUserByIdValidator(IPropertyUserRepository _propertyUserRepository, IPropertyRepository _propertyRepository
                                                , IWorkContext _workContext)
        {
            RuleFor(m => m.Id).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyUserRepository.IsPropertyUserIdExist(model.Id).ConfigureAwait(false);
                if (!isValid)
                    context.AddFailure(nameof(DeletePropertyUserById.Id), string.Format(CommonConstants.NotExist, nameof(DeletePropertyUserById.Id)));
                else
                {
                    var propertyOwner = await _propertyUserRepository.GetPropertyOwnerIdByPropertyUserId(model.Id).ConfigureAwait(false);
                    if (propertyOwner != null)
                    {
                        var user = await _propertyUserRepository.GetPropertyUserById(model.Id).ConfigureAwait(false);

                        if (user != null && user.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
                        {
                            var associateCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(propertyOwner.PropertyId, (int)PropertyUserRelationEnum.Associate).ConfigureAwait(false);
                            var tenantsCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(propertyOwner.PropertyId, (int)PropertyUserRelationEnum.Tenant).ConfigureAwait(false);

                            //if (tenantsCount > 0 && propertyOwner.OwnerId == _workContext.CurrentUserId)
                            //{
                            //    context.AddFailure(nameof(DeletePropertyUserById.Id), "You are not authorize to remove tenant associate users");
                            //}
                            //if (propertyOwner.OwnerId != _workContext.CurrentUserId)
                            //{
                            //    //Loged in user must be tenant 
                            //    var isValidTenant = await _propertyUserRepository.IsTenantValid(propertyOwner.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                            //    if (!isValidTenant)
                            //    {
                            //        context.AddFailure(nameof(DeletePropertyUserById.Id), "You are not authorize to delete associates");
                            //    }
                            //}
                        }
                        else
                        {
                            //if (propertyOwner.OwnerId != _workContext.CurrentUserId)
                            //{
                            //    context.AddFailure(nameof(DeletePropertyUserById.Id), "Only property owner can delete tenent.");
                            //}
                        }
                    }
                }
            });
        }
    }
}
