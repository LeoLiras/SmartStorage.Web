namespace SmartStorage_API.Model;

public partial class Sale
{
    public int Id { get; set; }

    public int IdEnter { get; set; }

    public int? EnterId { get; set; }

    public int Qntd { get; set; }

    public DateTime DateSale { get; set; }

    public virtual Enter? Enter { get; set; }
}
