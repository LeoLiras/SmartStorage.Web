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
            if (string.IsNullOrWhiteSpace(employee.Name)) return BadRequest("O campo Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Rg)) return BadRequest("O campo RG é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Cpf)) return BadRequest("O campo CPF é obrigatório.");

            var newEmployee = _employeeService.RegisterNewEmployee(employee);

            if (newEmployee == null) return BadRequest("Funcionário já registrado.");

            return Ok(newEmployee);
        }

        [HttpPut]
        public IActionResult UpdateEmployee(EmployeeDTO employee)
        {
            if (employee.employeeId.Equals(0)) return BadRequest("O campo ID do Colaborador é obrigatório.");

            var updateEmployee = _employeeService.UpdateEmployee(employee);

            if (updateEmployee == null) return BadRequest("O Colaborador com o ID informado não foi encontrado na base de dados.");

            return Ok(updateEmployee);
        }

        #endregion

    }
}
