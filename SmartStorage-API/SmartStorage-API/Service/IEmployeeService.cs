using SmartStorage_API.DTO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Service
{
    public interface IEmployeeService
    {
        List<Employee> FindAllEmployees();
        Employee FindEmployeeById(int employeeId);
        Employee RegisterNewEmployee(EmployeeDTO employee);
        Employee UpdateEmployee(int employeeId, EmployeeDTO employee);
        Employee DeleteEmployee(int employeeId);    
    }
}
