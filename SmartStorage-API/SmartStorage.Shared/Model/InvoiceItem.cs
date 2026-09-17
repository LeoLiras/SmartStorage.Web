using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartStorage_Shared.Model;

[Table("InvoiceItem", Schema = "dbo")]
public partial class InvoiceItem
{
    [Key]
    public int IniId { get; set; }

    public int IniInvId { get; set; }

    public int IniNumero { get; set; }

    public int IniProId { get; set; }

    [StringLength(14)]
    public string IniCodigo { get; set; }

    [Required]
    [StringLength(120)]
    public string IniDescricao { get; set; }

    [Required]
    [StringLength(6)]
    public string IniUnidade { get; set; }

    public decimal IniQntdNota { get; set; }

    public decimal IniValorTotal { get; set; }

    public int IniFatorConversao { get; set; }

    public int IniQntdEstoque { get; set; }

    public decimal IniCustoUnitario { get; set; }

    [JsonIgnore]
    public virtual Invoice Invoice { get; set; }

    [JsonIgnore]
    public virtual Product Product { get; set; }
}
