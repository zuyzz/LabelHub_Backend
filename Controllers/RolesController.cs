using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private static readonly List<Role> Roles = new();

        // GET /api/roles
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Roles);
        }

        // POST /api/roles
        [HttpPost]
        public IActionResult Create(Role role)
        {
            role.RoleId = Guid.NewGuid();
            Roles.Add(role);
            return Ok(role);
        }

        // PUT /api/roles/{id}
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, Role updatedRole)
        {
            var role = Roles.FirstOrDefault(r => r.RoleId == id);
            if (role == null) return NotFound();

            role.RoleName = updatedRole.RoleName;
            return Ok(role);
        }

        // DELETE /api/roles/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var role = Roles.FirstOrDefault(r => r.RoleId == id);
            if (role == null) return NotFound();

            Roles.Remove(role);
            return NoContent();
        }
    }
}
