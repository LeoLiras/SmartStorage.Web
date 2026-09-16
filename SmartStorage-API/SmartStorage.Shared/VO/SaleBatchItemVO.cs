using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class SaleBatchItemVO
    {
        [Range(1, int.MaxValue, ErrorMessage = "O ID da entrada relativa a venda é obrigatório.")]
        public int IdEnter { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade da venda deve ser maior que zero.")]
        public int Qntd { get; set; }
    }
}
