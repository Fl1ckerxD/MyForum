﻿using System.Security.Claims;

namespace MyForum.Models
{
    public static class UserExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            return int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) ? userId : (int?)null;
        }
    }
}
