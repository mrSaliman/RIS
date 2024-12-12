using System.Net.Sockets;
using System.Text;
using Chat.Models;
using Newtonsoft.Json;
using JsonDocument = System.Text.Json.JsonDocument;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Chat.Services;

public static class MessageUtils
{
    public static bool ReceiveRequest(Socket clientSocket, out string message)
    {
        try
        {
            var lengthBuffer = new byte[4];
            clientSocket.Receive(lengthBuffer, 0, 4, SocketFlags.None);
            var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            var messageBuffer = new byte[messageLength];
            clientSocket.Receive(messageBuffer, 0, messageLength, SocketFlags.None);

            message = Encoding.UTF8.GetString(messageBuffer);
            return true;
        }
        catch
        {
            message = string.Empty;
            return false;
        }
    }

    public static void SendResponse(Socket clientSocket, object response)
    {
        var responseJson = JsonConvert.SerializeObject(response);
        SendResponse(clientSocket, responseJson);
    }

    public static void SendResponse(Socket clientSocket, string response)
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        var lengthPrefix = BitConverter.GetBytes(responseBytes.Length);

        clientSocket.Send(lengthPrefix);
        clientSocket.Send(responseBytes);
    }

    public static RequestBase? DeserializeRequest(string jsonMessage)
    {
        using var document = JsonDocument.Parse(jsonMessage);
        var root = document.RootElement;

        if (!root.TryGetProperty("Type", out var typeProperty))
            throw new Exception("Type property not found in JSON");
        var type = typeProperty.GetString();
        return type switch
        {
            "auth" => JsonSerializer.Deserialize<AuthRequest>(jsonMessage),
            "reg" => JsonSerializer.Deserialize<RegisterRequest>(jsonMessage),
            "message" => JsonSerializer.Deserialize<MessageRequest>(jsonMessage),
            "history" => JsonSerializer.Deserialize<HistoryRequest>(jsonMessage),
            _ => throw new Exception("Unknown request type")
        };
    }
}