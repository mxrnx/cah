using Microsoft.AspNetCore.Mvc;
using Server.Services;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class CzarController : ControllerBase
{
    private readonly MemoryService _memoryService;

    public CzarController(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpGet]
    public ActionResult<Guid> Get() =>
        _memoryService.GetCzar();

}
