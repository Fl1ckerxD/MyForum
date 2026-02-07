namespace MyForum.Api.Core.DTOs.Responses
{
    public record AdminLoginResponse(
        int Id,
        string Username,
        string Role
    );
}