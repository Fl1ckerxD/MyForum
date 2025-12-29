namespace MyForum.Api.Application.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetClientIp(this HttpContext context)
        {
            var headers = context.Request.Headers;

            if (headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }

            if (headers.TryGetValue("X-Real-IP", out var realIp))
            {
                return realIp.FirstOrDefault();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}