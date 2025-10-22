using SmartStorage_Shared.Model;

namespace SmartStorage_Shared.VO
{
    public partial class EmployeeVO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Rg { get; set; }

        public string? Cpf { get; set; }

        public DateTime DateRegister { get; set; }

        public static Employee Parse(EmployeeVO origin)
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

        public static List<Employee> Parse(List<EmployeeVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
