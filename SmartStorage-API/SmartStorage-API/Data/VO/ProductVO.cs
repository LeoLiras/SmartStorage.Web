using SmartStorage_API.Hypermedia;
using SmartStorage_API.Hypermedia.Abstract;

namespace SmartStorage_API.Data.VO
{
    public partial class ProductVO : ISupportHyperMedia
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Descricao { get; set; }

        public DateTime DateRegister { get; set; }

        public int Qntd { get; set; }

        public int? EmployeeId { get; set; }

        public byte[]? ProImage { get; set; }

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();
    }
}
