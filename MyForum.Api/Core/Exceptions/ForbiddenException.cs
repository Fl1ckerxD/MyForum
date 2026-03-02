namespace MyForum.Api.Core.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Доступ запрещён")
            : base(message)
        {
        }

        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}