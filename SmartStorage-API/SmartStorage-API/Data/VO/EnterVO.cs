using SmartStorage_API.Hypermedia;
using SmartStorage_API.Hypermedia.Abstract;

namespace SmartStorage_API.Data.VO
{
    public partial class EnterVO : ISupportHyperMedia
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public int ProductQuantity { get; set; }

        public decimal ProductPrice { get; set; }

        public int ShelfId { get; set; }

        public string? ShelfName { get; set; }

        public DateTime DateEnter { get; set; }

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();
    }
}
