using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Model;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Descricao { get; set; }

    public DateTime DateRegister { get; set; }

    public int Qntd { get; set; }

    public int? EmployeeId { get; set; }

    [JsonIgnore]
    public int EmployeeId1 { get; set; }

    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [JsonIgnore]
    public virtual ICollection<Enter> Enters { get; set; } = new List<Enter>();
}
