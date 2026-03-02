using AutoMapper;
using MyForum.Api.Core.DTOs;
using MyForum.Api.Core.DTOs.Responses;
using MyForum.Api.Core.Entities;
using Thread = MyForum.Api.Core.Entities.Thread;

namespace MyForum.Api.Core.MappingProfiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<Board, BoardDto>();
            CreateMap<Board, BoardNamesDto>();
            CreateMap<Board, BoardSummary>();
            CreateMap<Thread, ThreadDto>()
                .ForMember(dest => dest.OriginalPost, opt => opt.MapFrom(src => src.Posts.First(p => p.IsOriginal)));
            CreateMap<Post, PostDto>();
            CreateMap<Post, CreatePostResponse>();
            CreateMap<Ban, BanDto>()
                .ForMember(dest => dest.BoardShortName, opt => opt.MapFrom(src => src.Board != null ? src.Board.ShortName : null))
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src =>
                    src.ExpiresAt != null && src.ExpiresAt <= DateTime.UtcNow))
                .ForMember(dest => dest.IsCurrentlyActive, opt => opt.MapFrom(src =>
                    src.IsActive && (src.ExpiresAt == null || src.ExpiresAt > DateTime.UtcNow)));
        }
    }
}
