using Microsoft.AspNetCore.Mvc;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Controllers;

[ApiController]
[Route("api/trees")]
public class TreesController(ITreeService treeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var trees = await treeService.GetAllAsync();
        return Ok(trees);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tree = await treeService.GetByIdAsync(id);
        return Ok(tree);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TreeDto treeDto)
    {
        var createdTree = await treeService.AddAsync(treeDto);
        return CreatedAtAction(nameof(GetById), new { id = createdTree.TreeId }, createdTree);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TreeDto treeDto)
    {
        await treeService.UpdateAsync(id, treeDto);
        return Ok(new { message = "Tree updated successfully" });
    }


    [HttpPut("disable/{treeId:int}")]
    public async Task<IActionResult> DisableTree(int treeId)
    {
        await treeService.DisableTreeAsync(treeId);
        return Ok(new { message = "Tree disabled successfully." });
    }
}