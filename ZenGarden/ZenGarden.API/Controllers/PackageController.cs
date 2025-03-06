using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController(IPackagesService packagesService) : ControllerBase
    {
        private readonly IPackagesService _PackageService = packagesService ?? throw new ArgumentNullException(nameof(packagesService));

        [HttpGet]
        public async Task<IActionResult> GetPackages()
        {
            var Packages = await _PackageService.GetAllPackagesAsync();
            return Ok(Packages);
        }

        [HttpGet("{PackageId}")]
        public async Task<IActionResult> GetPackageById(int PackageId)
        {
            var Package = await _PackageService.GetPackageByIdAsync(PackageId);
            if (Package == null) return NotFound();
            return Ok(Package);
        }

        [HttpDelete("{PackageId}")]
        [Produces("application/json")]
        public async Task<IActionResult> DeletePackage(int PackageId)
        {
            await _PackageService.DeletePackageAsync(PackageId);
            return Ok(new { message = "Package deleted successfully" });
        }

        [HttpPut("update-Package")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdatePackage(PackageDto Package)
        {
            await _PackageService.UpdatePackageAsync(Package);
            return Ok(new { message = "Package updated successfully" });
        }
        [HttpPost("create-Package")]
        public async Task<IActionResult> CreatePackage([FromBody] PackageDto dto)
        {
          var Package = await _PackageService.CreatePackageAsync(dto);
            return Ok(Package);
        }
    }
}