using Chat.Core;

namespace Chat;

internal static class Program
{
    public static void Main()
    {
        var chatServer = new TcpChatServer(8080);
        chatServer.Start();
    }
}