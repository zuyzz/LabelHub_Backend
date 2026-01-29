using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.Models;
using DataLabel_Project_BE.DTOs;

namespace DataLabel_Project_BE.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private static readonly List<User> Users = new();

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var user = Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            user.UserId = Guid.NewGuid();
            user.IsActive = true;
            Users.Add(user);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] User updatedUser)
        {
            var user = Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            user.Email = updatedUser.Email;
            user.IsActive = updatedUser.IsActive;

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var user = Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            Users.Remove(user);
            return NoContent();
        }

        [HttpPut("{id}/role")]
        public IActionResult AssignRole(Guid id, [FromBody] AssignRoleRequest request)
        {
            var user = Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            user.RoleId = request.RoleId;
            // user.RoleName = request.RoleName;

            return Ok(user);
        }
    }
}
