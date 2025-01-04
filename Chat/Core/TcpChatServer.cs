using System.Net;
using System.Net.Sockets;
using Chat.Services;

namespace Chat.Core;

public class TcpChatServer(int port)
{
    private readonly ClientManager _clientManager = new();

    public void Start(object? smth = null)
    {
        var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        listener.Bind(new IPEndPoint(IPAddress.Any, port));
        listener.Listen(100);
        ThreadPool.SetMinThreads(100, 0);
        Console.WriteLine($"TCP chat server started on port {port}...");

        while (true)
        {
            var clientSocket = listener.Accept();
            ThreadPool.QueueUserWorkItem(_clientManager.HandleClient, clientSocket);
        }
    }
}