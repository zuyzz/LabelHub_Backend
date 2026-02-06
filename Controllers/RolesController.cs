using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// Role Management
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <response code="200">List of roles</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            
            // Map to RoleResponse DTOs
            var response = roles.Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <response code="200">Role details</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Map to RoleResponse DTO
            var response = new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };

            return Ok(response);
        }

        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="request">Role name</param>
        /// <response code="201">Role created</response>
        /// <response code="400">Duplicate name</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                var newRole = await _roleService.CreateRoleAsync(request.RoleName);
                
                // Map to RoleResponse DTO
                var response = new RoleResponse
                {
                    RoleId = newRole.RoleId,
                    RoleName = newRole.RoleName
                };

                return CreatedAtAction(nameof(GetById), new { id = newRole.RoleId }, response);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Check for unique constraint violations
                if (dbEx.InnerException?.Message.Contains("Role_roleName_key") == true)
                {
                    return BadRequest(new { message = "Role name already exists" });
                }
                return BadRequest(new { message = "Database error occurred", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="request">New role name</param>
        /// <response code="200">Role updated</response>
        /// <response code="400">Duplicate name</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                var role = await _roleService.UpdateRoleAsync(id, request.RoleName);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Map to RoleResponse DTO
                var response = new RoleResponse
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName
                };

                return Ok(response);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Check for unique constraint violations
                if (dbEx.InnerException?.Message.Contains("Role_roleName_key") == true)
                {
                    return BadRequest(new { message = "Role name already exists" });
                }
                return BadRequest(new { message = "Database error occurred", details = dbEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <response code="204">Role deleted</response>
        /// <response code="400">Role is in use</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _roleService.DeleteRoleAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
