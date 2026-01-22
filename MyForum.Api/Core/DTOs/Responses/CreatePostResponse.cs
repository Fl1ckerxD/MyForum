namespace MyForum.Api.Core.DTOs.Responses
{
    public record CreatePostResponse
    (
        int Id,
        string AuthorName,
        string Content,
        DateTime CreatedAt,
        IEnumerable<FileDto>? Files
    );
}