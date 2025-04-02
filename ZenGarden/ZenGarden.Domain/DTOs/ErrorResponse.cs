namespace ZenGarden.Domain.DTOs;

public class ErrorResponse
{
    public ErrorResponse()
    {
    }

    public ErrorResponse(string message, object? details = null)
    {
        StatusCode = 500;
        Message = message;
        Details = details;
    }

    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Details { get; set; }
}