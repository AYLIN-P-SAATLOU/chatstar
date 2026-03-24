using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

namespace chatstar.Services;

public class ChatService
{
    private TcpListener? _listener;
    private readonly List<TcpClient> _clients = new();
    public event Action<string, string>? MessageReceived;
    public event Action<string>? StatusChanged;

    public async Task StartHosting(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        StatusChanged?.Invoke($"Hosting on port {port}...");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _clients.Add(client);
            StatusChanged?.Invoke("A friend joined!");
            _ = Task.Run(() => Listen(client, "Friend"));
        }
    }

    public async Task ConnectTo(string ip, int port)
    {
        var client = new TcpClient();
        await client.ConnectAsync(ip, port);
        _clients.Add(client);
        StatusChanged?.Invoke("Connected to host!");
        _ = Task.Run(() => Listen(client, "Host"));
    }

    private async Task Listen(TcpClient client, string senderName)
    {
        using var reader = new StreamReader(client.GetStream());
        while (client.Connected)
        {
            var msg = await reader.ReadLineAsync();
            if (msg != null) MessageReceived?.Invoke(senderName, msg);
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