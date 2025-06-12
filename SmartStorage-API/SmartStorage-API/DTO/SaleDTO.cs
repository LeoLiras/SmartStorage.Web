namespace SmartStorage_API.DTO
{
    public class SaleDTO
    {
        public int? saleId { get; set; }
        public string? saleProductName { get; set; }
        public int? saleProductId { get; set; }
        public string? saleShelfName { get; set; }
        public int saleSaleQntd { get; set; }
        public int? saleEnterId {  get; set; }
        public DateTime? saleSaleData { get; set; }
        public decimal? saleEnterPrice { get; set; }
        public decimal? saleTotal { get; set; }
    }
}
