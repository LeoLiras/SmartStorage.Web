using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class StockAdjustmentVO
    {
        [Required(ErrorMessage = "A quantidade do ajuste é obrigatória.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "O motivo do ajuste é obrigatório.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Insira entre 5 e 300 caracteres.")]
        public string Reason { get; set; }
    }
}
