namespace Remp.Service.DTOs;

public class EmailRequestDto
{
    public string To { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? ReplyTo { get; set; }
}