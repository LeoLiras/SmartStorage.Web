using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.Shared.Validation;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.Service;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Controllers
{
    [ApiVersion($"{Utils.apiVersion}")]
    [Route("api/storage/[controller]/v{version:apiVersion}")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        #region Propriedades

        private IEmployeeBusiness _employeeService;

        #endregion

        #region Construtores

        public EmployeesController(IEmployeeBusiness employeeService)
        {
            _employeeService = employeeService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindAllEmployees()
        {
            return Ok(_employeeService.FindAllEmployees());
        }

        [HttpGet("{employeeId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult FindEmployeeById(int employeeId)
        {
            if (employeeId.Equals(0))
                throw new BusinessException("O campo ID do Colaborador é obrigatório.");

            return Ok(_employeeService.FindEmployeeById(employeeId));
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewEmployee(EmployeeVO employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name))
                throw new BusinessException("O campo Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Rg))
                throw new BusinessException("O campo RG é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Cpf))
                throw new BusinessException("O campo CPF é obrigatório.");

            return Ok(_employeeService.CreateNewEmployee(employee));
        }

        [HttpPut("{employeeId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateEmployee(int employeeId, [FromBody] EmployeeVO employee)
        {
            if (employeeId.Equals(0))
                throw new BusinessException("O campo ID do Funcionário é obrigatório.");

            return Ok(_employeeService.UpdateEmployee(employeeId, employee));
        }

        [HttpDelete("{employeeId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult DeleteEmployee(int employeeId)
        {
            if (employeeId.Equals(0))
                throw new BusinessException("O campo ID do Colaborador é obrigatório.");

            return Ok(_employeeService.DeleteEmployee(employeeId));
        }

        #endregion
    }
}
