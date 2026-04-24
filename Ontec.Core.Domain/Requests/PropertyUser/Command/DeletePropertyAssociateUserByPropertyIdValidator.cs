using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class DeletePropertyAssociateUserByPropertyIdValidator : AbstractValidator<DeletePropertyAssociateUserByPropertyId>
    {
        public DeletePropertyAssociateUserByPropertyIdValidator(IPropertyUserRepository _propertyUserRepository, IPropertyRepository _propertyRepository, IWorkContext _workContext)
        {
            RuleFor(m => m.PropertyId).NotNull().GreaterThanOrEqualTo(1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isValid = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isValid)
                {
                    context.AddFailure(nameof(DeletePropertyAssociateUserByPropertyId.PropertyId),"Property id does not exist.");
                }
                else
                {
                    var propertyOwner = await _propertyRepository.GetPropertyById(model.PropertyId);
                    //var propertyUserRoles = await _propertyUserRepository.GetPropertyUsersByPropertyId(model.PropertyId).ConfigureAwait(false);

                    if (propertyOwner != null)
                    {
                        var associateCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(propertyOwner.Id, (int)PropertyUserRelationEnum.Associate).ConfigureAwait(false);
                        var tenantsCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(propertyOwner.Id, (int)PropertyUserRelationEnum.Tenant).ConfigureAwait(false);

                        if (tenantsCount > 0 && propertyOwner.OwnerId == _workContext.CurrentUserId)
                        {
                            context.AddFailure(nameof(DeletePropertyAssociateUserByPropertyId.PropertyId), "You are not authorize to remove tenant's associate users");
                        }
                        if (propertyOwner.OwnerId != _workContext.CurrentUserId)
                        {
                            //Loged in user must be tenant 
                            var isValidTenant = await _propertyUserRepository.IsTenantValid(model.PropertyId, _workContext.CurrentUserId).ConfigureAwait(false);
                            if (!isValidTenant)
                            {
                                context.AddFailure(nameof(DeletePropertyAssociateUserByPropertyId.PropertyId), "You are not authorize to delete associates");
                            }
                        }
                    }
                }
            });
        }
    }
}
