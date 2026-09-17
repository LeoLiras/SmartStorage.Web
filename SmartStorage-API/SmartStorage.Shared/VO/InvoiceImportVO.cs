using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class InvoiceImportVO
    {
        [Required(ErrorMessage = "O XML da nota fiscal é obrigatório.")]
        public string Xml { get; set; }

        public List<InvoiceImportItemVO> Items { get; set; } = new List<InvoiceImportItemVO>();
    }
}
