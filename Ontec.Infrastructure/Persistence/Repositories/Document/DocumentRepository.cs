using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;

namespace Ontec.Infrastructure.Persistence.Repositories.Document
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IGenericRepository _genericRepository;
        private IHostingEnvironment Environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DocumentRepository(IGenericRepository enericRepository
                                   , IHostingEnvironment _environment
                                   , IHttpContextAccessor httpContextAccessor)
        {
            _genericRepository = enericRepository;
            Environment = _environment;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> AddDocument(DocumentDto request)
        {
            var sQuery = @"INSERT INTO public.ohd_document(
	                       document_type
                           ,url
                           ,title
                           ,extension
                           ,status_id
                           ,created_at
                            ,doc_number)
	                      VALUES (@DocumentTypeId
                            ,@Url
                            ,@Title
                            ,@Extension
                            ,@StatusId
                            ,@CreatedAt
                            ,@DocNumber)
                            RETURNING lastval(); ";
            var parameters = new DynamicParameters();
            parameters.Add("@DocumentTypeId", request.DocumentTypeId);
            parameters.Add("@Url", request.Url);
            parameters.Add("@Title", request.Title);
            parameters.Add("@Extension", request.extension);
            parameters.Add("@StatusId", request.StatusId);
            parameters.Add("@CreatedAt", DateTime.UtcNow);
            if (!string.IsNullOrEmpty(request.Documentnumber))
            {
                parameters.Add("@DocNumber", request.Documentnumber);
            }
            else
            {
                parameters.Add("@DocNumber", null);
            }

            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }

        public async Task<bool> IsDocumentTypeValid(int id)
        {
            var sQuery = @"SELECT id
	                      FROM public.ohd_document_type
                          WHERE id=@Id ";
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);

            var result = await _genericRepository.GetFirstOrDefaultAsync<int>(sQuery, parameters).ConfigureAwait(false);

            return result > 0;
        }

        public async Task<int> UpdateDocument(DocumentDto request)
        {
            var sQuery = @"UPDATE public.ohd_document
	                       SET document_type=@documentTypeId
                            , url=@Url
                            , title=@Title
                            , extension=@Extension
                            , status_id=@StatusId
                            , modified_at=@ModifiedAt
                            ,doc_number=@DocNumber
	                       WHERE id=@Id;
                        SELECT Id FROM public.ohd_document
                        WHERE id=@Id";
            var parameters = new DynamicParameters();
            parameters.Add("@DocumentTypeId", request.DocumentTypeId);
            parameters.Add("@Url", request.Url);
            parameters.Add("@Title", request.Title);
            parameters.Add("@Extension", request.extension);
            parameters.Add("@StatusId", request.StatusId);
            parameters.Add("@ModifiedAt", DateTime.UtcNow);
            parameters.Add("@Id", request.Id);
            if (!string.IsNullOrEmpty(request.Documentnumber))
            {
                parameters.Add("@DocNumber", request.Documentnumber);
            }
            else
            {
                parameters.Add("@DocNumber", null);
            }
            return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
        }
        public async Task<IEnumerable<OntecSelectListItem>> GetDocumentTypeMasters()
        {
            var sQuery = @"	SELECT id, name  
                            FROM public.ohd_document_type
                            where Status_id=@StatusId
	                        ORDER BY Name ASC  ";
            var parameter = new DynamicParameters();
            parameter.Add("@StatusId", (int)StatusEnum.Active);
            var result = await _genericRepository.GetAsync<OntecSelectListItem>(sQuery, parameter).ConfigureAwait(false);
            return result;
        }
        public async Task<int> UploadDocument(UploadDocumentDto request)
        {
            string extension = Path.GetExtension(request.UploadFile.FileName);
            var path = await SaveDocument(request, false).ConfigureAwait(false);
            var doc = new DocumentDto
            {
                Id = request.Id,
                DocumentTypeId = request.DocumentTypeId,
                extension = extension,
                StatusId = (int)StatusEnum.Active,
                Title = request.Title,
                Url = path,
                Documentnumber=request.DocumentNumber,
            };
            if (request.Id == 0)
                return await AddDocument(doc).ConfigureAwait(false);
            else
                return await UpdateDocument(doc).ConfigureAwait(false);
        }
       /* public async Task<string> SaveDocument(UploadDocumentDto request, bool isProfilePic)
        {
            string wwwPath = this.Environment.WebRootPath;
            string contentPath = this.Environment.ContentRootPath;
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";

            var absoluteUrl = domain + "/uploads/";
            string path = Path.Combine(this.Environment.WebRootPath, "uploads");
            if (isProfilePic)
            {
                absoluteUrl += "profiles/";
                path = Path.Combine(this.Environment.WebRootPath, "uploads/profiles");
            }
           
            else
            {
                absoluteUrl += "documents/";
                path = Path.Combine(this.Environment.WebRootPath, "uploads/documents");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = request.FileName;
            string extension = Path.GetExtension(request.UploadFile.FileName.Trim());
            //fileName += extension;
            path = Path.Combine(path, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var file = request.UploadFile;
            using (FileStream fs = System.IO.File.Create(path))
            {
                file.CopyTo(fs);
            }
            absoluteUrl += fileName;
            return absoluteUrl;
        }*/


        public async Task<string> SaveDocument(UploadDocumentDto request, bool isProfilePic)
        {
            string wwwPath = this.Environment.WebRootPath;
            string contentPath = this.Environment.ContentRootPath;
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";

            var absoluteUrl =  "/uploads/";
            string path = Path.Combine(this.Environment.WebRootPath, "uploads");
            if (isProfilePic)
            {
                absoluteUrl += "profiles/";
                path = Path.Combine(this.Environment.WebRootPath, "uploads/profiles");
            }

            else
            {
                absoluteUrl += "documents/";
                path = Path.Combine(this.Environment.WebRootPath, "uploads/documents");
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = request.FileName;
            string extension = Path.GetExtension(request.UploadFile.FileName.Trim());
            //fileName += extension;
            path = Path.Combine(path, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var file = request.UploadFile;
            using (FileStream fs = System.IO.File.Create(path))
            {
                file.CopyTo(fs);
            }
            absoluteUrl += fileName;
            return absoluteUrl;
        }

        public async Task<string> SaveLogo(UploadDocumentDto request)
        {
            string wwwPath = this.Environment.WebRootPath;
            string contentPath = this.Environment.ContentRootPath;
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";

            var absoluteUrl =  "/assets/";
            string path = Path.Combine(this.Environment.WebRootPath, "assets");


            absoluteUrl += "images/";
            path = Path.Combine(this.Environment.WebRootPath, "assets/images");


            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileName = request.FileName;
            string extension = Path.GetExtension(request.UploadFile.FileName.Trim());
            //fileName += extension;
            path = Path.Combine(path, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            var file = request.UploadFile;
            using (FileStream fs = System.IO.File.Create(path))
            {
                file.CopyTo(fs);
            }
            absoluteUrl += fileName;
            return absoluteUrl;
        }
       
       

        public async Task<int> AddApplicationLogger(ApplicationLogger request)
        {
            try
            {
                var sQuery = @"INSERT INTO public.ohd_application_log(
	                       request, error, method)
	                      VALUES (@Request, @Error, @Method)
                        RETURNING lastval();";

                var parameters = new DynamicParameters();
                parameters.Add("@Request", request.Request);
                parameters.Add("@Error", request.Error);
                parameters.Add("@Method", request.Method);

                return await _genericRepository.ExecuteScalarAsync<int>(sQuery, parameters).ConfigureAwait(false);
            }
            catch(Exception ex) {
                return 0;
            }
        }
    }
}