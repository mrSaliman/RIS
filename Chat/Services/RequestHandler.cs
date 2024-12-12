using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using Chat.Models;
using Chat.Storage;

namespace Chat.Services;

public static class RequestHandler
{
    public static void ProcessRequest(string message, Socket clientSocket, ConcurrentDictionary<Socket, User> clients)
    {
        var baseRequest = MessageUtils.DeserializeRequest(message);
        if (baseRequest == null) return;

        switch (baseRequest.Type)
        {
            case "auth":
                HandleAuth(clientSocket, baseRequest as AuthRequest, clients);
                break;

            case "reg":
                HandleRegistration(clientSocket, baseRequest as RegisterRequest);
                break;

            case "message":
                HandleMessage(clientSocket, baseRequest as MessageRequest, clients);
                break;

            case "history":
                HandleHistory(clientSocket, baseRequest as HistoryRequest, clients);
                break;
        }
    }

    private static void HandleAuth(Socket clientSocket, AuthRequest? request,
        ConcurrentDictionary<Socket, User> clients)
    {
        if (request == null) return;
        var user = Authentication.Authenticate(request.Username, request.Password);

        if (user != null)
        {
            clients[clientSocket] = user;

            var response = new AuthResponse
            {
                Type = "auth",
                Status = "success",
                Message = $"Authenticated as {user.Username}",
                Username = user.Username
            };

            MessageUtils.SendResponse(clientSocket, response);
        }
        else
        {
            var response = new AuthResponse
            {
                Type = "auth",
                Status = "failed",
                Message = "Invalid credentials"
            };

            MessageUtils.SendResponse(clientSocket, response);
        }
    }

    private static void HandleRegistration(Socket clientSocket, RegisterRequest? request)
    {
        if (request == null) return;
        if (Authentication.Register(request.Username, request.Password, "user"))
        {
            var response = new RegisterResponse
            {
                Type = "reg",
                Status = "success",
                Message = "Registration successful",
                Username = request.Username
            };

            MessageUtils.SendResponse(clientSocket, response);
        }
        else
        {
            var response = new RegisterResponse
            {
                Type = "reg",
                Status = "failed",
                Message = "User already exists"
            };

            MessageUtils.SendResponse(clientSocket, response);
        }
    }

    private static void HandleMessage(Socket clientSocket, MessageRequest? request,
        ConcurrentDictionary<Socket, User> clients)
    {
        if (request == null) return;
        var user = clients.GetValueOrDefault(clientSocket);
        if (user == null) return;

        MessageHistory.LogMessage(user.Username, request.Content, request.Image);

        var response = new MessageResponse
        {
            Type = "message",
            Status = "success",
            Username = user.Username,
            Content = request.Content,
            HasImage = request.HasImage,
            Image = request.Image
        };

        BroadcastMessage(response, clients.Keys);
    }

    private static void HandleHistory(Socket clientSocket, HistoryRequest? request,
        ConcurrentDictionary<Socket, User> clients)
    {
        if (request == null) return;
        var user = clients.GetValueOrDefault(clientSocket);
        var exists = Authentication.Exists(request.Username);

        switch (exists)
        {
            case true when user != null && (user.Username == request.Username || user.Role == "admin"):
            {
                var history = MessageHistory.GetMessages(request.Username);

                var response = new HistoryResponse
                {
                    Type = "history",
                    Status = "success",
                    Username = request.Username,
                    Messages = history
                };

                MessageUtils.SendResponse(clientSocket, response);
                break;
            }
            case false:
            {
                var response = new HistoryResponse
                {
                    Type = "history",
                    Status = "failed",
                    Message = "User with this name does not exist",
                    Username = request.Username
                };

                MessageUtils.SendResponse(clientSocket, response);
                break;
            }
            default:
            {
                var response = new HistoryResponse
                {
                    Type = "history",
                    Status = "failed",
                    Message = "Access denied",
                    Username = request.Username
                };

                MessageUtils.SendResponse(clientSocket, response);
                break;
            }
        }
    }

    private static void BroadcastMessage(object message, IEnumerable<Socket> clients)
    {
        var responseJson = JsonSerializer.Serialize(message);
        foreach (var client in clients) MessageUtils.SendResponse(client, responseJson);
    }
}