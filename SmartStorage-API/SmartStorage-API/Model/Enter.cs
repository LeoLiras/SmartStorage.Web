using System;
using System.Collections.Generic;

namespace SmartStorage_API.Model;

public partial class Enter
{
    public int Id { get; set; }

    public int IdProduct { get; set; }

    public int IdShelf { get; set; }

    public DateTime DateEnter { get; set; }

    public int? ProductId { get; set; }

    public int? ShelfId { get; set; }

    public int Qntd { get; set; }

    public decimal Price { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual Shelf? Shelf { get; set; }
}
