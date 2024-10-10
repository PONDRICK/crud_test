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
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PermissionsController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 5. Get All Permissions
        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();

            var responseData = permissions.Select(p => new
            {
                p.PermissionId,
                p.PermissionName
            });

            return Ok(new { status = new { code = "200", description = "Permissions retrieved successfully" }, data = responseData });
        }

        // 3. Add New Permission
        [HttpPost]
        public async Task<IActionResult> AddPermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid data" }, data = ModelState });
            }

            // ตรวจสอบว่า PermissionName ซ้ำกันหรือไม่
            var existingPermission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionName == createPermissionDto.PermissionName);
            if (existingPermission != null)
            {
                return Conflict(new { status = new { code = "409", description = "PermissionName already exists" }, data = (object)null });
            }

            // Map DTO to Permission entity
            var newPermission = _mapper.Map<Permission>(createPermissionDto);

            await _context.Permissions.AddAsync(newPermission);
            await _context.SaveChangesAsync();

            var responseData = new
            {
                newPermission.PermissionId,
                newPermission.PermissionName
            };

            return CreatedAtAction(nameof(GetAllPermissions), new { id = newPermission.PermissionId }, new { status = new { code = "201", description = "Permission created successfully" }, data = responseData });
        }
    }
}
