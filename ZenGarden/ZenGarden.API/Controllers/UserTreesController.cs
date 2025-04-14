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
        var userTrees = await userTreeService.GetAllUserTreesAsync();
        return Ok(userTrees);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userTree = await userTreeService.GetUserTreeDetailAsync(id);
        return Ok(userTree);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateUserTreeDto createUserTreeDto)
    {
        await userTreeService.AddAsync(createUserTreeDto);
        return CreatedAtAction(nameof(GetById), new { id = createUserTreeDto.UserId },
            new { message = "UserTree created successfully" });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUserTreeDto createUserTreeDto)
    {
        await userTreeService.UpdateAsync(id, createUserTreeDto);
        return Ok(new { message = "UserTree updated successfully" });
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] TreeStatus newStatus)
    {
        await userTreeService.ChangeStatusAsync(id, newStatus);
        return Ok(new { message = "UserTree status updated successfully" });
    }

    [HttpGet("GetUserTree-ByUserId/{id:int}")]
    public async Task<IActionResult> GetAllUserTreeByUserId(int id)
    {
        var userTrees = await userTreeService.GetAllUserTreesByUserIdAsync(id);
        return Ok(userTrees);
    }

    [HttpGet("ListUserTree-ByOwner/{ownerId:int}")]
    public async Task<IActionResult> ListUserTreeByOwner(int ownerId)
    {
        var userTrees = await userTreeService.ListUserTreeByOwner(ownerId);
        return Ok(userTrees);
    }
}