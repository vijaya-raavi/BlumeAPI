using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Login.Command;
using Ontec.Core.Domain.Requests.User.Commands;

namespace Ontec.Core.Domain.Requests.Operator.Command
{
    public class AddOrUpdateOperatorQueryValidator : AbstractValidator<AddOrUpdateOperatorQuery>
    {
        public AddOrUpdateOperatorQueryValidator(IUserRepository _userRepository)
        {
            RuleFor(m => m.Id).NotNull();
            RuleFor(x=>x.EmailId).NotNullAndEmptyAsync().IsValidEmailId();
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.CompanyId).SplitPascalCase(), 1);
            RuleFor(m => m.FirstName).NotNullAndEmptyAsync().IsValidName(nameof(AddOrUpdateOperatorQuery.FirstName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.FirstName).SplitPascalCase(), 2).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.FirstName).SplitPascalCase(), 15);
            RuleFor(m => m.LastName).NotNullAndEmptyAsync().IsValidName(nameof(AddOrUpdateOperatorQuery.LastName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.LastName).SplitPascalCase(),2).LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.LastName).SplitPascalCase(), 15);
            RuleFor(x => x.MobileNumber).NotNullAndEmptyAsync().IsValidMobile().LengthShouldBeEqualAsync(nameof(AddOrUpdateOperatorQuery.MobileNumber).SplitPascalCase(), 10);
            RuleFor(x => x.Password).NotNullAndEmptyAsync().IsValidPassword().LengthShouldBeLessOrEqualToAsync(nameof(AddOrUpdateOperatorQuery.Password), 15);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.Id).ConfigureAwait(false);
                
                if (model.Id == 0 && !isExist)
                {
                    int id = await _userRepository.IsOperatorEmailExist(model.EmailId, model.CompanyId).ConfigureAwait(false);
                    if (id != 0)
                    {
                        context.AddFailure(nameof(AddOrUpdateOperatorQuery.EmailId),"Email id already registered");
                    }
                   int Emailid = await _userRepository.IsOperatorMobileExist(model.MobileNumber, model.CompanyId).ConfigureAwait(false);
                    if (Emailid != 0)
                    {
                        context.AddFailure(nameof(AddOrUpdateOperatorQuery.MobileNumber), "Mobile number already registered.");
                    }
                }
                
            });

        }
    }
}
