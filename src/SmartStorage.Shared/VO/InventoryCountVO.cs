using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class InventoryCountVO
    {
        [Required(ErrorMessage = "O motivo do inventário é obrigatório.")]
        [StringLength(180, MinimumLength = 5, ErrorMessage = "Insira entre 5 e 180 caracteres.")]
        public string Reason { get; set; }

        public List<InventoryCountItemVO> Items { get; set; } = new List<InventoryCountItemVO>();
    }
}
