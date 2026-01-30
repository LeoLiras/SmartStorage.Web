using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Hypermedia.Filters;
using SmartStorage_API.Service;

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
            try
            {
                if (employeeId.Equals(0))
                    throw new Exception("O campo ID do Colaborador é obrigatório.");

                return Ok(_employeeService.FindEmployeeById(employeeId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult CreateNewEmployee(EmployeeVO employee)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employee.Name))
                    throw new Exception("O campo Nome é obrigatório.");

                if (string.IsNullOrWhiteSpace(employee.Rg))
                    throw new Exception("O campo RG é obrigatório.");

                if (string.IsNullOrWhiteSpace(employee.Cpf))
                    throw new Exception("O campo CPF é obrigatório.");

                return Ok(_employeeService.CreateNewEmployee(employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{employeeId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult UpdateEmployee(int employeeId, [FromBody] EmployeeVO employee)
        {
            try
            {
                if (employeeId.Equals(0))
                    throw new Exception("O campo ID do Funcionário é obrigatório.");

                return Ok(_employeeService.UpdateEmployee(employeeId, employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{employeeId}")]
        [TypeFilter(typeof(HyperMediaFilter))]
        public IActionResult DeleteEmployee(int employeeId)
        {
            try
            {
                if (employeeId.Equals(0))
                    throw new Exception("O campo ID do Colaborador é obrigatório.");

                return Ok(_employeeService.DeleteEmployee(employeeId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion
    }
}
