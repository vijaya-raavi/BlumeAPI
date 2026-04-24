using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Communication;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.Status;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.User;
using Ontec.Core.Domain.Requests.User.Queries;

namespace Ontec.Core.Application.User.Handler.Queries
{
    public class UserQueryHandler : IRequestHandler<GetUserByEmailComapnyId, UserDto>,
                                    IRequestHandler<GetUserByIdQuery, UserProfileDto>,
                                    IRequestHandler<GetUserMastersQuery, UserMasters>,
                                    IRequestHandler<GetRegistrationRequestQuery,DatatableModel<GetRegistrationRequestDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IDocumentRepository _documentRepository;
        public UserQueryHandler(IUserRepository userRepository
                                , ICommunicationRepository communicationRepository
                                , IUserRoleRepository userRoleRepository
                                , IStatusRepository statusRepository
                                , IDocumentRepository documentRepository)
        {
            _userRepository = userRepository;
            _communicationRepository = communicationRepository;
            _userRoleRepository = userRoleRepository;
            _statusRepository = statusRepository;
            _documentRepository = documentRepository;
        }

        public async Task<UserDto> Handle(GetUserByEmailComapnyId request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetUserByEmailComapnyIdValidator();
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _userRepository.GetUserByEmailComapnyId(request).ConfigureAwait(false);
        }

        public async Task<UserProfileDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
            var userProfile = new UserProfileDto();
            var commonValidator = new GetUserByIdQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            
            int count = await _userRepository.IsSessionKeyExist(request.SessionKey).ConfigureAwait(false);
            if (count > 0)
                return await _userRepository.GetUserById(request.Id).ConfigureAwait(false);
            
            else
                return userProfile;
            
        }

        public async Task<UserMasters> Handle(GetUserMastersQuery request, CancellationToken cancellationToken)
        {
            var userMasters = new UserMasters();
            var communicationTypeList = _communicationRepository.GetCommunicationMasters();
            var roleMasterList = _userRoleRepository.GetRoleMasters();
            var statusList = _statusRepository.GetStatusMaster();
            var titleList = _userRoleRepository.GetTitleMasters();
            var proofDocumentTypeList = _documentRepository.GetDocumentTypeMasters();

            await Task.WhenAll(communicationTypeList, roleMasterList, statusList, titleList, proofDocumentTypeList).ConfigureAwait(false);

            userMasters.CommunicationTypeList = communicationTypeList.Result;
            userMasters.RoleMasterList = roleMasterList.Result;
            userMasters.StatusList = statusList.Result;
            userMasters.TitleList = titleList.Result;
            userMasters.ProofDocumentTypeList = proofDocumentTypeList.Result.Where(t=>t.Id>1).ToList();

            return userMasters;
        }
       
        #region GetRegistrationRequest
        public async Task<DatatableModel<GetRegistrationRequestDto>> Handle(GetRegistrationRequestQuery request, CancellationToken cancellationToken)
        {
            return await _userRepository.GetRegistrationRequests(request);
        }
        #endregion
    }
}
