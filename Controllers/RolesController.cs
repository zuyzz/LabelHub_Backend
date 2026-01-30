using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataLabel_Project_BE.DTOs;
using DataLabel_Project_BE.Services;

namespace DataLabel_Project_BE.Controllers
{
    /// <summary>
    /// 🎭 Quản lý Vai trò
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
        /// 📋 Lấy danh sách vai trò
        /// </summary>
        /// <remarks>
        /// Chức năng: Lấy tất cả roles  
        /// Quyền: Admin  
        /// Lỗi: 401, 403
        /// </remarks>
        /// <response code="200">Danh sách vai trò</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();

            var response = roles.Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// 🔍 Xem chi tiết vai trò
        /// </summary>
        /// <remarks>
        /// Chức năng: Lấy 1 role theo ID  
        /// Quyền: Admin  
        /// Lỗi: 401, 403, 404
        /// </remarks>
        /// <param name="id">ID vai trò</param>
        /// <response code="200">Thông tin vai trò</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound(new { message = "Role not found" });

            var response = new RoleResponse { RoleId = role.RoleId, RoleName = role.RoleName };
            return Ok(response);
        }

        /// <summary>
        /// ➕ Tạo vai trò mới
        /// </summary>
        /// <remarks>
        /// Chức năng: Tạo role tùy chỉnh  
        /// Quyền: Admin  
        /// Body: roleName (bắt buộc, duy nhất)  
        /// Lỗi: 400 nếu tên trùng, 401, 403
        /// </remarks>
        /// <param name="request">Tên vai trò</param>
        /// <response code="201">Tạo thành công</response>
        /// <response code="400">Tên trùng</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newRole = await _roleService.CreateRoleAsync(request.RoleName);

                var response = new RoleResponse { RoleId = newRole.RoleId, RoleName = newRole.RoleName };
                return CreatedAtAction(nameof(GetById), new { id = newRole.RoleId }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// ✏️ Cập nhật vai trò
        /// </summary>
        /// <remarks>
        /// Chức năng: Đổi tên role  
        /// Quyền: Admin  
        /// Body: roleName (mới, duy nhất)  
        /// Lỗi: 400 nếu tên trùng, 401, 403, 404
        /// </remarks>
        /// <param name="id">ID vai trò</param>
        /// <param name="request">Tên mới</param>
        /// <response code="200">Cập nhật thành công</response>
        /// <response code="400">Tên trùng</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var role = await _roleService.UpdateRoleAsync(id, request.RoleName);
                if (role == null) return NotFound(new { message = "Role not found" });

                var response = new RoleResponse { RoleId = role.RoleId, RoleName = role.RoleName };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 🗑️ Xóa vai trò
        /// </summary>
        /// <remarks>
        /// Chức năng: Xóa role khỏi hệ thống  
        /// Quyền: Admin  
        /// ⚠️ KHÔNG THỂ xóa nếu có user đang dùng role này  
        /// Lỗi: 400, 401, 403, 404
        /// </remarks>
        /// <param name="id">ID vai trò</param>
        /// <response code="204">Xóa thành công</response>
        /// <response code="400">Role đang dùng</response>
        /// <response code="401">Chưa xác thực</response>
        /// <response code="403">Không có quyền</response>
        /// <response code="404">Không tìm thấy</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _roleService.DeleteRoleAsync(id);
                if (!success) return NotFound(new { message = "Role not found" });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
