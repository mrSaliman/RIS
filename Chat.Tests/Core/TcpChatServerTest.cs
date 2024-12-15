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
        server.Start();

        // Assert
        Assert.That(server, Is.Not.Null, "TcpChatServer instance should not be null.");
    }
}