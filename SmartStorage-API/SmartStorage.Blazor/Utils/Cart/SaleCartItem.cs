namespace SmartStorage.Blazor.Utils.Cart
{
    public class SaleCartItem
    {
        public int EnterId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string ShelfName { get; set; }

        public decimal Price { get; set; }

        public int Available { get; set; }

        public int Qntd { get; set; }

        public decimal Subtotal => Price * Qntd;

        public bool ExceedsAvailable => Qntd > Available;
    }
}
