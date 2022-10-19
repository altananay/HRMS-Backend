using Application.Abstractions;
using Application.Dtos;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPositionController : ControllerBase
    {
        private readonly IJobPositionService _jobPositionService;

        public JobPositionController(IJobPositionService jobPositionService)
        {
            _jobPositionService = jobPositionService;
        }

        [HttpPost("addjobposition")]
        public IActionResult Add(JobPositionDto jobPosition)
        {
            var jobPositionExists = _jobPositionService.JobPositionExists(jobPosition.PositionName);
            if (!jobPositionExists.IsSuccess)
            {
                return BadRequest(jobPositionExists.Message);
            }

            var position = new JobPosition
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                PositionName = jobPosition.PositionName
            };
            var result = _jobPositionService.Add(position);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _jobPositionService.GetAll();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(string id)
        {
            var result = _jobPositionService.Delete(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(JobPosition jobPosition)
        {
            var result = _jobPositionService.GetById(jobPosition.Id);
            jobPosition.CreatedAt = result.Data.CreatedAt;
            jobPosition.UpdatedAt = DateTime.UtcNow;
            var result2 = _jobPositionService.Update(jobPosition);
            if (result2.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("{id}")]
        public IActionResult GetById(string id)
        {
            var result = _jobPositionService.GetById(id);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}