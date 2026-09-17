using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_Shared.Model;

[Table("Invoice", Schema = "dbo")]
public partial class Invoice
{
    [Key]
    public int InvId { get; set; }

    [Required]
    [StringLength(44, MinimumLength = 44)]
    public string InvChave { get; set; }

    [Required]
    [StringLength(9)]
    public string InvNumero { get; set; }

    [Required]
    [StringLength(3)]
    public string InvSerie { get; set; }

    [StringLength(14)]
    public string InvEmitenteCnpj { get; set; }

    [Required]
    [StringLength(60)]
    public string InvEmitenteNome { get; set; }

    public DateTime InvDataEmissao { get; set; }

    public DateTime InvDataImportacao { get; set; }

    public long InvUseId { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; }

    [JsonIgnore]
    public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
