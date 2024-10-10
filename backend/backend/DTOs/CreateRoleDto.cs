using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreateRoleDto
    {
        [Required]
        public string RoleName { get; set; }
    }
}
