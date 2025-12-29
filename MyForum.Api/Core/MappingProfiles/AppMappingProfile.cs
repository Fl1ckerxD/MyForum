using AutoMapper;
using MyForum.Api.Core.DTOs;
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
            CreateMap<Thread, ThreadDto>()
                .ForMember(dest => dest.OriginalPost, opt => opt.MapFrom(src => src.Posts.FirstOrDefault()));
            CreateMap<Post, PostDto>();
            CreateMap<PostFile, FileDto>();
        }
    }
}