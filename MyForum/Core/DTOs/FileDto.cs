namespace MyForum.Core.DTOs
{
    public record FileDto
    (
        int Id,
        string FileName,
        string FilePath,
        long FileSize,
        DateTime UploadedAt
    );
}