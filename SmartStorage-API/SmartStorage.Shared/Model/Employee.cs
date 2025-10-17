using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Model;

[Table("Employee", Schema = "dbo")]
public partial class Employee
{
    [Key]
    public int EmpId { get; set; }

    [Required(ErrorMessage ="O nome do colaborador é obrigatório.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
    public string? EmpName { get; set; }

    [Required(ErrorMessage = "O CPF do colaborador é obrigatório.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF com formato incorreto.")]
    public string? EmpCpf { get; set; }

    [Required(ErrorMessage = "O RG do colaborador é obrigatório.")]
    [StringLength(15)]
    public string? EmpRg { get; set; }

    public DateTime EmpDateRegister { get; set; }

    [JsonIgnore]
    public virtual ICollection<Product>? Products { get; set; }
}
