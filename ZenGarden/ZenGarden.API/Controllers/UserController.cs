using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;

using ZenGarden.Infrastructure.Base;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        IUserService userService;

        [HttpGet]
        public async Task<BusinessResult> GetUsers()
        {
            var users = await userService.GetAllUsersAsync();
            return new BusinessResult(200,"oke",users);
        }
    
    }
}
