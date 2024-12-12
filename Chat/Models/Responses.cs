namespace Chat.Models;

public class ResponseBase
{
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "success";
    public string Message { get; set; } = string.Empty;
}

public class AuthResponse : ResponseBase
{
    public string Username { get; set; } = string.Empty;
}

public class RegisterResponse : ResponseBase
{
    public string Username { get; set; } = string.Empty;
}

public class MessageResponse : ResponseBase
{
    public string Username { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool HasImage { get; set; }
    public byte[]? Image { get; set; }
}

public class HistoryResponse : ResponseBase
{
    public string Username { get; set; } = string.Empty;
    public List<(string Message, string? Base64Image)> Messages { get; set; } = [];
}