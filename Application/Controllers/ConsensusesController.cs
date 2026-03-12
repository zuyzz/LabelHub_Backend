using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Business.Services.Consensus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/consensuses")]
[Authorize]
public class ConsensusesController : ControllerBase
{
    private readonly IConsensusService _consensusService;

    public ConsensusesController(IConsensusService consensusService)
    {
        _consensusService = consensusService;
    }

    [HttpPost("{taskId:guid}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> CreateConsensus([FromRoute] Guid taskId, [FromBody] ConsensusCreateRequest request)
    {
        try
        {
            var result = await _consensusService.CreateConsensusAsync(taskId, request);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{consensusId:guid}")]
    [Authorize(Roles = "admin,manager,reviewer")]
    public async Task<IActionResult> GetConsensusById([FromRoute] Guid consensusId)
    {
        var result = await _consensusService.GetConsensusByIdAsync(consensusId);
        if (result == null)
            return NotFound(new { message = "Consensus not found" });

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "admin,manager,reviewer")]
    public async Task<IActionResult> GetConsensuses([FromQuery] ConsensusQueryParameters @params)
    {
        var result = await _consensusService.GetConsensusesAsync(@params);
        return Ok(result);
    }
}
