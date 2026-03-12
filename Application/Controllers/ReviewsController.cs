using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabelProject.Application.DTOs.Reviews;
using DataLabelProject.Business.Services.Reviews;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> ReviewAnnotation([FromBody] CreateReviewRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid input data", errors = ModelState });

        try
        {
            var review = await _reviewService.ReviewAnnotationAsync(request);
            return StatusCode(201, review);
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(new { message = knf.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // [HttpGet("/api/tasks/{taskId}/reviews")]
    // public async Task<IActionResult> GetReviewsForTask(Guid taskId)
    // {
    //     var reviews = await _reviewService.GetReviewsForTaskAsync(taskId);
    //     return Ok(reviews);
    // }
}
