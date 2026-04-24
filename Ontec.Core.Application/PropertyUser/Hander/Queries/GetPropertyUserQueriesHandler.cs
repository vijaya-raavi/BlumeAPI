using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.PropertyUser;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.PropertyUser;
using Ontec.Core.Domain.Requests.PropertyUser.Queries;

namespace Ontec.Core.Application.PropertyUser.Hander.Queries
{
    public class GetPropertyUserQueriesHandler : IRequestHandler<GetPropertyUserMastersByOwnerIdQuery, EditPropertyUserMasters>
                                                , IRequestHandler<GetPropertyUserById, AddEditPropertyUser>
                                                , IRequestHandler<GetPropertyUserListByPropertyIdQuery, PropertyUserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPropertyUserRepository _propertyUserRepository;
        private readonly IPropertyRepository _propertyRepository;
        public GetPropertyUserQueriesHandler(IUserRepository userRepository
                                        , IPropertyRepository propertyRepository
                                        , IPropertyUserRepository propertyUserRepository)
        {
            _userRepository = userRepository;
            _propertyUserRepository = propertyUserRepository;
            _propertyRepository = propertyRepository;
        }
        public async Task<EditPropertyUserMasters> Handle(GetPropertyUserMastersByOwnerIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetPropertyUserMastersByOwnerIdQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _propertyUserRepository.GetEditPropertyUserMasters(request.OwnerId).ConfigureAwait(false);
        }

        public async Task<AddEditPropertyUser> Handle(GetPropertyUserById request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetPropertyUserByIdValidator(_propertyUserRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _propertyUserRepository.GetPropertyUserById(request.Id).ConfigureAwait(false);
        }

        public async Task<PropertyUserDto> Handle(GetPropertyUserListByPropertyIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var commonValidator = new GetPropertyUserListByPropertyIdQueryValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _propertyUserRepository.GetPropertyUserLists(request.PropertyId).ConfigureAwait(false);
        }
    }
}