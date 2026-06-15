#pragma warning disable S6967
using Microsoft.AspNetCore.Mvc;

namespace DemoApp.Web.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Error");
        }

        [HttpGet("{statusCode}")]
        public IActionResult HandleError(int statusCode)
        {
            Response.StatusCode = statusCode;

            return statusCode switch
            {
                404 => View("NotFound"),
                403 => View("AccessDenied"),
                401 => View("InvalidToken"),
                500 => View("InternalServerError"),
                _ => View("Error")
            };
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [HttpGet("InvalidToken")]
        public IActionResult InvalidToken()
        {
            return View("InvalidToken");
        }
    }
}
