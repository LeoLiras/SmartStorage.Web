using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Model;

public partial class Employee
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Cpf { get; set; }

    public string? Rg { get; set; }

    public DateTime DateRegister { get; set; }

    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
