using Microsoft.AspNetCore.Mvc;

namespace ZenGarden.API.Controllers;

public class TaskController : ControllerBase
{
    // GET
    public IActionResult Index()
    {
        return Ok();
    }
}