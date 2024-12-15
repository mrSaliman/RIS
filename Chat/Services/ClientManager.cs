using System.Collections.Concurrent;
using System.Net.Sockets;
using Chat.Models;

namespace Chat.Services;

public class ClientManager
{
    private readonly ConcurrentDictionary<Socket, User> _clients = new();
    private int _reqCount;

    public void HandleClient(object? state)
    {
        if (state is not Socket clientSocket) return;

        try
        {
            _clients[clientSocket] = new User { Username = "Anon", Role = "user" };
            Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");

            while (true)
            {
                if (!MessageUtils.ReceiveRequest(clientSocket, out var message)) break;
                RequestHandler.ProcessRequest(message, clientSocket, _clients);
                Console.WriteLine($"Request {++_reqCount} successfully handled: {clientSocket.RemoteEndPoint}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with client {clientSocket.RemoteEndPoint}: {ex.Message}");
        }
        finally
        {
            DisconnectClient(clientSocket);
        }
    }

    public void DisconnectClient(Socket clientSocket)
    {
        if (!_clients.TryRemove(clientSocket, out _)) return;
        Console.WriteLine($"Client disconnected: {clientSocket.RemoteEndPoint}");
        clientSocket.Close();
    }
}