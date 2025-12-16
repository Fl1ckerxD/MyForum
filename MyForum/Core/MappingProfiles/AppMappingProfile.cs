using AutoMapper;
using MyForum.Core.DTOs;
using MyForum.Core.Entities;
using Thread = MyForum.Core.Entities.Thread;

namespace MyForum.Core.MappingProfiles
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