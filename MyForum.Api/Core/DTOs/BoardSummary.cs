namespace MyForum.Api.Core.DTOs
{
    public record BoardSummary
    (
        int Id,
        string Name,
        string ShortName,
        string Description
    );
}