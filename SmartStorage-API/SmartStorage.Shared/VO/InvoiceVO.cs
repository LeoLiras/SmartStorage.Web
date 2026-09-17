namespace SmartStorage_Shared.VO
{
    public partial class InvoiceVO
    {
        public int Id { get; set; }

        public string Chave { get; set; }

        public string Numero { get; set; }

        public string Serie { get; set; }

        public string EmitenteNome { get; set; }

        public DateTime DataImportacao { get; set; }

        public int ItemsCount { get; set; }

        public int CreatedProducts { get; set; }
    }
}
