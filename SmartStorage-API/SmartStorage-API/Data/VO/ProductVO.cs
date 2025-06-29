using SmartStorage_API.Model;
using System.Text.Json.Serialization;

namespace SmartStorage_API.Data.VO
{
    public partial class ProductVO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Descricao { get; set; }

        public DateTime DateRegister { get; set; }

        public int Qntd { get; set; }

        public int? EmployeeId { get; set; }
    }
}
