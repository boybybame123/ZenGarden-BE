using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Enums;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserTreesController(IUserTreeService userTreeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userTrees = await userTreeService.GetAllAsync();
        return Ok(userTrees);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userTree = await userTreeService.GetByIdAsync(id);
        return Ok(userTree);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] UserTreeDto userTreeDto)
    {
        await userTreeService.AddAsync(userTreeDto);
        return CreatedAtAction(nameof(GetById), new { id = userTreeDto.UserId },
            new { message = "UserTree created successfully" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserTreeDto userTreeDto)
    {
        await userTreeService.UpdateAsync(id, userTreeDto);
        return Ok(new { message = "UserTree updated successfully" });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] TreeStatus newStatus)
    {
        await userTreeService.ChangeStatusAsync(id, newStatus);
        return Ok(new { message = "UserTree status updated successfully" });
    }
}