using AutoMapper;
using GameShopWebAPI.DTO;
using GameShopWebAPI.Model;

namespace GameShopWebAPI.Configurations
{
    public class AutoMapperConfig : Profile
    {
        // SignupDTO = ApplicationUser
        public AutoMapperConfig()
        {
            CreateMap<ApplicationUser, SignUpDTO>().ReverseMap()
            .ForMember(f => f.UserName, t2 => t2.MapFrom(src => src.Email));

        }
    }
}
