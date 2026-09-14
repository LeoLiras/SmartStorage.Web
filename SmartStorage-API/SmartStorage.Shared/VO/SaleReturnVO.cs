using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class SaleReturnVO
    {
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade a devolver deve ser maior que zero.")]
        public int Quantity { get; set; }
    }
}
