using AutoMapper;
using Ontec.Core.Domain.Models.Dto.Login;
using Ontec.Core.Domain.Models.Dto.User;

namespace Ontec.WebUI.Helper
{
    public class AutoMappingConfig : Profile
    {
        public AutoMappingConfig()
        {
            CreateMap<LoggedUserDto, LoginResult>();
        }
    }
}
