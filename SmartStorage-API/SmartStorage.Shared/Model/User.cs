using System;
using System.Collections.Generic;

namespace SmartStorage_API.Model;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Cpf { get; set; }

    public string? Rg { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }
}
