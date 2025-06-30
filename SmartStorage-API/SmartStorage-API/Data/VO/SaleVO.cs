namespace SmartStorage_API.Data.VO
{
    public partial class SaleVO
    {
        public string? ProductName { get; set; }

        public int ProductId { get; set; }

        public string? ShelfName { get; set; }

        public decimal EnterPrice { get; set; }

        public decimal SaleTotal { get; set; }

        public int Id { get; set; }

        public int IdEnter { get; set; }

        public int Qntd { get; set; }

        public DateTime DateSale { get; set; }
    }
}
