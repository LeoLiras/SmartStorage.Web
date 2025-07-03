using SmartStorage_API.Hypermedia;
using SmartStorage_API.Hypermedia.Abstract;

namespace SmartStorage_API.Data.VO
{
    public partial class SaleVO : ISupportHyperMedia
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

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();
    }
}
