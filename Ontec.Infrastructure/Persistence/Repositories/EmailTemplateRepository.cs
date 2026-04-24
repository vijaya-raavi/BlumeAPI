using Dapper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.EmailTemplate;
using Ontec.Core.Domain.Models.Dto.EmailTemplates;
using Ontec.Core.Domain.Requests.EmailTemplates.Command;

namespace Ontec.Infrastructure.Persistence.Repositories
{
    public class EmailTemplateRepository: IEmailTemplateRepository
    {
        private readonly IGenericRepository _genericRepository;
        public EmailTemplateRepository(IGenericRepository genericRepository) 
        {
        _genericRepository = genericRepository;
        }
        public async Task<IEnumerable<EmailTemplateDto>> GetEmailTemplates()
        {

            var sQuery = @"SELECT id as Id,
                            template_name as Name,
                            html As Html
                            FROM public.ohd_email_template_master
                            ORDER BY id ASC";
            try
            {
                var result = await _genericRepository.GetAsync<EmailTemplateDto>(sQuery).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return new List<EmailTemplateDto>();
            }
        }
        public async Task<int> IsEmailTemplateIdExists(int id)
        {

            var sQuery = @"SELECT id                            
                            FROM public.ohd_email_template_master
                            WHERE id=@Id";

            var parameter = new DynamicParameters();
            parameter.Add("@Id", id);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery,parameter).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public async Task<int> UpdateEmailTemplate(AddEditEmailTemplateCommandRequest request)
        {

            var sQuery = @"UPDATE public.ohd_email_template_master
                            SET html=@HTML
                            WHERE id=@Id;
                            SELECT id FROM public.ohd_email_template_master
                            WHERE id=@Id;";

            var parameter = new DynamicParameters();
            parameter.Add("@Id", request.Id);
            parameter.Add("@HTML", request.HtmlContent);
            try
            {
                var result = await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameter).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
