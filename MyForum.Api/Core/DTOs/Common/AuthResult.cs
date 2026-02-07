using System.Security.Claims;
using MyForum.Api.Core.DTOs.Responses;

namespace MyForum.Api.Core.DTOs.Common
{

    public record AuthResult(
        ClaimsPrincipal Principal,
        AdminLoginResponse Response
    );
}