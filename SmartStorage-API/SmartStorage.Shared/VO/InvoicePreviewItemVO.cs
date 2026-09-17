namespace SmartStorage_Shared.VO
{
    public partial class InvoicePreviewItemVO
    {
        public int Numero { get; set; }

        public string CodigoProduto { get; set; }

        public string Codigo { get; set; }

        public string CodigoInvalido { get; set; }

        public string Descricao { get; set; }

        public string Unidade { get; set; }

        public decimal QntdNota { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal ValorTotal { get; set; }

        public int? ProductId { get; set; }

        public string ProductName { get; set; }

        public int FatorConversao { get; set; } = 1;

        public int? QntdEstoque { get; set; }
    }
}
