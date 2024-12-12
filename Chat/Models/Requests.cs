namespace Chat.Models;

public class RequestBase
{
    public string Type { get; set; } = string.Empty;
}

public class AuthRequest : RequestBase
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest : RequestBase
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class MessageRequest : RequestBase
{
    public string Content { get; set; } = string.Empty;
    public bool HasImage { get; set; } = false;
    public byte[]? Image { get; set; } = null;
}

public class HistoryRequest : RequestBase
{
    public string Username { get; set; } = string.Empty;
}