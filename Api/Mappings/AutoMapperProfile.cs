using AutoMapper;
using Proyecto_Grupo_gris.Api.DTOs.Comments;
using Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;
using Proyecto_Grupo_gris.Api.DTOs.Forum;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore());
        CreateMap<ApplicationUser, UserProfileDto>();
        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        CreateMap<EcoRoute, EcoRouteDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Nombre : string.Empty));
        CreateMap<CreateEcoRouteDto, EcoRoute>();
        CreateMap<UpdateEcoRouteDto, EcoRoute>();

        CreateMap<ForumPost, ForumPostDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));
        CreateMap<CreateForumPostDto, ForumPost>();
        CreateMap<UpdateForumPostDto, ForumPost>();

        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Nombre : string.Empty));
        CreateMap<CreateCommentDto, Comment>();
        CreateMap<UpdateCommentDto, Comment>();
    }
}
