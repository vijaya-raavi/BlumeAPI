using System.ComponentModel;
using FluentValidation;
using MediatR;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Estate;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Consumption;
using Ontec.Core.Domain.Requests.Login.Queries;

namespace Ontec.Core.Domain.Requests.Property.Command
{
    public class AddOrUpdatePropertyQueryValidator : AbstractValidator<AddOrUpdatePropertyQuery>
    {
        public AddOrUpdatePropertyQueryValidator(IPropertyRepository _propertyRepository
                                                , ICompanyRepository _companyRepository
                                                , IUserRepository _userRepository)
        {
            RuleFor(m => m.Id).NotNull();

            RuleFor(m => m.Name).NotNullAndEmptyAsyncForProperty().LengthShouldBeLessOrEqualToAsync("Property name should be less than or equal to ", 55);
            RuleFor(m => m.UnitNumber).NotNullAndEmptyAsyncForUnitNumber().LengthShouldBeLessOrEqualToAsyncUnitNumber(nameof(AddOrUpdatePropertyQuery.UnitNumber).SplitPascalCase(), 55);
            RuleFor(m => m.OwnerId).NotNull().GreaterThanOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.OwnerId).SplitPascalCase(), 1);
            RuleFor(m => m.CompanyId).NotNull().GreaterThanOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m.AddressLine1).NotNullAndEmptyAsync().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.AddressLine1), 500);
            //RuleFor(m => m.AddressLine2).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.AddressLine2), 500);
            //RuleFor(m => m.City).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.City), 55);
            //RuleFor(m => m.State).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.State), 55);
            //RuleFor(m => m.Country).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdatePropertyQuery.Country), 55);
            RuleFor(m => m).CustomAsync(async (model, context, cancellationToken) =>
            {
                //var isExist = await _propertyRepository.IsUnitNumberExist(model.UnitNumber, model.Id).ConfigureAwait(false);
                //if (isExist)
                //{
                //    context.AddFailure(nameof(AddOrUpdatePropertyQuery.UnitNumber),"Unit number already registered");
                //}

                //isExist = await _propertyRepository.IsPropertyNameExist(model.Name, model.Id).ConfigureAwait(false);
                //if (isExist)
                //{
                //    context.AddFailure(nameof(AddOrUpdatePropertyQuery.Name), string.Format(CommonConstants.AlreadyExist, nameof(AddOrUpdatePropertyQuery.Name)));
                //}

                var user = await _userRepository.GetUserById(model.OwnerId).ConfigureAwait(false);
              
                if (model.Id == 0)
                {
                    var isValid = await _companyRepository.IsCompanyExist(model.CompanyId).ConfigureAwait(false);
                    if (!isValid)
                        context.AddFailure(nameof(AddOrUpdatePropertyQuery.CompanyId), "Company id does not exist");

                    isValid = await _userRepository.IsUserIdExist(model.OwnerId).ConfigureAwait(false);
                    if (!isValid)
                        context.AddFailure(nameof(AddOrUpdatePropertyQuery.OwnerId), "Owner id does not exist");


                }
                else
                {
                    var existingProperty = await _propertyRepository.GetPropertyById(model.Id).ConfigureAwait(false);
                    if (existingProperty.CompanyId != model.CompanyId)
                    {
                        context.AddFailure(nameof(AddOrUpdatePropertyQuery.CompanyId), "company id is invalid");
                    }
                    if (existingProperty.OwnerId != model.OwnerId)
                    {
                        context.AddFailure(nameof(AddOrUpdatePropertyQuery.OwnerId), "Owner id is invalid");
                    }
                }
            });
        }
    }
}
