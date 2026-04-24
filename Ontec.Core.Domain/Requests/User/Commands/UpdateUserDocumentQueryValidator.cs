using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Requests.Meter.Command;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserDocumentQueryValidator : AbstractValidator<UpdateUserDocumentQuery>
    {
        public UpdateUserDocumentQueryValidator(IUserRepository _userRepository, IDocumentRepository _documentRepository,IWorkContext _workContext)
        {
            RuleFor(x => x.UserId).GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.Id), 1);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                var isExist = await _userRepository.IsUserIdExist(model.UserId).ConfigureAwait(false);
                if (!isExist)
                {
                    context.AddFailure(nameof(ChangePasswordQuery.UserId), string.Format(CommonConstants.NotExist, nameof(ChangePasswordQuery.UserId).SplitPascalCase()));
                }
                else
                {
                    if (_workContext.CurrentUserId != model.UserId)
                    {
                        context.AddFailure(nameof(UpdateUserDocumentQuery.UserId), "You are not authorize.");
                    }
                    if (model.UserId > 0 && model.ProofDocument != null)
                    {
                        var isValid = true;
                        string[] supportedTypes = [".jpg", ".jpeg", ".png", ".pdf"];
                        var fileExt = System.IO.Path.GetExtension(model.ProofDocument.FileName);
                        if (!supportedTypes.Contains(fileExt.ToLower()))
                        {
                            isValid = false;
                        }
                        if (model.ProofDocument.Length > (5 * 1024 * 1024))//5 MB 
                        {
                            isValid = false;
                        }
                        if (!isValid)
                        {
                            context.AddFailure(nameof(UpdateUserDocumentQuery.ProofDocument), "Allowed file formats (jpg, jpeg, png, pdf) with 10 MB");
                        }
                        if (model.ProofDocumentTypeId != 0)
                        {
                            var isIdValid = await _documentRepository.IsDocumentTypeValid(model.ProofDocumentTypeId).ConfigureAwait(false);
                            if (!isIdValid)
                            {
                                context.AddFailure(nameof(UpdateUserDocumentQuery.ProofDocumentTypeId), string.Format(CommonConstants.NotExist, nameof(UpdateUserDocumentQuery.ProofDocumentTypeId).SplitPascalCase()));
                            }
                        }
                        else
                        {
                            context.AddFailure(nameof(UpdateUserDocumentQuery.ProofDocumentTypeId), string.Format(CommonConstants.IsRequired, nameof(UpdateUserDocumentQuery.ProofDocumentTypeId).SplitPascalCase()));
                        }
                    }
                }
            });
        }
    }
}
