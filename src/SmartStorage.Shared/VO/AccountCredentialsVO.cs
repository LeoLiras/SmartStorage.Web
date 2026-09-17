using SmartStorage.Shared.Enum;

namespace SmartStorage.Shared.VO
{
    public class AccountCredentialsVO
    {
        public AccountCredentialsVO() { }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public TipoUsuario Type { get; set; }
    }
}
