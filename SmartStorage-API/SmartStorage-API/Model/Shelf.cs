using System.ComponentModel.DataAnnotations;

namespace SmartStorage_API.Model;

public partial class Shelf
{
    [Key]
    public int SheId { get; set; }

    [Required(ErrorMessage = "O nome da prateleira é obrigatório.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
    public string? SheName { get; set; }

    [Required(ErrorMessage = "A data de registro da prateleira é obrigatória.")]
    public DateTime SheDataRegister { get; set; }

    public virtual ICollection<Enter>? Enters { get; set; }
}
