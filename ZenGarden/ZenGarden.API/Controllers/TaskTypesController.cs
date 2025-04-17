using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskTypesController(ITaskTypeService taskTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await taskTypeService.GetAllTaskTypesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await taskTypeService.GetTaskTypeByIdAsync(id);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskTypeDto dto)
    {
        var result = await taskTypeService.CreateTaskTypeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.TaskTypeId }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskTypeDto dto)
    {
        var success = await taskTypeService.UpdateTaskTypeAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await taskTypeService.DeleteTaskTypeAsync(id);
        return success ? NoContent() : NotFound();
    }
}