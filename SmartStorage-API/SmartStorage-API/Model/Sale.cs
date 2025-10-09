using System.ComponentModel.DataAnnotations;

namespace SmartStorage_API.Model;

public partial class Sale
{
    [Key]
    public int SalId { get; set; }

    [Required(ErrorMessage = "O ID da entrada relativa a venda é obrigatório.")]
    public int SalEntId { get; set; }

    [Required(ErrorMessage = "A quantidade da venda é obrigatória.")]
    public int SalQntd { get; set; }

    [Required(ErrorMessage = "A data da venda é obrigatória.")]
    public DateTime SalDateSale { get; set; }

    public virtual Enter? Enter { get; set; }
}
