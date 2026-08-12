using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Requests.Company.Command;

namespace Ontec.Core.Application.Company.Command
{
    public class AddUpdateCompanyCommandHandler : IRequestHandler<AddUpdateCompanyQuery, AddUpdateResultDto>,
                                                   IRequestHandler<UpdateCompanyLogo, string>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IAuditTrail _auditTrail;
        private readonly IWorkContext _workContext;
        public AddUpdateCompanyCommandHandler(ICompanyRepository companyRepository,
                                            IDocumentRepository documentRepository,
                                            IAuditTrail auditTrail,
                                             IWorkContext workContext)
        {
            _companyRepository = companyRepository;
            _documentRepository = documentRepository;
            _auditTrail = auditTrail;
            _workContext = workContext;
        }
        public async Task<AddUpdateResultDto> Handle(AddUpdateCompanyQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var objAudit = new AuditHelper();
            var commonValidator = new AddUpdateCompanyQueryValidator(_companyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var response = new AddUpdateResultDto();


            int result;
            var companyId = await _companyRepository.IsCompanyExist(request.Id);

            request.Id = Convert.ToInt32(companyId);

            if (request.Id > 0)
            {
                var getByIdRequest = new AddUpdateCompanyQuery
                {
                    Id = request.Id,

                };

                result = await _companyRepository.UpdateCompany(request).ConfigureAwait(false);
                objAudit.AddedBy = _workContext.CurrentUserId;
                objAudit.Action = "Update Company";
                objAudit.ActionTable = "ohd_company";
                objAudit.ModuleName = "Company";
                objAudit.StatusId = (int)StatusEnum.Active;
                objAudit.UpdatedId = result;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            else
            {
                result = await _companyRepository.AddCompany(request).ConfigureAwait(false);
            }
            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Records created successfully!";
                else
                    response.Message = "Records updated successfully!";
            }
            return response;
        }

        public async Task<string> Handle(UpdateCompanyLogo request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();
            request.TrimAllStrings();
            var commonValidator = new UpdateCompanyLogoValidator(_companyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var uploadDocDto = new UploadDocumentDto
            {
                UploadFile = request.CompanyLogo,
                FileName = request.Id.ToString()+DateTime.UtcNow.ToString("ddMMyyyhhmmss") + Path.GetExtension(request.CompanyLogo.FileName)
            };

            var companyLogoUrl = await _documentRepository.SaveLogo(uploadDocDto).ConfigureAwait(false);

            await _companyRepository.UpdateCompanyLogo(companyLogoUrl, request.Id,request.Type).ConfigureAwait(false);

            objAudit.ModifiedBy = _workContext.CurrentUserId;
            objAudit.Action = "Update Company Logo";
            objAudit.ActionTable = "ohd_company";
            objAudit.ModuleName = "Company";
            objAudit.StatusId = (int)StatusEnum.Active;
            objAudit.UpdatedId = request.Id;
            await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            return "Updated successfully";
        }
    }
}
