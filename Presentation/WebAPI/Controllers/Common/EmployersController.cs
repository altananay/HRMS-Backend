using Application.Abstractions.Common;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployersController : ControllerBase
    {
        private readonly IEmployerService _employerService;

        public EmployersController(IEmployerService employerService)
        {
            _employerService = employerService;
        }

        [HttpPost("addemployer")]
        public IActionResult Add(EmployerForRegisterDto employer)
        {
            var exists = _employerService.EmployerExists(employer.Email);
            if (!exists.IsSuccess)
            {
                return BadRequest(exists.Message);
            }

            var result = _employerService.Add(employer);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _employerService.GetAll();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(string id)
        {
            var result = _employerService.Delete(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(EmployerForUpdateDto employer)
        {
            var result = _employerService.Update(employer);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("{id}")]
        public IActionResult GetById(string id)
        {
            var result = _employerService.GetById(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyemail")]
        public IActionResult GetByEmail(string email)
        {
            var result = _employerService.EmployerExists(email);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}