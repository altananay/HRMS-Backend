using Application.Abstractions.Common;
using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSeekersController : ControllerBase
    {
        IJobSeekerService _jobSeekerService;

        public JobSeekersController(IJobSeekerService userService)
        {
            _jobSeekerService = userService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _jobSeekerService.GetAll();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyemail")]
        public IActionResult GetByEmail(string email)
        {
            var result = _jobSeekerService.GetByMail(email);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(string id)
        {
            var result = _jobSeekerService.Delete(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(JobSeeker user)
        {
            var result = _jobSeekerService.Update(user);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}