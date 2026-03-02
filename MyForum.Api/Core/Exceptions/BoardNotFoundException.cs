namespace MyForum.Api.Core.Exceptions
{
    [Serializable]
    public class BoardNotFoundException : Exception
    {
        public BoardNotFoundException(string message)
            : base(message)
        {
        }

        public BoardNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}