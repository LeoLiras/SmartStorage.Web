using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class AllocationBatchItemVO
    {
        [Range(1, int.MaxValue, ErrorMessage = "A seleção do produto é obrigatória.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A seleção da prateleira é obrigatória.")]
        public int ShelfId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Qntd { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        public decimal Price { get; set; }
    }
}
