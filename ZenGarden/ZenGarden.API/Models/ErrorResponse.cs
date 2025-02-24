namespace ZenGarden.API.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Details { get; set; } // Chi tiết lỗi nếu có

        public ErrorResponse() { }

        public ErrorResponse(string message, string? details = null)
        {
            StatusCode = 500; // Mặc định lỗi 500 nếu không có status
            Message = message;
            Details = details;
        }
    }
}