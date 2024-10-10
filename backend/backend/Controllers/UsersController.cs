using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // 1. Delete User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { status = new { code = "404", description = "User not found" }, data = (object)null });
            }

            _context.UserPermissions.RemoveRange(user.UserPermissions);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { status = new { code = "200", description = "User deleted successfully" }, data = new { result = true, message = "User deleted successfully" } });
        }

        // 2. Edit User
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] CreateUserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid user ID" }, data = (object)null });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid data" }, data = ModelState });
            }

            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { status = new { code = "404", description = "User not found" }, data = (object)null });
            }

            // ตรวจสอบ RoleId
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == userDto.RoleId);
            if (role == null)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid RoleId" }, data = (object)null });
            }

            // ตรวจสอบ PermissionIds
            var permissionIds = userDto.UserPermissions.Select(up => up.PermissionId).ToList();
            var permissions = await _context.Permissions.Where(p => permissionIds.Contains(p.PermissionId)).ToListAsync();
            if (permissions.Count != permissionIds.Count)
            {
                return BadRequest(new { status = new { code = "400", description = "One or more PermissionIds are invalid" }, data = (object)null });
            }

            // อัปเดตข้อมูลผู้ใช้
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.Phone = userDto.Phone;
            user.RoleId = userDto.RoleId;
            user.Username = userDto.Username;

            // อัปเดตรหัสผ่านถ้ามีการเปลี่ยนแปลง
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                using var sha256 = SHA256.Create();
                user.Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)));
            }

            // อัปเดต Permissions
            _context.UserPermissions.RemoveRange(user.UserPermissions);

            user.UserPermissions = userDto.UserPermissions.Select(up => new UserPermission
            {
                UserId = user.Id,
                PermissionId = up.PermissionId,
                IsReadable = up.IsReadable,
                IsWritable = up.IsWritable,
                IsDeletable = up.IsDeletable
            }).ToList();

            await _context.SaveChangesAsync();

            // เตรียมข้อมูลตอบกลับ
            var responseData = new
            {
                userId = user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                role = new { roleId = role.RoleId, roleName = role.RoleName },
                user.Username,
                permissions = user.UserPermissions.Select(up => new
                {
                    up.PermissionId,
                    permissionName = up.Permission.PermissionName
                })
            };

            return Ok(new { status = new { code = "200", description = "User updated successfully" }, data = responseData });
        }

        // 3. Get User By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { status = new { code = "404", description = "User not found" }, data = (object)null });
            }

            var responseData = new
            {
                id = user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                role = new { roleId = user.Role.RoleId, roleName = user.Role.RoleName },
                user.Username,
                permissions = user.UserPermissions.Select(up => new
                {
                    up.PermissionId,
                    permissionName = up.Permission.PermissionName
                })
            };

            return Ok(new { status = new { code = "200", description = "User retrieved successfully" }, data = responseData });
        }

        // 4. Get Users (DataTable)
        [HttpPost("DataTable")]
        public async Task<IActionResult> GetUsers([FromBody] DataTableRequest request)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .AsQueryable();

            // การค้นหา
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(request.Search) ||
                    u.LastName.Contains(request.Search) ||
                    u.Email.Contains(request.Search));
            }

            // การจัดเรียง
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                switch (request.OrderBy)
                {
                    case "FirstName":
                        query = request.OrderDirection == "DESC" ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName);
                        break;
                    case "LastName":
                        query = request.OrderDirection == "DESC" ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName);
                        break;
                    case "Email":
                        query = request.OrderDirection == "DESC" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                        break;
                    default:
                        break;
                }
            }

            var totalCount = await query.CountAsync();

            // การแบ่งหน้า
            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dataSource = users.Select(user => new
            {
                userId = user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                role = new { roleId = user.Role.RoleId, roleName = user.Role.RoleName },
                user.Username,
                permissions = user.UserPermissions.Select(up => new
                {
                    up.PermissionId,
                    permissionName = up.Permission.PermissionName
                }),
                createdDate = user.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss")
            });

            var responseData = new
            {
                dataSource,
                page = request.PageNumber,
                pageSize = request.PageSize,
                totalCount
            };

            return Ok(responseData);
        }

        // 7. Add new user
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] CreateUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid data" }, data = ModelState });
            }

            // Check if username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { status = new { code = "400", description = "Username already exists" }, data = (object)null });
            }

            // Check if RoleId exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == userDto.RoleId);
            if (role == null)
            {
                return BadRequest(new { status = new { code = "400", description = "Invalid RoleId" }, data = (object)null });
            }

            // Check if all PermissionIds exist
            var permissionIds = userDto.UserPermissions.Select(up => up.PermissionId).ToList();
            var permissions = await _context.Permissions.Where(p => permissionIds.Contains(p.PermissionId)).ToListAsync();
            if (permissions.Count != permissionIds.Count)
            {
                return BadRequest(new { status = new { code = "400", description = "One or more PermissionIds are invalid" }, data = (object)null });
            }

            // Hash the password (แนะนำให้ใช้ hashing แทนการเก็บเป็น plain text)
            using var sha256 = SHA256.Create();
            var hashedPassword = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)));

            // Map DTO to User entity
            var user = _mapper.Map<User>(userDto);
            user.Password = hashedPassword;
            user.CreatedDate = DateTime.UtcNow;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Prepare response data
            var responseData = new
            {
                userId = user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Phone,
                role = new { roleId = role.RoleId, roleName = role.RoleName },
                user.Username,
                permissions = user.UserPermissions.Select(up => new
                {
                    up.PermissionId,
                    permissionName = up.Permission.PermissionName
                })
            };

            return Ok(new { status = new { code = "200", description = "User created successfully" }, data = responseData });
        }
    }
}
