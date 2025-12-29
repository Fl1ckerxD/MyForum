namespace MyForum.Api.Core.DTOs
{
    public record PostDto
    (
        int Id,
        string AuthorName,
        string Content,
        DateTime CreatedAt,
        IEnumerable<FileDto> Files
    );
}