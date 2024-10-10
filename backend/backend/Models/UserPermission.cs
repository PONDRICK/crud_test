namespace backend.Models
{
    public class UserPermission
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string PermissionId { get; set; }
        public Permission Permission { get; set; }

        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
        public bool IsDeletable { get; set; }
    }
}
