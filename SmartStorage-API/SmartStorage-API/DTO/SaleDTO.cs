namespace SmartStorage_API.DTO
{
    public class SaleDTO
    {
        public string? productName { get; set; }
        public string? shelfName { get; set; }
        public int? saleQntd { get; set; }
        public DateTime? saleData { get; set; }
        public decimal? enterPrice { get; set; }
    }
}
