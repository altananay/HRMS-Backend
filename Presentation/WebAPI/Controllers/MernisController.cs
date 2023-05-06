using Application.Abstractions;
using Application.Constants;
using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MernisController : ControllerBase
    {
        private readonly ICheckPersonService _checkRealPersonService;

        public MernisController(ICheckPersonService checkRealPersonService)
        {
            _checkRealPersonService = checkRealPersonService;
        }

        [HttpPost("checkperson")]
        public IActionResult CheckPerson(MernisCheckDto checkUser)
        {
            var result = _checkRealPersonService.CheckIfRealPerson(checkUser);
            if (result)
            {
                return Ok(Messages.CitizenSuccessful);
            }
            return BadRequest(Messages.CitizenError);  
        }
    }
}