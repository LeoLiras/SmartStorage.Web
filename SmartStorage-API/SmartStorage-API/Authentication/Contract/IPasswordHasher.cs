namespace SmartStorage_API.Authentication.Contract.Tools
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }
}
