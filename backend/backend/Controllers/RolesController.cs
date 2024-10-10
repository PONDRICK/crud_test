using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.DTOs;
using backend.Models;
using AutoMapper;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public RolesController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 6. Get All Roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();

            var responseData = roles.Select(r => new
            {
                r.RoleId,
                r.RoleName
            });

            return Ok(new { status = new { code = "200", description = "Roles retrieved successfully" }, data = responseData });
        }

        // 2. Add New Role
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid data" }, data = ModelState });
            }

            // ตรวจสอบว่า RoleName ซ้ำกันหรือไม่
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == createRoleDto.RoleName);
            if (existingRole != null)
            {
                return Conflict(new { status = new { code = "409", description = "RoleName already exists" }, data = (object)null });
            }

            // Map DTO to Role entity
            var newRole = _mapper.Map<Role>(createRoleDto);

            await _context.Roles.AddAsync(newRole);
            await _context.SaveChangesAsync();

            var responseData = new
            {
                newRole.RoleId,
                newRole.RoleName
            };

            return CreatedAtAction(nameof(GetAllRoles), new { id = newRole.RoleId }, new { status = new { code = "201", description = "Role created successfully" }, data = responseData });
        }
    }
}
