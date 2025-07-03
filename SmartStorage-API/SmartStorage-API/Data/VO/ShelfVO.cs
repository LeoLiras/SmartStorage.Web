using SmartStorage_API.Hypermedia;
using SmartStorage_API.Hypermedia.Abstract;

namespace SmartStorage_API.Data.VO
{
    public partial class ShelfVO : ISupportHyperMedia
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public DateTime DataRegister { get; set; }

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();
    }
}
