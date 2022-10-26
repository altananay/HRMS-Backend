using Application.Abstractions;
using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSeekersController : ControllerBase
    {
        IJobSeekerService _jobSeekerService;
        IAuthService _authService;

        public JobSeekersController(IJobSeekerService userService, IAuthService authService)
        {
            _jobSeekerService = userService;
            _authService = authService;
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

        [HttpPost("add")]
        public IActionResult Add(JobSeekerForRegisterDto jobSeeker)
        {
            var result = _jobSeekerService.NationalityIdExists(jobSeeker.IdentityNumber);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            var result2 = _authService.Register(jobSeeker, jobSeeker.Password);
            if (result2.IsSuccess)
            {
                return Ok(result2);
            }
            return BadRequest(result2);
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