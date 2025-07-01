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
            return _converter.Parse(_context.Employees.OrderBy(x => x.Name).ToList());
        }

        public EmployeeVO FindEmployeeById(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id.Equals(employeeId));

            if (employee is null)
                throw new Exception("Funcionário não encontrado com o ID informado");

            return _converter.Parse(employee);
        }

        public EmployeeVO CreateNewEmployee(EmployeeVO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.Cpf == employee.Cpf);

            if (searchEmployee != null)
                throw new Exception("Funcionário já registrado com o CPF informado.");

            var newEmployee = new Employee
            {
                Name = employee.Name,
                Cpf = employee.Cpf,
                Rg = employee.Rg,
                DateRegister = DateTime.UtcNow,
            };

            _context.Add(newEmployee);
            _context.SaveChanges();

            return _converter.Parse(newEmployee);
        }

        public EmployeeVO UpdateEmployee(int employeeId, EmployeeVO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.Id == employeeId);

            if (searchEmployee == null)
                throw new Exception("Funcionário com ID informado não encontrado na base de dados.");

            if (!string.IsNullOrWhiteSpace(employee.Cpf))
            {
                var searchEmployeeCpf = _context.Employees.FirstOrDefault(x => x.Cpf == employee.Cpf && x.Id != employeeId);

                if (searchEmployeeCpf != null)
                    throw new Exception("Funcionário já registrado com o CPF informado.");

                searchEmployee.Cpf = employee.Cpf;
            }

            if (!string.IsNullOrWhiteSpace(employee.Name))
                searchEmployee.Name = employee.Name;

            if (!string.IsNullOrWhiteSpace(employee.Rg))
                searchEmployee.Rg = employee.Rg;

            _context.SaveChanges();

            return _converter.Parse(searchEmployee);
        }

        public EmployeeVO DeleteEmployee(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee is null)
                throw new Exception("Funcionário não encontrado com o Id informado");

            var products = _context.Products.Where(p => p.EmployeeId == employeeId).ToList();

            if (products.Count() > 0)
            {
                foreach (var product in products)
                {
                    product.EmployeeId = null;
                }
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();

            return _converter.Parse(employee);
        }

        #endregion

    }
}
