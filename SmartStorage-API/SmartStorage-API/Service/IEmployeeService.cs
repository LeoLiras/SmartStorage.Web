using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IEmployeeService
    {
        List<Employee> FindAllEmployees();
        Employee RegisterNewEmployee(EmployeeDTO employee);
        Employee UpdateEmployee(EmployeeDTO employee);
    }
}
