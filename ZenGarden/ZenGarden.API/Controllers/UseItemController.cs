using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;

namespace ZenGarden.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UseItemController(IUseItemService useItemService) : ControllerBase
    {
        [HttpPost("use")]
        public async Task<IActionResult> UseItem(int userId, int itemId, int usertreeId)
        {


            var result = await useItemService.UseItemAsync(userId, itemId, usertreeId);

            if (result.Contains("thành công"))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }


    }
}
