using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class EmployeeConverter : IParser<EmployeeVO, Employee>, IParser<Employee, EmployeeVO>
    {
        public Employee Parse(EmployeeVO origin)
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

        public EmployeeVO Parse(Employee origin)
        {
            if (origin == null)
                return null;

            return new EmployeeVO
            {
                Id = origin.EmpId,
                Name = origin.EmpName,
                Cpf = origin.EmpCpf,
                Rg = origin.EmpRg,
                DateRegister = origin.EmpDateRegister
            };
        }

        public List<Employee> Parse(List<EmployeeVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }

        public List<EmployeeVO> Parse(List<Employee> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
