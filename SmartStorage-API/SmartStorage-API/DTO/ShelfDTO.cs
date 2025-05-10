namespace SmartStorage_API.DTO
{
    public class ShelfDTO
    {
        public string? productName { get; set; }
        public int? productId { get; set; }
        public string? shelfName { get; set; }
        public int? qntd { get; set; }
        public DateTime? allocateData { get; set; }
        public decimal? price { get; set; }
    }
}
