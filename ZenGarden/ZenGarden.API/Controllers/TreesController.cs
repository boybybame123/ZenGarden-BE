using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TreesController(ITreeService treeService) : ControllerBase
{
    private readonly ITreeService _treeService = treeService ?? throw new ArgumentNullException(nameof(treeService));

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        var items = await _treeService.GetAllTreeAsync();
        return Ok(items);
    }


}
