using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using Ontec.Core.Domain.Common;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Helper;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.User;

namespace Ontec.Core.Domain.Requests.User.Commands
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator(IUserRepository _userRepository
                                          , ICommunicationRepository _communicationRepository
                                          , IDocumentRepository _documentRepository
                                          , IUserRoleRepository _userRoleRepository)
        {
            RuleFor(x => x.Id).GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.Id), 1);
            RuleFor(x => x.TitleId).GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.Id), 1);
            RuleFor(x => x.Email).NotNullAndEmptyAsync().IsValidEmailId();
            RuleFor(x => x.Mobile).NotNullAndEmptyAsync().IsValidMobile();
            RuleFor(x => x.FirstName).NotNullAndEmptyAsync().IsValidName(nameof(UpdateUserCommand.FirstName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.FirstName).SplitPascalCase(), 2).LengthShouldBeLessOrEqualToAsync(nameof(UpdateUserCommand.FirstName).SplitPascalCase(), 55);
            RuleFor(x => x.LastName).NotNullAndEmptyAsync().IsValidName(nameof(UpdateUserCommand.LastName).SplitPascalCase()).GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.LastName).SplitPascalCase(), 2).LengthShouldBeLessOrEqualToAsync(nameof(UpdateUserCommand.LastName).SplitPascalCase(), 55);
            RuleFor(x => x.AddressLine1).LengthShouldBeLessOrEqualToAsync(nameof(UpdateUserCommand.AddressLine1), 300);
            RuleFor(x => x.AddressLine2).LengthShouldBeLessOrEqualToAsync(nameof(UpdateUserCommand.AddressLine2), 300);
            RuleFor(x => x.CompanyId).NotNullAndEmptyAsync().GreaterThanOrEqualToAsync(nameof(UpdateUserCommand.CompanyId).SplitPascalCase(), 1);
            RuleFor(x => x.CommunicationTypesIds).NotEmpty();
            //  RuleFor(x => x.TaxNumber).LengthShouldBeLessOrEqualToAsync(nameof(UpdateUserCommand.TaxNumber), 55);
            RuleFor(x => x).CustomAsync(async (model, context, cencellation) =>
            {
                int id = await _userRepository.IsEmailExist(model.Email, model.CompanyId).ConfigureAwait(false);
                if (id != model.Id)
                {
                    context.AddFailure(nameof(UpdateUserCommand.Email), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.Email)));
                }
                id = await _userRepository.IsMobileExist(model.Mobile, model.CompanyId).ConfigureAwait(false);
                if (id != model.Id)
                {
                    context.AddFailure(nameof(UpdateUserCommand.Mobile), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.Mobile)));
                }
                int communicationIdCount = 0;
                if (model.CommunicationTypesIds != null)
                {
                    communicationIdCount = model.CommunicationTypesIds.Count();
                }
                if (communicationIdCount == 0)
                {
                    context.AddFailure(nameof(UpdateUserCommand.CommunicationTypesIds), "Communication type is required");
                }
                else
                {
                    var communications = await _communicationRepository.GetCommunicationMasters().ConfigureAwait(false);
                    var commIds = communications.Select(t => t.Id);

                    var noDbRecords = model.CommunicationTypesIds.Where(m => !commIds.Contains(m));
                    if (noDbRecords.Any())
                    {
                        context.AddFailure(nameof(UpdateUserCommand.CommunicationTypesIds), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.CommunicationTypesIds).SplitPascalCase()));
                    }
                }
                if (model.TitleId > 0)
                {
                    var titleList = await _userRoleRepository.GetTitleMasters();
                    if (!titleList.Any(t => t.Id.Equals(model.TitleId)))
                    {
                        context.AddFailure(nameof(UpdateUserCommand.TitleId), string.Format(CommonConstants.InValid, nameof(UpdateUserCommand.TitleId).SplitPascalCase()));
                    }
                }
                if (model.ProofDocumentTypeId == (int)DocumentTypeEnum.SouthAfricanID)
                {
                    bool isLuhnDigit = false;

                    if (!string.IsNullOrEmpty(model.DocumentNumber))
                    {
                        string doc = model.DocumentNumber;
                        string mmdd = doc.Substring(2, 4); // Extract MMDD
                        if (doc.Length == 13)
                        {
                            int sum = 0;
                            bool doubleDigit = false;

                            // Process right to left
                            for (int i = doc.Length - 1; i >= 0; i--)
                            {
                                int digit = doc[i] - '0';
                                if (doubleDigit)
                                {
                                    digit *= 2;
                                    if (digit > 9) digit -= 9;
                                }
                                sum += digit;
                                doubleDigit = !doubleDigit;
                            }

                            if (sum % 10 == 0)
                            {
                                isLuhnDigit = true;
                            }
                        }
                        if (!isLuhnDigit)
                        {
                            context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.InValid, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));
                        }
                        else
                        {
                            if (model.TitleId == (int)Title.Mrs || model.TitleId == (int)Title.Ms)
                            {
                                if (!DateTime.TryParseExact(mmdd, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                {
                                    context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.SAID, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));
                                }
                                if (!Regex.IsMatch(model.DocumentNumber, "^[0-9]{2}[0-9]{2}[0-9]{2}[0-4][0-9]{3}[0-1]{1}[8]{1}[0-9]{1}$"))
                                //{
                                //if (!Regex.IsMatch(model.DocumentNumber, "^[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[0-4][0-9]{3}[0-1][8][0-9]$"))
                                {
                                    context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.SAID, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));
                                }
                            }
                            if (model.TitleId == (int)Title.Mr)
                            {

                                if (!DateTime.TryParseExact(mmdd, "MMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                {
                                    context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.SAID, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));
                                }
                                if (!Regex.IsMatch(model.DocumentNumber, "^[0-9]{2}[0-9]{2}[0-9]{2}[5-9][0-9]{3}[0-1]{1}[8]{1}[0-9]{1}$"))
                                //{
                                //if (!Regex.IsMatch(model.DocumentNumber, "^[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[5-9][0-9]{3}[0-1][8][0-9]$"))
                                {

                                    context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.SAID, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));

                                }
                            }
                        }
                    }
                }
                if (model.Id > 0 && model.ProofDocument != null && model.ProofDocumentTypeId == (int)DocumentTypeEnum.SouthAfricanID || model.ProofDocumentTypeId == (int)DocumentTypeEnum.Passport)
                {
                    if (string.IsNullOrEmpty(model.DocumentNumber) || model.DocumentNumber == "string")
                    {
                        context.AddFailure(nameof(UpdateUserCommand.DocumentNumber), string.Format(CommonConstants.IsRequired, nameof(UpdateUserCommand.DocumentNumber).SplitPascalCase()));
                    }

                }
                //if (model.Id > 0)
                //{
                //    var user = await _userRepository.GetUserById(model.Id).ConfigureAwait(false);
                //    if (user.IsBusiness)
                //    {
                //        if (string.IsNullOrEmpty(model.TaxNumber) || model.TaxNumber == "string")
                //        {
                //            context.AddFailure(nameof(UpdateUserCommand.TaxNumber), string.Format(CommonConstants.IsRequired, nameof(UpdateUserCommand.TaxNumber).SplitPascalCase()));
                //        }
                //    }
                //}
                if (model.Id > 0 && model.ProofDocument != null)
                {
                    var isValid = true;
                    string[] supportedTypes = [".jpg", ".jpeg", ".png", ".pdf"];
                    var fileExt = System.IO.Path.GetExtension(model.ProofDocument.FileName);
                    if (!supportedTypes.Contains(fileExt.ToLower()))
                    {
                        isValid = false;
                    }
                    if (model.ProofDocument.Length > (50 * 1024 * 1024))//50 MB 
                    {
                        isValid = false;
                    }
                    if (!isValid)
                    {
                        context.AddFailure(nameof(UpdateUserDocumentQuery.ProofDocument), "Allowed file formats (jpg, jpeg, png, pdf) with 50 MB");
                    }
                    if (model.ProofDocumentTypeId != 0)
                    {
                        var isIdValid = await _documentRepository.IsDocumentTypeValid(model.ProofDocumentTypeId).ConfigureAwait(false);
                        if (!isIdValid)
                        {
                            context.AddFailure(nameof(UpdateUserCommand.ProofDocumentTypeId), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.ProofDocumentTypeId).SplitPascalCase()));
                        }
                    }
                    else
                    {
                        context.AddFailure(nameof(UpdateUserCommand.ProofDocumentTypeId), string.Format(CommonConstants.IsRequired, nameof(UpdateUserCommand.ProofDocumentTypeId).SplitPascalCase()));
                    }
                }

                if (model.ProofDocument != null)
                {
                    var isValid = true;
                    string[] supportedTypes = [".jpg", ".jpeg", ".png", ".pdf"];
                    var fileExt = System.IO.Path.GetExtension(model.ProofDocument.FileName);
                    if (!supportedTypes.Contains(fileExt.ToLower()))
                        isValid = false;
                    if (model.ProofDocument.Length > (50 * 1024 * 1024))//50 MB 
                        isValid = false;
                    if (!isValid)
                        context.AddFailure(nameof(UpdateUserCommand.ProofDocument), "Allowed file formats (jpg, jpeg, png, pdf) with 50 MB");
                    if (model.ProofDocumentTypeId != 0)
                    {
                        var isIdValid = await _documentRepository.IsDocumentTypeValid(model.ProofDocumentTypeId).ConfigureAwait(false);
                        if (!isIdValid)
                        {
                            context.AddFailure(nameof(UpdateUserCommand.ProofDocumentTypeId), string.Format(CommonConstants.NotExist, nameof(UpdateUserCommand.ProofDocumentTypeId).SplitPascalCase()));
                        }
                    }
                }
            });
        }
    }
}
