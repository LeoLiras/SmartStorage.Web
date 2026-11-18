using System.ComponentModel.DataAnnotations;

namespace SmartStorage.Shared.VO
{
    // Senha que o administrador define para outro usuário; a própria muda pelo my-account, com a senha atual.
    public partial class UserPasswordVO
    {
        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        public string NewPassword { get; set; }
    }
}
