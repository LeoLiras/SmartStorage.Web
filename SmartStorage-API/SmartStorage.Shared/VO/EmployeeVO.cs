using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class EmployeeVO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do colaborador é obrigatório.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O RG do colaborador é obrigatório.")]
        [StringLength(15)]
        public string? Rg { get; set; }

        [Required(ErrorMessage = "O CPF do colaborador é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF com formato incorreto.")]
        public string? Cpf { get; set; }

        public DateTime DateRegister { get; set; }

        public static Employee? Parse(EmployeeVO origin)
        {
            if (origin == null)
                return null;

            return new Employee
            {
                EmpId = origin.Id,
                EmpName = origin.Name,
                EmpCpf = origin.Cpf,
                EmpRg = origin.Rg,
                EmpDateRegister = origin.DateRegister
            };
        }

        public static List<Employee?>? ParseList(List<EmployeeVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
