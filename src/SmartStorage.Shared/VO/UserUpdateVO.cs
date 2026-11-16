using SmartStorage.Shared.Enum;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage.Shared.VO
{
    // Sem senha: a que o administrador define tem rota própria.
    public partial class UserUpdateVO
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(120, ErrorMessage = "Insira no máximo 120 caracteres.")]
        public string FullName { get; set; }

        [EnumDataType(typeof(TipoUsuario), ErrorMessage = "Tipo de usuário inválido.")]
        public TipoUsuario Type { get; set; }
    }
}
