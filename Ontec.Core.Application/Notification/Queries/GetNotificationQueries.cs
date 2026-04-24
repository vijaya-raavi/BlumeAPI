using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Services;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Interface.UserNotificationConnection;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Notification;
using Ontec.Core.Domain.Requests.Notification.Queries;

namespace Ontec.Core.Application.Notification.Queries
{
    public class GetNotificationQueries : IRequestHandler<SendUserNotificationQuery, bool>
                                          , IRequestHandler<GetNotificationTypeMasterQuery, NotificationTypeMasterDto>
                                          , IRequestHandler<GetNotificationsByUserIdQuery, NotificationsDto>
                                            , IRequestHandler<GetGroupForNotificationQuery, IEnumerable<OntecSelectListItem>>
                                            , IRequestHandler<GetUserstoSendNotificationQuery, IEnumerable<OntecSelectListItem>>
                                              , IRequestHandler<GetGroupWiseUsersQuery, IEnumerable<GroupWiseDetailedDto>>

    {
        private readonly ISignalRService _signalRService;

        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationConnection _userNotificationConnection;
        public GetNotificationQueries(ISignalRService signalRService, INotificationRepository notificationRepository
                                      , IUserRepository userRepository
                                        , IUserNotificationConnection userNotificationConnection)
        {
            _signalRService = signalRService;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _userNotificationConnection = userNotificationConnection;
        }
        public async Task<bool> Handle(SendUserNotificationQuery request, CancellationToken cancellationToken)
        {
            var data = true;
            if (request.UserId != 0)
            {
                var connectionIds = await _userNotificationConnection.GetConnectionsByUserId(request.UserId).ConfigureAwait(false);
                connectionIds = connectionIds.Distinct();
                foreach (var connection in connectionIds)
                {
                    await _signalRService.SendMessage(connection, request.Message);
                }
            }
            return data;
        }
        public async Task<NotificationTypeMasterDto> Handle(GetNotificationTypeMasterQuery request, CancellationToken cancellationToken)
        {
            var notificationTypeMasterDto = new NotificationTypeMasterDto();
            var notificationtypeList = _notificationRepository.GetNotificationTypeMasters();
            await Task.WhenAll(notificationtypeList).ConfigureAwait(false);
            notificationTypeMasterDto.NotificationTypeList = notificationtypeList.Result;
            return notificationTypeMasterDto;
        }

        public async Task<NotificationsDto> Handle(GetNotificationsByUserIdQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new GetNotificationsByUserIdQueryValidator(_userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            return await _notificationRepository.GetNotificationsByUserId(request).ConfigureAwait(false);
        }
        public async Task<IEnumerable<OntecSelectListItem>> Handle(GetGroupForNotificationQuery request, CancellationToken cancellationToken)
        {
            return await _notificationRepository.GetGroups();

        }
        public async Task<IEnumerable<OntecSelectListItem>> Handle(GetUserstoSendNotificationQuery request, CancellationToken cancellationToken)
        {
            return await _notificationRepository.GetUsers();

        }
        public async Task<IEnumerable<GroupWiseDetailedDto>> Handle(GetGroupWiseUsersQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();
           IEnumerable<GroupWiseDetailedDto> dto =new List<GroupWiseDetailedDto>();
            var commonValidator = new GetGroupWiseUsersQueryValidator(_notificationRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var result= await _notificationRepository.GetGroupWiseUsers(request).ConfigureAwait(false);

            dto = result
                     .GroupBy(x => new { x.GroupId, x.GroupName, x.NotificationKey })
                     .Select(group => new GroupWiseDetailedDto
                     {
                         GroupId = group.Key.GroupId,
                         GroupName = group.Key.GroupName,
                         NotificationKey = group.Key.NotificationKey,
                         UserList = group
                             .Where(user => user.UserId > 0) // ✅ Only include valid users
                             .Select(user => new UserDetails
                             {
                                 UserId = user.UserId,
                                 Customer = user.Customer,
                                 DeviceToken = user.DeviceToken
                             })
                             .ToList()
                     })
                     .ToList();

                            return dto;
        }
    }
}
