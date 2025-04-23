using System;
using System.Collections.Generic;

namespace SmartStorage_API.Model;

public partial class Employee
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Cpf { get; set; }

    public string? Rg { get; set; }

    public DateTime DateRegister { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
