using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public  class AddUpdateAssociateUserSettingsQueryValidator : AbstractValidator<AddUpdateAssociateUserSettingsQuery>
    {
        public AddUpdateAssociateUserSettingsQueryValidator(IUserRepository _userRepository,
                                                            IPropertyRepository _propertyRepository,
                                                            IWorkContext _workContext,
                                                            IPropertyUserRepository _propertyUserRepository)
        {
            RuleFor(x => x.PropertyUserId).GreaterThanOrEqualToAsync(nameof(AddUpdateAssociateUserSettingsQuery.PropertyUserId).SplitPascalCase(), 1);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await _propertyUserRepository.IsPropertyUserIdExist(model.PropertyUserId).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(AddUpdateAssociateUserSettingsQuery.PropertyUserId), "Property user id does not exist.");
                }
                var isPropetyExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                if (!isPropetyExist)
                {
                    context.AddFailure(nameof(AddUpdateAssociateUserSettingsQuery.PropertyId), "Property id does not exist");
                }
                else
                {
                    var tenantsCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(model.PropertyId, (int)PropertyUserRelationEnum.Tenant).ConfigureAwait(false);
                    var property = await _propertyRepository.GetPropertyById(model.PropertyId).ConfigureAwait(false);
                    
                    //if(tenantsCount>0 && property.OwnerId == _workContext.CurrentUserId)
                    //{
                    //    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyId), "Only tenant can update associate user settings.");
                    //}

                    //if (tenantsCount==0 && property.OwnerId!=_workContext.CurrentUserId)
                    //{
                    //    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyId), "Only owner can update associate user settings.");
                    //}
                    
                    var propertyUsers = await _propertyUserRepository.GetPropertyUserLists(model.PropertyId).ConfigureAwait(false);
                    //if(tenantsCount>0 && propertyUsers.Tenant.SystemUserId != _workContext.CurrentUserId)
                    //{
                    //    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyId), "You are not authorized.");
                    //}
                }
            });
        }
    }
}