using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabelProject.Application.DTOs.Reviews;
using DataLabelProject.Business.Services.Reviews;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("reviews")]
    [Authorize(Roles = "admin,manager,reviewer")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] ReviewQueryParameters @params)
    {
        var result = await _reviewService.GetReviewsAsync(@params);
        return Ok(result);
    }

    [HttpGet("tasks/{taskId:guid}/reviews")]
    [Authorize(Roles = "admin,manager,reviewer")]
    public async Task<IActionResult> GetReviewsByTask(
        [FromRoute] Guid taskId, 
        [FromQuery] ReviewQueryParameters @params)
    {
        var result = await _reviewService.GetReviewsByTaskAsync(taskId, @params);
        return Ok(result);
    }

    [HttpGet("tasks/items/{itemId:guid}/reviews")]
    [Authorize(Roles = "admin,manager,reviewer")]
    public async Task<IActionResult> GetReviewsByTaskItem(
        [FromRoute] Guid itemId, 
        [FromQuery] ReviewQueryParameters @params)
    {
        var result = await _reviewService.GetReviewsByTaskItemAsync(itemId, @params);
        return Ok(result);
    }

    [HttpPost("reviews")]
    [Authorize(Roles = "reviewer")]
    public async Task<IActionResult> CreateReviews(
        [FromBody] CreateReviewRequest request)
    {
        await _reviewService.CreateReviewAsync(request);
        return Ok();
    }
}
