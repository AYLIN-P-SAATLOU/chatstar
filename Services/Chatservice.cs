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
    public event Action<string>? FriendJoined;

    public async Task StartHosting(int port, string myName)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        StatusChanged?.Invoke($"Server Live as {myName}");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _clients.Add(client);
            
            
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

        
        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
        await writer.WriteLineAsync($"NAME_IDENTIFY:{myName}");

        _ = Task.Run(() => Listen(client));
    }

    private async Task Listen(TcpClient client)
    {
        using var reader = new StreamReader(client.GetStream());
        string remoteUser = "Unknown";

        try 
        {
            while (client.Connected)
            {
                var msg = await reader.ReadLineAsync();
                if (msg == null) break;

            
                if (msg.StartsWith("NAME_IDENTIFY:"))
                {
                    remoteUser = msg.Replace("NAME_IDENTIFY:", "");
                    FriendJoined?.Invoke(remoteUser);
                }
                else
                {

                    if (msg.Contains(": ")) 
                    {
                        var parts = msg.Split(": ", 2);
                        MessageReceived?.Invoke(parts[0], parts[1]);
                    } 
                    else 
                    {
                    
                        MessageReceived?.Invoke(remoteUser, msg);
                    }

                
                    if (_listener != null) 
                    {
                    
                        await SendBroadcast($"{remoteUser}: {msg}", client); 
                    }
                }
            }
        } 
        catch 
        { 
            StatusChanged?.Invoke($"{remoteUser} disconnected.");
        }
        finally
        {
            _clients.Remove(client);
        }
    }

    public async Task SendBroadcast(string message, TcpClient? excludeClient = null)
    {
        foreach (var client in _clients.Where(c => c.Connected))
        {
            
            if (client == excludeClient) continue;

            try {
                var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                await writer.WriteLineAsync(message);
            } catch { }
        }
    }

    public void Disconnect()
    {
        try
        {
        
            _listener?.Stop();
            _listener = null;

        
            foreach (var client in _clients)
            {
                client.Close();
            }
            _clients.Clear();
        
            StatusChanged?.Invoke("Disconnected.");
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke("Error during disconnect: " + ex.Message);
        }
    }
}