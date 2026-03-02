namespace MyForum.Api.Core.DTOs.Requests
{
    public record AdminLoginRequest(
        string Username,
        string Password
    );
}