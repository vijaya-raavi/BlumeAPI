using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Configuration;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Models.Dto.Configuration;
using Ontec.Core.Domain.Requests.Configuration.Queries;
using Ontec.Core.Domain.Requests.ConfigurationSettings.Queries;

namespace Ontec.Core.Application.ConfigurationSettings.Queries
{
    public class ConfigurationSettingsQueryHandler : IRequestHandler<GetConfigurationQuery, Configurations>
                                                       , IRequestHandler<GetRejectionReasonQuery, IEnumerable<RejectionReasonDto>>
    {
        private readonly IConfigurationRepository _configurationRepo;
        private readonly IWorkContext _workContext;
        private IHostingEnvironment Environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMeterRepository _meterRepository;
        public ConfigurationSettingsQueryHandler(IConfigurationRepository configurationRepo
                                        , IWorkContext workContext
                                         , IHostingEnvironment _environment
                                   , IHttpContextAccessor httpContextAccessor
                                    , IMeterRepository meterRepository)
        {
            _configurationRepo = configurationRepo;
            _workContext = workContext;
            Environment = _environment;
            _httpContextAccessor = httpContextAccessor;
            _meterRepository = meterRepository;
        }

        public async Task<Configurations> Handle(GetConfigurationQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var dto = new Configurations();
            var commonValidator = new GetConfigurationQueryValidator(_workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            string wwwPath = this.Environment.WebRootPath;
            string contentPath = this.Environment.ContentRootPath;
            var requestPath = _httpContextAccessor.HttpContext.Request;
            var domain = $"{requestPath.Scheme}://{requestPath.Host}";

            var absoluteUrl = domain;
            var configurations = await _configurationRepo.GetConfigurations().ConfigureAwait(false);
            if (configurations != null && configurations.Any(t => t.Name.ToLower().Equals("backgroundImage")))
            {
                var backgroundUrl = configurations.FirstOrDefault(t => t.Name.ToLower().Equals("backgroundImage"));
                if (backgroundUrl != null && !string.IsNullOrEmpty(backgroundUrl.Value))
                {
                    backgroundUrl.Value = absoluteUrl + backgroundUrl.Value;
                }
                
            }
            if (configurations != null)
            {
                dto.configurations = configurations;
            }
            var meterTypes = await _meterRepository.GetMeterType().ConfigureAwait(false);
            dto.MeterUtilityTypes = meterTypes.ToList();
            dto.businessHoursConfigurations = await _configurationRepo.GetBusinessHoursConfigurations().ConfigureAwait(false);


            return dto;
        }


        public async Task<IEnumerable<RejectionReasonDto>> Handle(GetRejectionReasonQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetRejectionReasonQueryValidator(_configurationRepo, _workContext);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return await _configurationRepo.GetRejectionReason(request).ConfigureAwait(false);
        }
    }
}

