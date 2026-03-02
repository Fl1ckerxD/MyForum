namespace MyForum.Api.Core.Exceptions
{
    public class AccountCreationException : Exception
    {
        public AccountCreationException(string message)
            : base(message)
        {
        }

        public AccountCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}