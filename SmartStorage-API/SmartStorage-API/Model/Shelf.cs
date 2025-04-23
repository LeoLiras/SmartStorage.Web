using System;
using System.Collections.Generic;

namespace SmartStorage_API.Model;

public partial class Shelf
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateTime DataRegister { get; set; }

    public virtual ICollection<Enter> Enters { get; set; } = new List<Enter>();
}
