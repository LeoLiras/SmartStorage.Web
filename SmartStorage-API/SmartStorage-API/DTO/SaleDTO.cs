namespace SmartStorage_API.DTO
{
    public class SaleDTO
    {
        public string? productName { get; set; }
        public int productId { get; set; }
        public string? shelfName { get; set; }
        public int saleQntd { get; set; }
        public int? enterId {  get; set; }
        public DateTime? saleData { get; set; }
        public decimal? enterPrice { get; set; }
        public decimal? total { get; set; }
    }
}
