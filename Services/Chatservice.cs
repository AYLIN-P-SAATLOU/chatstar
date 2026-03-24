using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace chatstar.Services;

public class ChatService
{
    private TcpListener? _listener;
    private readonly List<TcpClient> _clients = new();
    public event Action<string, string>? MessageReceived;
    public event Action<string>? StatusChanged;
    public event Action<string>? FriendJoined; // New event!

    public async Task StartHosting(int port, string myName)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        StatusChanged?.Invoke($"Hosting as {myName}...");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _clients.Add(client);
            
            // Handshake: Send our name to the person who joined
            var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            await writer.WriteLineAsync($"NAME_IDENTIFY:{myName}");

            _ = Task.Run(() => Listen(client));
        }
    }

    public async Task ConnectTo(string ip, int port, string myName)
    {
        var client = new TcpClient();
        await client.ConnectAsync(ip, port);
        _clients.Add(client);

        // Handshake: Send our name to the host
        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
        await writer.WriteLineAsync($"NAME_IDENTIFY:{myName}");

        _ = Task.Run(() => Listen(client));
    }

    private async Task Listen(TcpClient client)
    {
        using var reader = new StreamReader(client.GetStream());
        string remoteUserName = "Unknown";

        while (client.Connected)
        {
            var msg = await reader.ReadLineAsync();
            if (msg == null) break;

            if (msg.StartsWith("NAME_IDENTIFY:"))
            {
                remoteUserName = msg.Replace("NAME_IDENTIFY:", "");
                FriendJoined?.Invoke(remoteUserName);
                StatusChanged?.Invoke($"Connected to {remoteUserName}");
            }
            else
            {
                MessageReceived?.Invoke(remoteUserName, msg);
            }
        }
    }

    public async Task SendBroadcast(string message)
    {
        foreach (var client in _clients.Where(c => c.Connected))
        {
            var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
            await writer.WriteLineAsync(message);
        }
    }
}