using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Model;

[Table("Product", Schema = "dbo")]
public partial class Product
{
    [Key]
    public int ProId { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
    public string? ProName { get; set; }

    [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
    public string? ProDescription { get; set; }

    [Required(ErrorMessage = "A data de registro do produto é obrigatória.")]
    public DateTime ProDateRegister { get; set; }

    [Required(ErrorMessage = "A quantidade do produto é obrigatória.")]
    public int ProQntd { get; set; }

    public int? ProEmpId { get; set; }

    public byte[]? ProImage { get; set; }

    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [JsonIgnore]
    public virtual ICollection<Enter>? Enters { get; set; }
}
