using DataLabelProject.Business.Services.Reviews;
using DataLabel_Project_BE.DTOs.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DataLabel_Project_BE.Controllers;

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
    [Authorize(Roles = "reviewer")]
    public async Task<ActionResult> Create([FromBody] CreateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid input data", errors = ModelState });
        }

        var reviewerId = GetCurrentUserId();
        var (review, errorMessage) = await _reviewService.CreateReviewAsync(reviewerId, request);
        if (review == null)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(new { message = "Review created successfully", data = review });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
