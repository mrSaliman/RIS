using System.Net;
using System.Net.Sockets;
using Chat.Services;

namespace Chat.Core;

public class TcpChatServer(int port)
{
    private readonly ClientManager _clientManager = new();
    
    internal ClientManager ClientManager => _clientManager;

    public void Start()
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Any, port));
        listener.Listen(10);

        Console.WriteLine($"TCP chat server started on port {port}...");

        while (true)
        {
            var clientSocket = listener.Accept();
            ThreadPool.QueueUserWorkItem(_clientManager.HandleClient, clientSocket);
        }
    }
}