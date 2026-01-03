namespace MyForum.Api.Core.DTOs.Responses
{
    public class ApiErrorResponse
    {
        public string Error { get; }

        public ApiErrorResponse(string error)
        {
            Error = error;
        }
    }
}