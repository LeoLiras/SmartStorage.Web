namespace SmartStorage_Shared.VO
{
    public partial class InvoicePreviewVO
    {
        public string Chave { get; set; }

        public string Numero { get; set; }

        public string Serie { get; set; }

        public string EmitenteCnpj { get; set; }

        public string EmitenteNome { get; set; }

        public DateTime DataEmissao { get; set; }

        public DateTime? ImportadaEm { get; set; }

        public List<InvoicePreviewItemVO> Items { get; set; } = new List<InvoicePreviewItemVO>();
    }
}
