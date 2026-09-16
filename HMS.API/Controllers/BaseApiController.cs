using HMS.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult HandleResponse<T>(GenericResponse<T> result)
        => result.StatusCode switch
        {

            StatusCodes.Status200OK => Ok(result),
            StatusCodes.Status400BadRequest => BadRequest(result),
            StatusCodes.Status404NotFound => NotFound(result),
            StatusCodes.Status401Unauthorized => Unauthorized(result),
            StatusCodes.Status500InternalServerError => StatusCode(StatusCodes.Status500InternalServerError),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
