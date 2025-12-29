namespace MyForum.Api.Core.Interfaces.Services
{
    public interface IIPHasher
    {
        string HashIP(string ipAddress);
    }
}