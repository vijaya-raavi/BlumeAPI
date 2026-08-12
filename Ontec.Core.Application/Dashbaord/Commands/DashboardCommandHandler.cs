using MediatR;
using Microsoft.AspNetCore.Hosting;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Interface.TopUp;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Requests.Dashboard.Command;

namespace Ontec.Core.Application.Dashbaord.Commands
{
    public class DashboardCommandHandler : IRequestHandler<CaptureUsersCreditsToSaveCommandReuqest, AddUpdateResultDto>
    {
        private readonly ITopUpRepository _topUpRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMeterRepository _meterRepository;
        private IHostingEnvironment _environment;
        private readonly IGenericRepository _genericRepository;
        public DashboardCommandHandler(ITopUpRepository topUpRepository,
                                        IDocumentRepository documentRepository,
                                        IUserRepository userRepository,
                                        IMeterRepository meterRepository,
                                        IHostingEnvironment environment,
                                        IGenericRepository genericRepository)
        {
            _topUpRepository = topUpRepository;
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _meterRepository = meterRepository;
            _environment = environment;
            _genericRepository = genericRepository;
        }
        


        public async Task<AddUpdateResultDto> Handle(CaptureUsersCreditsToSaveCommandReuqest request,CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var validator = new CaptureUsersCreditsToSaveCommandReuqestValidator(_userRepository, _meterRepository);
            var validation = await validator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var response = new AddUpdateResultDto();

            // 1. Save new image to server
            var uploadDto = new UploadDocumentDto
            {
                UploadFile = request.Image,
                FileName = $"{request.MeterId}_{Guid.NewGuid()}_credit_pic{Path.GetExtension(request.Image.FileName)}"
            };

            var imageUrl = await _documentRepository.SaveCreditImages(uploadDto);

            // 2. Get existing images (ordered)
            var images = await _topUpRepository.GetUserImages(request.MeterId);

            // 3. If already 6 → delete oldest file
            if (images.Count == 6)
            {
                var oldest = images.OrderBy(x => x.CreditImageId).FirstOrDefault();

                if (oldest != null && !string.IsNullOrEmpty(oldest.ImageUrl))
                {
                    var fullPath = Path.Combine(_environment.WebRootPath, oldest.ImageUrl.TrimStart('/'));

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath); // 🔥 physical file delete
                    }
                }
            }

            // 4. DB operation (delete + shift + insert)
            var result = await _topUpRepository.SaveCreditImage(request ,imageUrl);

            // 5. Response
            if (result > 0)
            {
                response.Message = "Image saved successfully!";
                response.Id = result;
            }

            return response;
        }

      
    }


}
