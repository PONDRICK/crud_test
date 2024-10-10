namespace backend.Models
{
    public class DataTableRequest
    {
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; }
        public int PageNumber { get; set; } = 1; // ค่าเริ่มต้นหน้าแรก
        public int PageSize { get; set; } = 10; // ค่าเริ่มต้นขนาดหน้า
        public string Search { get; set; }
    }
}
