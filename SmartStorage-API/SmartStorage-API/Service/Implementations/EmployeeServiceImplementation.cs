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
            var searchEmployee = _context.Employees.FirstOrDefault(x => x.Cpf == employee.Cpf);

            if (searchEmployee == null)
            {
                var newEmployee = new Employee
                {
                    Name = employee.Name,
                    Cpf = employee.Cpf,
                    Rg = employee.Rg,
                    DateRegister = DateTime.UtcNow,
                };

                _context.Add(newEmployee);
                _context.SaveChanges();

                return newEmployee;
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}
