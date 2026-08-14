namespace MovieHub.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public int StatusCode { get; set; }

    public string Title { get; set; } = "Something went wrong";

    public string Message { get; set; } = string.Empty;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
