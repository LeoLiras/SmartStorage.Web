using SmartStorage_API.Hypermedia;
using SmartStorage_API.Hypermedia.Abstract;

namespace SmartStorage_API.Data.VO;

public partial class EmployeeVO : ISupportHyperMedia
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Rg { get; set; }

    public string? Cpf { get; set; }

    public DateTime DateRegister { get; set; }
    public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();
}
