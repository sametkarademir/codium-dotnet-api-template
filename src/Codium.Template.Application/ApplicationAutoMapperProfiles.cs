using AutoMapper;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Application;

public class ApplicationAutoMapperProfiles : Profile
{
    public ApplicationAutoMapperProfiles()
    {
        CreateMap<Permission, PermissionResponseDto>();
        
        CreateMap<Role, RoleResponseDto>();
        CreateMap<Role, CreateRoleRequestDto>();
        CreateMap<Role, UpdateRoleRequestDto>();
        
        CreateMap<Session, SessionResponseDto>();
        
        CreateMap<User, UserResponseDto>();
        CreateMap<User, CreateUserRequestDto>();
        CreateMap<User, UpdateUserRequestDto>();
    }
}