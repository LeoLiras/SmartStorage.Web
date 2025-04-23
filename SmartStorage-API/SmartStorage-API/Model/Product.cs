using System;
using System.Collections.Generic;

namespace SmartStorage_API.Model;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Descricao { get; set; }

    public DateTime DateRegister { get; set; }

    public int Qntd { get; set; }

    public int? EmployeeId { get; set; }

    public int EmployeeId1 { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Enter> Enters { get; set; } = new List<Enter>();
}
