using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class ShelfTransferVO
    {
        [Range(1, int.MaxValue, ErrorMessage = "A prateleira de destino é obrigatória.")]
        public int ShelfId { get; set; }
    }
}
