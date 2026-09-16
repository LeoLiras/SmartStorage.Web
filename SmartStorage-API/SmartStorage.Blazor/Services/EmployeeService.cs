using SmartStorage.Blazor.Services.IServices;
using SmartStorage.Blazor.Utils.API;
using SmartStorage_Shared.VO;

namespace SmartStorage.Blazor.Services
{
    public class EmployeeService : IEmployeeService
    {
        #region Properties

        private readonly HttpClient _client;

        public const string BasePath = "api/storage/employees/v1";

        #endregion

        #region Constructors

        public EmployeeService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        #endregion

        #region Methods

        public async Task<List<EmployeeVO>> GetEmployees()
        {
            var response = await _client.GetAsync(BasePath);

            return await response.ReadApiAsync<List<EmployeeVO>>();
        }

        #endregion
    }
}
