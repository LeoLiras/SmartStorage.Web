using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class InventoryCountItemVO
    {
        [Range(1, int.MaxValue, ErrorMessage = "A entrada da prateleira é obrigatória.")]
        public int EnterId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade contada não pode ser negativa.")]
        public int CountedQntd { get; set; }
    }
}
