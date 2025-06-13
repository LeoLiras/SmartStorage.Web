using SmartStorage_API.DTO;
using SmartStorage_API.Model;
using SmartStorage_API.Model.Context;

namespace SmartStorage_API.Service.Implementations
{
    
    public class EmployeeServiceImplementation : IEmployeeService
    {
        #region Propriedades

        private readonly SmartStorageContext _context;

        #endregion

        #region Construtores

        public EmployeeServiceImplementation(SmartStorageContext context)
        {
            _context = context;
        }

        #endregion

        #region Métodos
        public List<Employee> FindAllEmployees()
        {
            return _context.Employees.OrderBy(x => x.Name).ToList();
        }

        public Employee RegisterNewEmployee(EmployeeDTO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.Cpf == employee.employeeCpf);

            if (searchEmployee != null)
                throw new Exception("Funcionário já registrado com o CPF informado.");
            
            var newEmployee = new Employee
            {
                Name = employee.employeeName,
                Cpf = employee.employeeCpf,
                Rg = employee.employeeRg,
                DateRegister = DateTime.UtcNow,
            };

            _context.Add(newEmployee);
            _context.SaveChanges();

            return newEmployee;
        }

        public Employee UpdateEmployee(int employeeId, EmployeeDTO employee)
        {
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.Id == employeeId);

            if (searchEmployee == null)
                throw new Exception("Colaborador com ID informado não encontrado na base de dados.");

            if (!string.IsNullOrWhiteSpace(employee.employeeCpf))
            {
                var searchEmployeeCpf = _context.Employees.FirstOrDefault(x => x.Cpf == employee.employeeCpf && x.Id != employeeId);

                if (searchEmployeeCpf != null)
                    throw new Exception("Colaborador já registrado com o CPF informado.");

                searchEmployee.Cpf = employee.employeeCpf;
            }
                
            if (!string.IsNullOrWhiteSpace(employee.employeeName))
                searchEmployee.Name = employee.employeeName;

            if (!string.IsNullOrWhiteSpace(employee.employeeRg))
                searchEmployee.Rg = employee.employeeRg;

            

            

            _context.SaveChanges();

            return searchEmployee;
        }

        public Employee DeleteEmployee(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == employeeId);

            if (employee is null)
                throw new Exception("Colaborador não encontrado com o Id informado");

            var products = _context.Products.Where(p => p.EmployeeId == employeeId).ToList();

            if(products.Count() > 0)
            {
                foreach (var product in products)
                {
                    product.EmployeeId = null;
                }
            }
            
            _context.Remove(employee);

            _context.SaveChanges();

            return employee;
        }
        #endregion

    }
}
