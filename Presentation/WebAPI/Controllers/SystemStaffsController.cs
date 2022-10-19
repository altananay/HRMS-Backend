using Application.Abstractions;
using Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemStaffsController : ControllerBase
    {
        private readonly ISystemStaffService _systemStaffService;
        private readonly ISystemStaffAuthService _systemStaffAuthService;

        public SystemStaffsController(ISystemStaffService systemStaffService, ISystemStaffAuthService systemStaffAuthService)
        {
            _systemStaffService = systemStaffService;
            _systemStaffAuthService = systemStaffAuthService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _systemStaffService.GetAll();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public IActionResult Add(SystemStaffForRegisterDto systemStaff)
        {
            var result = _systemStaffAuthService.Register(systemStaff, systemStaff.Password);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(string id)
        {
            var result = _systemStaffService.Delete(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(SystemStaffForRegisterDto systemStaff)
        {
            var result = _systemStaffService.Update(systemStaff);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("{id}")]
        public IActionResult GetById(string id)
        {
            var result = _systemStaffService.GetById(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}