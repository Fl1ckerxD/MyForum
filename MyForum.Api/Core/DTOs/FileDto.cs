namespace MyForum.Api.Core.DTOs
{
    public record FileDto
    (
        int Id,
        string FileName,
        string FileUrl,
        string? ThumbnailUrl,
        long FileSize,
        int? Width,
        int? Height
    );
}