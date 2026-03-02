namespace MyForum.Api.Core.DTOs
{
    public record BoardDto
    (
        int Id,
        string Name,
        string ShortName,
        string Description,
        bool IsHidden,
        DateTime CreatedAt,
        IEnumerable<ThreadDto> Threads
    );
}