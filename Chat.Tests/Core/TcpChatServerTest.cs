using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chat.Core;
using Chat.Models;
using Chat.Services;
using NUnit.Framework;

namespace Chat.Tests.Core;

[TestFixture]
[TestOf(typeof(TcpChatServer))]
public class TcpChatServerTest
{

    [Test]
    public void ServerCreationTest()
    {
        // Arrange
        int testPort = 12345;

        // Act
        TcpChatServer server = new TcpChatServer(testPort);

        // Assert
        Assert.That(server, Is.Not.Null, "TcpChatServer instance should not be null.");
    }

    [Test]
    public async Task ServerStartTest()
    {
        // Arrange
        const int testPort = 12345;
        var server = new TcpChatServer(testPort);

        // Act
        var serverTask = Task.Run(() => server.Start());

        await Task.Delay(500); // Даем серверу немного времени на запуск

        // Проверяем, что сервер слушает порт
        using var testClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            await testClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, testPort));
        }
        catch (SocketException ex)
        {
            Assert.Fail($"Unable to connect to the server: {ex.Message}");
        }
        
        var clientManagerField = typeof(TcpChatServer).GetField("_clientManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var clientManager = clientManagerField?.GetValue(server);
        var clientsField = typeof(ClientManager).GetField("_clients",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var clients = clientsField?.GetValue(clientManager) as ConcurrentDictionary<Socket, User>;
        Assert.That(clients, Is.Not.Null);
        var connected = clients.ContainsKey(testClient);

        // Assert
        Assert.That(connected, Is.True, "Client should be able to connect to the server.");
    }
}