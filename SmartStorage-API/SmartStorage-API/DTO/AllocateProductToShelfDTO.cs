namespace SmartStorage_API.DTO
{
    public class AllocateProductToShelfDTO
    {
        public int ProductId { get; set; }
        public int ShelfId { get; set; }
        public int ProductQuantity { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
