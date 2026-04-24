using MediatR;
using Ontec.Core.Application.Common.Exceptions;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Enums;
using Ontec.Core.Domain.Extension;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Company;
using Ontec.Core.Domain.Interface.Notifiation;
using Ontec.Core.Domain.Interface.Property;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Requests.Notification.Command;
using Ontec.Core.Domain.Requests.Property.Command;
using Ontec.Core.Domain.Requests.Property.Handler;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Ontec.Core.Application.Property.Handler.Command
{
    public class PropertyCommandHandler : IRequestHandler<AddOrUpdatePropertyQuery, AddUpdateResultDto>
                                          , IRequestHandler<DeletePropertyById, AddUpdateResultDto>
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IWorkContext _workContext;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IAuditTrail _auditTrail;
        public PropertyCommandHandler(IPropertyRepository propertyRepository
                                     , IWorkContext workContext
                                     , ICompanyRepository companyRepository
                                      , IUserRepository userRepository
                                    , INotificationRepository notificationRepository,
IAuditTrail auditTrail)
        {
            _propertyRepository = propertyRepository;
            _workContext = workContext;
            _companyRepository = companyRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _auditTrail = auditTrail;
        }
        public async Task<AddUpdateResultDto> Handle(AddOrUpdatePropertyQuery request, CancellationToken cancellationToken)
        {
            request.TrimAllStrings();

            var commonValidator = new AddOrUpdatePropertyQueryValidator(_propertyRepository, _companyRepository, _userRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);

            var response = new AddUpdateResultDto();
            int result;
            var user = await _userRepository.GetUserById(_workContext.CurrentUserId).ConfigureAwait(false);
            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);


            if (request.Id > 0)
            {
                var getByIdRequest = new GetPropertyByIdQuery
                {
                    Id = request.Id,
                    CompanyId = request.CompanyId,
                };
                var property = await _propertyRepository.GetPropertyById(getByIdRequest.Id).ConfigureAwait(false);
                property.Name = request.Name;
                property.OwnerId = request.OwnerId;
                property.CompanyId = request.CompanyId;
                property.UnitNumber = request.UnitNumber;
                property.AddressLine1 = request.AddressLine1;
                if (user.IsEstateEnable == "1")
                {
                    property.EstateId = request.EstateId.Value;
                }


                //property.AddressLine2 = request.AddressLine2;
                //property.City = request.City;
                //property.State = request.State;
                //property.Country = request.Country;

                result = await _propertyRepository.UpdateProperty(property).ConfigureAwait(false);
            }
            else
            {

                request.OwnerId = _workContext.CurrentUserId;
                request.StatusId = (int)StatusEnum.Active;
                result = await _propertyRepository.AddProperty(request).ConfigureAwait(false);

                AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                {
                    UserID = _workContext.CurrentUserId,
                    Title = "New property registered",
                    Description = request.Name + " - " + "property registered in " + company.CompanyName,
                    IsRead = (int)StatusEnum.Sent,
                    NotificationType = (int)NotificationType.Register

                };
                if (user.IsEstateEnable == "1" && request.EstateId > 0)
                {
                    var usersIds = new List<int>();
                    int groupId = await _notificationRepository.GetGroupIdByEstateId(request.EstateId.Value).ConfigureAwait(false);
                    usersIds.Add(request.OwnerId);
                    var addUserInTopicRequest = new SubscribeTopicsforUsersRequestQuery()
                    {
                        GroupId = groupId,
                        UserIds = usersIds
                    };

                    await _notificationRepository.AddCustomersInNotificationTopics(addUserInTopicRequest).ConfigureAwait(false);
                }
                await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
            }

            if (result > 0)
            {
                response.Id = result;
                if (request.Id == 0)
                    response.Message = "Records created successfully!";


                else
                {
                    AddOrUpdateNotificationsQuery newNotification = new AddOrUpdateNotificationsQuery
                    {
                        UserID = _workContext.CurrentUserId,
                        Title = "Property updated",
                        Description = request.Name + " - " + "property updated in " + company.CompanyName,
                        IsRead = (int)StatusEnum.Sent,
                        NotificationType = (int)NotificationType.Updated

                    };
                    await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);
                    response.Message = "Records updated successfully!";
                }
            }
            return response;
        }

        public async Task<AddUpdateResultDto> Handle(DeletePropertyById request, CancellationToken cancellationToken)
        {
            var objAudit = new AuditHelper();
            request.TrimAllStrings();
            var res = new AddUpdateResultDto();
            var commonValidator = new DeletePropertyByIdValidator(_propertyRepository);
            var validatorResult = await commonValidator.ValidateAsync(request, cancellationToken);
            if (!validatorResult.IsValid)
                throw new ValidationException(validatorResult.Errors);
            var property = await _propertyRepository.GetAllPropertyById(request.Id).ConfigureAwait(false);
            res.Id = await _propertyRepository.DeletePropertyById(request).ConfigureAwait(false);
            if (_workContext.CurrentRoleId == (int)RoleMasterEnum.Admin || _workContext.CurrentRoleId == (int)RoleMasterEnum.Operator)
            {
                objAudit.ModifiedBy = _workContext.CurrentUserId;
                objAudit.ActionTable = "ohd_property";
                objAudit.ModuleName = "Property";
                if (request.IsRestore)
                {
                    objAudit.StatusId = (int)StatusEnum.Active;
                    objAudit.Action = "Restore Property";
                }
                if (request.IsPermanentDelete)
                {
                    objAudit.StatusId = (int)StatusEnum.Deactive;
                    objAudit.Action = "Deactive Property";
                }
                if (!request.IsPermanentDelete && !request.IsRestore)
                {
                    objAudit.StatusId = (int)StatusEnum.Inactive;
                    objAudit.Action = "Delete Property";
                }
                objAudit.EntityName = property.Name;
                objAudit.UpdatedId = res.Id;
                await _auditTrail.AuditTrail(objAudit).ConfigureAwait(false);
            }
            
            var propertyName = property.Name;

            var user = await _userRepository.GetUserById(_workContext.CurrentUserId).ConfigureAwait(false);
            var company = await _companyRepository.GetCompanyDetails(user.CompanyId).ConfigureAwait(false);
            AddOrUpdateNotificationsQuery newNotification = new()
            {
                UserID = _workContext.CurrentUserId,
                Title = "Property deleted",
                Description = "Property : " + propertyName + " deleted from " + company.CompanyName,
                IsRead = (int)StatusEnum.Sent,
                NotificationType = (int)NotificationType.Deleted

            };
            await _notificationRepository.AddNotifications(newNotification).ConfigureAwait(false);

            res.Message = "Property updated successfully!";
            return res;
        }

    }
}
