namespace ZenGarden.Domain.DTOs;

public class ErrorResponse
{
    public ErrorResponse()
    {
    }

    public ErrorResponse(int statusCode, string message, object? details = null)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }

    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}