using AutoMapper;
using Ontec.Core.Domain.BankNotification;
using Ontec.Notification.Api.Models;

namespace Ontec.Notification.Api.Mapper
{
    public class ApiModelsProfile : Profile
    {
        public ApiModelsProfile()
        {
            CreateMap<NotificationRequest, BankNotificationDetails>();
        }
    }
}
