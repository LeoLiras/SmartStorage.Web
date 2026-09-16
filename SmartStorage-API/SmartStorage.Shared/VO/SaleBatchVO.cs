using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class SaleBatchVO
    {
        [Required(ErrorMessage = "A data da venda é obrigatória.")]
        public DateTime DateSale { get; set; }

        public List<SaleBatchItemVO> Items { get; set; } = new List<SaleBatchItemVO>();
    }
}
