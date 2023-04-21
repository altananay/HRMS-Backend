using Application.Abstractions;
using Application.Features.Employers.Commands;
using Application.Features.Employers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Application.Features.Employers.Commands.DeleteEmployerCommand;
using static Application.Features.Employers.Commands.UpdateEmployerCommand;
using static Application.Features.Employers.Queries.GetAllEmployerOrderByNumberOfEmployeesQuery;
using static Application.Features.Employers.Queries.GetAllEmployerQuery;
using static Application.Features.Employers.Queries.GetByEmailEmployerQuery;
using static Application.Features.Employers.Queries.GetByIdEmployerQuery;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployersController : ControllerBase
    {

        private readonly IMediator _mediator;

        public EmployersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            GetAllEmployerQueryResponse response = await _mediator.Send(new GetAllEmployerQuery { });
            if (response.Employers.IsSuccess)
            {
                return Ok(response.Employers);
            }
            return BadRequest(response.Employers);
        }

        [HttpGet("getallorderbynumberofemployees")]
        public async Task<IActionResult> GetAllByHighestNumberOfEmployees()
        {
            GetAllEmployerOrderByNumberOfEmployeesQueryResponse response = await _mediator.Send(new GetAllEmployerOrderByNumberOfEmployeesQuery { });
            if (response.Employers.IsSuccess)
            {
                return Ok(response.Employers);
            }
            return BadRequest(response.Employers);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            DeleteEmployerCommandResponse response = await _mediator.Send(new DeleteEmployerCommand { Id = id});
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateEmployerCommand employer)
        {
            UpdateEmployerCommandResponse response = await _mediator.Send(employer);
            if (response.Result.IsSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result);
        }

        [HttpGet("getbyemployerid/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            GetByIdEmployerQueryResponse response = await _mediator.Send(new GetByIdEmployerQuery { Id = id});
            if (response.Employer.IsSuccess)
            {
                return Ok(response.Employer);
            }
            return BadRequest(response.Employer);
        }

        [HttpPost("getbyemail")]
        public async Task<IActionResult> GetByEmail(GetByEmailEmployerQuery email)
        {
            GetByEmailEmployerQueryResponse response = await _mediator.Send(email);
            if (response.Employer.IsSuccess)
            {
                return Ok(response.Employer);
            }
            return BadRequest(response.Employer);
        }
    }
}