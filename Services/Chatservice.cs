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
            
            // Tell the new person who the Host is
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

        // Tell the Host who we are
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

            // 1. Check if this is a "Handshake" (Name Swap)
                if (msg.StartsWith("NAME_IDENTIFY:"))
                {
                    remoteUser = msg.Replace("NAME_IDENTIFY:", "");
                    FriendJoined?.Invoke(remoteUser);
                }
                else
                {
                // 2. Handle a regular message
                // If the message already has a ": ", it was relayed by the host
                    if (msg.Contains(": ")) 
                    {
                        var parts = msg.Split(": ", 2);
                        MessageReceived?.Invoke(parts[0], parts[1]);
                    } 
                    else 
                    {
                    // It's a direct message from a client to the host
                        MessageReceived?.Invoke(remoteUser, msg);
                    }

                // 3. THE RELAY LOGIC (The "Post Office" fix)
                // If I am the Host (_listener is not null), I broadcast to everyone else.
                    if (_listener != null) 
                    {
                    // CRITICAL: We pass the 'client' here so the sender is EXCLUDED 
                    // from the broadcast. This stops the "Double Message" bug.
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
            // If this client is the one who sent the message, skip them!
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
        // Stop the server if we are hosting
            _listener?.Stop();
            _listener = null;

        // Close all active connections
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