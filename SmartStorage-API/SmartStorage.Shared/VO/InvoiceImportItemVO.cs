using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class InvoiceImportItemVO
    {
        public int Numero { get; set; }

        public int? ProductId { get; set; }

        public ProductVO NewProduct { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O fator de conversão deve ser maior que zero.")]
        public int FatorConversao { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade de entrada deve ser maior que zero.")]
        public int QntdEstoque { get; set; }
    }
}
