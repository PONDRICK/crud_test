using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreateUserDto
    {
        public string Id { get; set; } // หรือให้สร้าง ID อัตโนมัติ

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public string RoleId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public List<CreateUserPermissionDto> UserPermissions { get; set; }
    }

    public class CreateUserPermissionDto
    {
        [Required]
        public string PermissionId { get; set; }

        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
        public bool IsDeletable { get; set; }
    }
}
