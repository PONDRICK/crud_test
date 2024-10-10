using AutoMapper;
using backend.DTOs;
using backend.Models;

namespace backend.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserPermissions, opt => opt.MapFrom(src => src.UserPermissions));

            CreateMap<CreateUserPermissionDto, UserPermission>();

            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));

            CreateMap<CreatePermissionDto, Permission>()
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => Guid.NewGuid().ToString()));
        }
    }
}
