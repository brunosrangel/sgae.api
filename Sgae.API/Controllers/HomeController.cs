using Microsoft.AspNetCore.Mvc;

namespace Sgae.API.Controllers;

[ApiController]
[Route("")]
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect("/swagger/index.html");
    }

    [HttpGet("api")]
    public IActionResult ApiRoot()
    {
        return Ok(new
        {
            service = "SGAE API - Sistema de Gestão de Atendimento Especializado",
            version = "v1",
            status = "Operational",
            documentation = "/swagger/index.html",
            health = "/health"
        });
    }
}
