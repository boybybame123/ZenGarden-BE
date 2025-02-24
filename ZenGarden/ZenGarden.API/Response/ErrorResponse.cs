namespace ZenGarden.API.Response
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public  string Message { get; set; } = string.Empty;
        public string? Details { get; set; } 

        public ErrorResponse() { }

        public ErrorResponse(string message, string? details = null)
        {
            StatusCode = 500; 
            Message = message;
            Details = details;
        }
    }
}