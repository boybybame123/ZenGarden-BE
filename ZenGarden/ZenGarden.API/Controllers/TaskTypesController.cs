using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TaskTypesController : ControllerBase
{
    private readonly ITaskTypeService _taskTypeService;

    public TaskTypesController(ITaskTypeService taskTypeService)
    {
        _taskTypeService = taskTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _taskTypeService.GetAllTaskTypesAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _taskTypeService.GetTaskTypeByIdAsync(id);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskTypeDto dto)
    {
        var result = await _taskTypeService.CreateTaskTypeAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.TaskTypeId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskTypeDto dto)
    {
        var success = await _taskTypeService.UpdateTaskTypeAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _taskTypeService.DeleteTaskTypeAsync(id);
        return success ? NoContent() : NotFound();
    }
    
}