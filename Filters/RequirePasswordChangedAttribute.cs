using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DataLabel_Project_BE.Services;
using System.Security.Claims;

namespace DataLabel_Project_BE.Filters
{
    /// <summary>
    /// Authorization filter to block users who haven't changed their password on first login
    /// Users with IsFirstLogin = true can only access the change-password endpoint
    /// </summary>
    public class RequirePasswordChangedAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Get AuthService from DI
            var authService = context.HttpContext.RequestServices.GetService<AuthService>();
            if (authService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Get user ID from JWT token
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                await next();
                return;
            }

            // Check if user needs to change password
            var user = await authService.GetUserById(userId);
            if (user != null && user.IsFirstLogin)
            {
                context.Result = new ObjectResult(new
                {
                    message = "You must change your password before accessing this feature. Please use POST /api/auth/change-password endpoint.",
                    requirePasswordChange = true
                })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
