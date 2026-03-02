namespace MyForum.Api.Core.Exceptions
{
    public class UserAlreadyBannedException : Exception
    {
        public UserAlreadyBannedException(string message)
            : base(message)
        {
        }

        public UserAlreadyBannedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}