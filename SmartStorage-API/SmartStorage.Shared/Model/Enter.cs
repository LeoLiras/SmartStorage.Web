using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_Shared.Model;

[Table("Enter", Schema = "dbo")]
public partial class Enter
{
    [Key]
    public int EntId { get; set; }

    [Required(ErrorMessage = "A seleção do produto é obrigatória.")]
    public int EntProId { get; set; }

    [Required(ErrorMessage = "A seleção da prateleira é obrigatória.")]
    public int EntSheId { get; set; }

    [Required(ErrorMessage = "A data da entrada é obrigatória.")]
    public DateTime EntDateEnter { get; set; }

    [Required(ErrorMessage = "A quantidade da entrada é obrigatória.")]
    [DefaultValue(typeof(int), "0")]
    public int EntQntd { get; set; }

    [Required(ErrorMessage = "O preço da entrada é obrigatório.")]
    public decimal EntPrice { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sale>? Sales { get; set; }

    [JsonIgnore]
    public virtual Shelf? Shelf { get; set; }
}
