using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        #region Propriedades

        private IEmployeeService _employeeService;

        #endregion

        #region Construtores

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        #endregion

        #region Métodos

        [HttpGet]
        public IActionResult FindAllEmployees()
        {
            return Ok(_employeeService.FindAllEmployees());
        }

        [HttpPost]
        public IActionResult RegisterNewEmployee(EmployeeDTO employee)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employee.employeeName))
                    throw new Exception("O campo Nome é obrigatório.");

                if (string.IsNullOrWhiteSpace(employee.employeeRg))
                    throw new Exception("O campo RG é obrigatório.");

                if (string.IsNullOrWhiteSpace(employee.employeeCpf))
                    throw new Exception("O campo CPF é obrigatório.");

                return Ok(_employeeService.RegisterNewEmployee(employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{employeeId}")]
        public IActionResult UpdateEmployee(int employeeId, [FromBody] EmployeeDTO employee)
        {
            try
            {
                if (employeeId.Equals(0)) 
                    throw new Exception("O campo ID do Colaborador é obrigatório.");

                return Ok(_employeeService.UpdateEmployee(employeeId, employee));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        #endregion
    }
}
