using SmartStorage_API.Data.Converter.Implementations;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{

    public class EmployeeBusinessImplementation : IEmployeeBusiness
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        private readonly EmployeeConverter _converter;

        #endregion

        #region Construtores

        public EmployeeBusinessImplementation(SmartStorageContext context)
        {
            _context = context;
            _converter = new EmployeeConverter();
        }

        #endregion

        #region Métodos
        public List<EmployeeVO> FindAllEmployees()
        {
            return _converter.Parse(_context.Employees.OrderBy(x => x.EmpName).ToList());
        }

        public EmployeeVO FindEmployeeById(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmpId.Equals(employeeId));

            if (employee is null)
                throw new Exception("Funcionário não encontrado com o ID informado");

            return _converter.Parse(employee);
        }

        public EmployeeVO CreateNewEmployee(EmployeeVO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.EmpCpf == employee.Cpf);

            if (searchEmployee != null)
                throw new Exception("Funcionário já registrado com o CPF informado.");

            var newEmployee = new Employee
            {
                EmpName = employee.Name,
                EmpCpf = employee.Cpf,
                EmpRg = employee.Rg,
                EmpDateRegister = DateTime.UtcNow,
            };

            _context.Add(newEmployee);
            _context.SaveChanges();

            return _converter.Parse(newEmployee);
        }

        public EmployeeVO UpdateEmployee(int employeeId, EmployeeVO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.EmpId == employeeId);

            if (searchEmployee == null)
                throw new Exception("Funcionário com ID informado não encontrado na base de dados.");

            if (!string.IsNullOrWhiteSpace(employee.Cpf))
            {
                var searchEmployeeCpf = _context.Employees.FirstOrDefault(x => x.EmpCpf == employee.Cpf && x.EmpId != employeeId);

                if (searchEmployeeCpf != null)
                    throw new Exception("Funcionário já registrado com o CPF informado.");

                searchEmployee.EmpCpf = employee.Cpf;
            }

            if (!string.IsNullOrWhiteSpace(employee.Name))
                searchEmployee.EmpName = employee.Name;

            if (!string.IsNullOrWhiteSpace(employee.Rg))
                searchEmployee.EmpRg = employee.Rg;

            _context.SaveChanges();

            return _converter.Parse(searchEmployee);
        }

        public EmployeeVO DeleteEmployee(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmpId == employeeId);

            if (employee is null)
                throw new Exception("Funcionário não encontrado com o Id informado");

            var products = _context.Products.Where(p => p.ProEmpId == employeeId).ToList();

            if (products.Count() > 0)
            {
                foreach (var product in products)
                {
                    product.ProEmpId = null;
                }
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();

            return _converter.Parse(employee);
        }

        #endregion

    }
}
