using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.PropertyUser.Command
{
    public class AddUpdatePropertyUserValidator : AbstractValidator<AddUpdatePropertyUser>
    {
        public AddUpdatePropertyUserValidator(IPropertyUserRepository _propertyUserRepository
                                              , IPropertyRepository _propertyRepository
                                               , IUserRepository _userRepository
                                                , IMeterRepository _meterRepository
                                                , IWorkContext _workContext
                                                , ICommunicationRepository communicationRepository)
        {
            RuleFor(x => x.TitleId).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.TitleId), 1);
            RuleFor(x => x.CompanyId).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.PropertyId).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.PropertyId).SplitPascalCase(), 1);
            RuleFor(x => x.PropertyUserTypeId).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.PropertyUserTypeId).SplitPascalCase(), 1);
            RuleFor(x => x.Email).NotNullAndEmptyAsync().IsValidEmailId().LengthShouldBeLessOrEqualToAsync(nameof(AddUpdatePropertyUser.Email), 100);
            RuleFor(x => x.Mobile).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeEqualAsync(nameof(AddUpdatePropertyUser.Mobile), 10);
            RuleFor(x => x.FirstName).NotNullAndEmptyAsync().IsValidName(nameof(AddUpdatePropertyUser.FirstName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.FirstName).SplitPascalCase(),2).LengthShouldBeLessOrEqualToAsync(nameof(AddUpdatePropertyUser.FirstName).SplitPascalCase(), 15);
            RuleFor(x => x.LastName).NotNullAndEmptyAsync().IsValidName(nameof(AddUpdatePropertyUser.LastName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(AddUpdatePropertyUser.FirstName).SplitPascalCase(), 2).LengthShouldBeLessOrEqualToAsync(nameof(AddUpdatePropertyUser.LastName).SplitPascalCase(), 15);
            RuleFor(x => x.TaxNumber).LengthShouldBeLessOrEqualToAsync(nameof(AddUpdatePropertyUser.TaxNumber), 20);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                var isExist = await _propertyRepository.IsPropertyIdExist(model.PropertyId).ConfigureAwait(false);
                var propertyUserId = 0;
                if (!isExist)
                {
                    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyId), "Property id does not exist");
                }
                var currentUserId = await _userRepository.GetUserById(model.CurrentUserId).ConfigureAwait(false);
                if (currentUserId == null)
                {
                    context.AddFailure(nameof(AddUpdatePropertyUser.Email), "User doesn't exist in the system");
                }

                if (currentUserId != null && (currentUserId.Email == model.Email || currentUserId.Mobile == model.Mobile))
                {
                    if (model.PropertyUserTypeId == (int)PropertyUserRelationEnum.Tenant)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.Email), "You can not be tenant.");
                    }
                    else
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.Email), "You can not be associate.");
                    }
                }
                if (model.Id > 0)
                {
                    context.AddFailure(nameof(AddUpdatePropertyUser.Id), CommonConstants.PropertyTenantsNotEditable);
                    
                }
                else
                {
                    var userIdByEmail = await _userRepository.IsEmailInTempUserExist(model.Email, model.CompanyId).ConfigureAwait(false);
                    var userIdByMobile = await _userRepository.IsMobileInTempUserExist(model.Mobile, model.CompanyId).ConfigureAwait(false);
                    if (userIdByEmail == 0 && userIdByMobile > 0)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.Mobile), "Mobile already registered with other email id.");
                    }
                    else if (userIdByEmail > 0 && userIdByMobile == 0)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.Email),"Email already registered with other mobile number.");
                    }

                    else if (userIdByMobile == userIdByEmail)
                    {
                        propertyUserId = userIdByEmail;
                    }
                   
                    var isPropertyUserExist = await _propertyUserRepository.IsPropertyUserExist(model.PropertyId, propertyUserId).ConfigureAwait(false);
                    int isPropertyUserInActive = await _propertyUserRepository.IsPropertyUserInActive(model.PropertyId, propertyUserId).ConfigureAwait(false);
                    if (isPropertyUserExist && isPropertyUserInActive == 0)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.Email), string.Format(CommonConstants.AlreadyExist, nameof(AddUpdatePropertyUser.Email)));
                        context.AddFailure(nameof(AddUpdatePropertyUser.Mobile), string.Format(CommonConstants.AlreadyExist, nameof(AddUpdatePropertyUser.Mobile)));
                    }

                }

                var metercount = await _meterRepository.GetMeterCountByPropertyId(model.PropertyId);
                var tenantsCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(model.PropertyId, (int)PropertyUserRelationEnum.Tenant).ConfigureAwait(false);
                var associateCount = await _propertyRepository.GetPropertyUsersCountByPropertyId(model.PropertyId, (int)PropertyUserRelationEnum.Associate).ConfigureAwait(false);

                var property = await _propertyRepository.GetPropertyById(model.PropertyId).ConfigureAwait(false);

                if (property == null)
                {
                    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyId), "Property doen't exist in the system");
                }
                else if (metercount == 0)
                {
                    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyUserTypeId), "To add tenent/associate user for a property their should be atleast one meter against same property");
                }
                else
                {
                    if (property.OwnerId == propertyUserId)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.PropertyUserTypeId), "Tenant can not add property owner as an associate user");
                    }
                    if (tenantsCount > 0 && property.OwnerId == _workContext.CurrentUserId && model.PropertyUserTypeId == (int)PropertyUserRelationEnum.Tenant)
                    {
                        context.AddFailure(nameof(AddUpdatePropertyUser.PropertyUserTypeId), "Tenant already added, only one tenant can be added for a property");
                    }
                    //if (property.OwnerId == _workContext.CurrentUserId && tenantsCount > 0 && model.PropertyUserTypeId == (int)PropertyUserRelationEnum.Associate)
                    //{
                    //    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyUserTypeId), "Only tenant can add user");
                    //}
                    //if (property.OwnerId == _workContext.CurrentUserId && tenantsCount == 0 && associateCount > 0 && model.PropertyUserTypeId == (int)PropertyUserRelationEnum.Tenant)
                    //{
                    //    context.AddFailure(nameof(AddUpdatePropertyUser.PropertyUserTypeId), "Please remove associates before adding tenants.");
                    //}
                }
            });
        }
    }
}