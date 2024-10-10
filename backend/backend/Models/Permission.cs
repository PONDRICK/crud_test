using System.Collections.Generic;

namespace backend.Models
{
    public class Permission
    {
        public string PermissionId { get; set; }
        public string PermissionName { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}
