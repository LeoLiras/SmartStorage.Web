using Microsoft.AspNetCore.Mvc;
using SmartStorage_API.DTO;
using SmartStorage_API.Service;

namespace SmartStorage_API.Controllers
{
    [Route("api/storage/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private IStorageService _storageService;

        public EmployeesController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet]
        public IActionResult GetEmployees()
        {
            return Ok(_storageService.FindAllEmployees());
        }

        [HttpPost]
        public IActionResult RegisterNewEmployee(EmployeeDTO employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Name)) return BadRequest("O campo Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Rg)) return BadRequest("O campo RG é obrigatório.");

            if (string.IsNullOrWhiteSpace(employee.Cpf)) return BadRequest("O campo CPF é obrigatório.");

            var newEmployee = _storageService.RegisterNewEmployee(employee);

            if (newEmployee == null) return BadRequest("Funcionário já registrado.");

            return Ok(newEmployee);
        }
    }
}
