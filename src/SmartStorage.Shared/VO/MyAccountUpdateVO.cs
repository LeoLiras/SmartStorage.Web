using System.ComponentModel.DataAnnotations;

namespace SmartStorage.Shared.VO
{
    public partial class MyAccountUpdateVO
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(120, ErrorMessage = "Insira no máximo 120 caracteres.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "A senha atual é obrigatória.")]
        public string CurrentPassword { get; set; }

        // Opcional: vazio altera só o nome.
        public string NewPassword { get; set; }
    }
}
