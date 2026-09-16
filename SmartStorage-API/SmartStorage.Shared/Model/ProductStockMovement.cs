using SmartStorage.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_Shared.Model;

/// <summary>
/// Uma linha por efeito de estoque, sempre com sinal: positivo entra, negativo sai.
/// PsmSheId nulo significa deposito, e por isso a transferencia entre prateleiras
/// vira dois lancamentos - sai de uma, entra na outra - em vez de uma linha com
/// origem e destino. Assim o saldo de qualquer local e sempre SUM(PsmQntd)
/// filtrado por PsmSheId, sem caso especial.
/// </summary>
[Table("ProductStockMovement", Schema = "dbo")]
public partial class ProductStockMovement
{
    [Key]
    public int PsmId { get; set; }

    [Required(ErrorMessage = "A seleção do produto é obrigatória.")]
    public int PsmProId { get; set; }

    public int? PsmSheId { get; set; }

    [Required(ErrorMessage = "O tipo da movimentação é obrigatório.")]
    public TipoMovimentacao PsmType { get; set; }

    [Required(ErrorMessage = "A quantidade da movimentação é obrigatória.")]
    public int PsmQntd { get; set; }

    [Required(ErrorMessage = "A data da movimentação é obrigatória.")]
    public DateTime PsmDate { get; set; }

    public long? PsmUseId { get; set; }

    [StringLength(300, ErrorMessage = "Insira no máximo 300 caracteres.")]
    public string PsmReason { get; set; }

    [JsonIgnore]
    public virtual Product Product { get; set; }

    [JsonIgnore]
    public virtual Shelf Shelf { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }
}
