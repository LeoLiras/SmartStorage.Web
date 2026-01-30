using SmartStorage.Shared.Enum;

namespace SmartStorage_Shared.DTO
{
    public class AccountCredentialsDTO
    {
        public AccountCredentialsDTO() { }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public TipoUsuario Type { get; set; }
    }
}
