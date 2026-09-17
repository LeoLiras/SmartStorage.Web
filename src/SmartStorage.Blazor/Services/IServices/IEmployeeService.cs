using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services.IServices
{
    public interface IEmployeeService
    {
        Task<List<EmployeeVO>> GetEmployees();
    }
}
