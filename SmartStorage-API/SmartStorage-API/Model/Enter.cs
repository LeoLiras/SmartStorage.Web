using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Model;

public partial class Enter
{
    public int Id { get; set; }

    public int IdProduct { get; set; }

    public int IdShelf { get; set; }

    public DateTime DateEnter { get; set; }

    [JsonIgnore]
    public int? ProductId { get; set; }

    [JsonIgnore]
    public int? ShelfId { get; set; }

    public int Qntd { get; set; }

    public decimal Price { get; set; }

    [JsonIgnore]
    public virtual Product? Product { get; set; }

    [JsonIgnore]
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    [JsonIgnore]
    public virtual Shelf? Shelf { get; set; }
}
