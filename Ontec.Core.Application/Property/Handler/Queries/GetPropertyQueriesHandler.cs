using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Property;
using Ontec.Core.Domain.Requests.Property.Handler;

namespace Ontec.Core.Application.Property.Handler.Queries
{
    public class GetPropertyQueriesHandler : IRequestHandler<GetPropertiesByOwnerIdQuery, IEnumerable<PropertyDto>>
                                             , IRequestHandler<GetPropertyByIdQuery, PropertyModelDto>
                                            , IRequestHandler<GetAllPropertiesRequestQuery, DatatableModel<PropertyDto>>


    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IUserRepository _userRepository;
        public GetPropertyQueriesHandler(IPropertyRepository propertyRepository
                                          , ICompanyRepository companyRepository
                                          , IUserRepository userRepository)
        {
            _propertyRepository = propertyRepository;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
        }
        public async Task<IEnumerable<PropertyDto>> Handle(GetPropertiesByOwnerIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetPropertiesByOwnerIdQueryValidator(_userRepository,_companyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            return await _propertyRepository.GetPropertiesByOwnerId(request).ConfigureAwait(false);
        }

        public async Task<PropertyModelDto> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetPropertyByIdQueryValidator(_propertyRepository, _companyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);


            return await _propertyRepository.GetPropertyById(request.Id).ConfigureAwait(false);
        }
        #region GetAllProperties
        public async Task<DatatableModel<PropertyDto>> Handle(GetAllPropertiesRequestQuery request, CancellationToken cancellationToken)
        {
            return await _propertyRepository.GetAllPropertiesNew(request);
        }
        #endregion
    }
}
