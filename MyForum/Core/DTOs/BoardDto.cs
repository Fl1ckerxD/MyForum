namespace MyForum.Core.DTOs
{
    public record BoardDto
    (
        int Id,
        string Name,
        string ShortName,
        string Description,
        IEnumerable<ThreadDto> Threads
    );
}