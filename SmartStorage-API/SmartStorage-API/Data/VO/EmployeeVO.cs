namespace SmartStorage_API.Data.VO;

public partial class EmployeeVO
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Rg { get; set; }

    public string? Cpf { get; set; }

    public DateTime DateRegister { get; set; }
}
