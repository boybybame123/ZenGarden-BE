using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackageController(IPackagesService packagesService) : ControllerBase
{
    private readonly IPackagesService _packageService =
        packagesService ?? throw new ArgumentNullException(nameof(packagesService));

    [HttpGet]
    public async Task<IActionResult> GetPackages()
    {
        var packages = await _packageService.GetAllPackagesAsync();
        return Ok(packages);
    }

    [HttpGet("{packageId:int}")]
    public async Task<IActionResult> GetPackageById(int packageId)
    {
        var package = await _packageService.GetPackageByIdAsync(packageId);
        if (package == null) return NotFound();
        return Ok(package);
    }

    [HttpDelete("{packageId:int}")]
    [Produces("application/json")]
    public async Task<IActionResult> DeletePackage(int packageId)
    {
        await _packageService.DeletePackageAsync(packageId);
        return Ok(new { message = "Package deleted successfully" });
    }

    [HttpPut("update-Package")]
    [Produces("application/json")]
    public async Task<IActionResult> UpdatePackage(PackageDto package)
    {
        await _packageService.UpdatePackageAsync(package);
        return Ok(new { message = "Package updated successfully" });
    }

    [HttpPost("create-Package")]
    public async Task<IActionResult> CreatePackage([FromBody] PackageDto dto)
    {
        var package = await _packageService.CreatePackageAsync(dto);
        return Ok(package);
    }
}