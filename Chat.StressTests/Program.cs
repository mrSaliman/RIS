using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Chat.Models;

namespace Chat.StressTests;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        const string ip = "127.0.0.1";
        const int port = 8080;

        List<Socket> clients = [];

        var auth = new AuthRequest { Username = "admin", Password = "1234", Type = "auth"};

        for (var i = 0; i < 100; i++)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.Connect(ip, port);
            SendMessage(auth, socket);
            clients.Add(socket);
        }

        var msg = GenerateBigMsg(100000);

        for (var i = 0; i < 1000; i++)
        {
            //Console.ReadKey();
            foreach (var client in clients)
            {
                SendMessage(msg, client);
            }

            Console.WriteLine($"Отправлено {(i + 1) * clients.Count} запросов");
        }

        Console.Read();
    }

    private static void SendMessage(object message, Socket socket)
    {
        
        var jsonMessage = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
        var lengthPrefix = BitConverter.GetBytes(messageBytes.Length);

        socket.Send(lengthPrefix);
        socket.Send(messageBytes);
    }

    private static void ReceiveMessage(Socket socket)
    {
        try
        {
            var lengthBuffer = new byte[4];
            socket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
            var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            var messageBuffer = new byte[messageLength];
            socket.Receive(messageBuffer, 0, messageLength, SocketFlags.None);
        }
        catch
        {
            Console.WriteLine("error");
        }
    }

    private static MessageRequest GenerateBigMsg(int contentSize)
    {
        var content = new StringBuilder();
        for (var i = 0; i < contentSize; i++)
        {
            content.Append('s');
        }

        return new MessageRequest
        {
            Content = content.ToString(),
            HasImage = false,
            Image = null,
            Type = "message"
        };
    }
}