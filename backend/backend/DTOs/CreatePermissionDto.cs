using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class CreatePermissionDto
    {
        [Required]
        public string PermissionName { get; set; }
    }
}
