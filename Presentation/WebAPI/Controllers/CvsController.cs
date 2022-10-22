using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CvsController : ControllerBase
    {
        private readonly ICVService _cvService;

        public CvsController(ICVService cvService)
        {
            _cvService = cvService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _cvService.GetAll();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public IActionResult Add(Cv cv)
        {
            var result = _cvService.Add(cv);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(Cv cv)
        {
            var result = _cvService.Update(cv);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public IActionResult Delete(string id)
        {
            var result = _cvService.Delete(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyjobseekerid/{id}")]
        public IActionResult GetById(string id)
        {
            var result = _cvService.GetByJobSeekerId(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}