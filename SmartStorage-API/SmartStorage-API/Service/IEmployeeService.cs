using SmartStorage_API.Data.VO;

namespace SmartStorage_API.Service
{
    public interface IEmployeeService
    {
        List<EmployeeVO> FindAllEmployees();
        EmployeeVO FindEmployeeById(int employeeId);
        EmployeeVO CreateNewEmployee(EmployeeVO employee);
        EmployeeVO UpdateEmployee(int employeeId, EmployeeVO employee);
        EmployeeVO DeleteEmployee(int employeeId);
    }
}
