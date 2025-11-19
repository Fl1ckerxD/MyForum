namespace MyForum.Core.Interfaces.Services
{
    public interface IIPHasher
    {
        string HashIP(string ipAddress);
    }
}