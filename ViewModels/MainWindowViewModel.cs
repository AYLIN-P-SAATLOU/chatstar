using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace chatstar.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    // 1. Connection Tools
    private TcpListener? _listener;
    private readonly List<TcpClient> _connectedFriends = new();
    
    [ObservableProperty]
    private string? _newMessageText;

    [ObservableProperty]
    private string _targetIp = "127.0.0.1"; // Default to "self" for testing

    public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

    // --- COMMAND 1: START THE SERVER (You do this) ---
    [RelayCommand]
    public async Task StartServer()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, 8080);
            _listener.Start();
            ChatHistory.Add(new ChatMessage { User = "System", Text = "Waiting for friends on Port 8080..." });

            while (true)
            {
                // Wait for a "knock" on the door
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _connectedFriends.Add(client);
                
                ChatHistory.Add(new ChatMessage { User = "System", Text = "A friend connected!" });

                // Start a background task to listen to what THIS friend says
                _ = Task.Run(() => ListenToFriend(client));
            }
        }
        catch (Exception ex)
        {
            ChatHistory.Add(new ChatMessage { User = "Error", Text = ex.Message });
        }
    }

    // --- COMMAND 2: CONNECT TO A SERVER (Your BF does this) ---
    [RelayCommand]
    public async Task Connect()
    {
        try
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(TargetIp, 8080);
            _connectedFriends.Add(client);
            
            ChatHistory.Add(new ChatMessage { User = "System", Text = "Connected to friend!" });
            
            // Start listening for messages from the host
            _ = Task.Run(() => ListenToFriend(client));
        }
        catch (Exception ex)
        {
            ChatHistory.Add(new ChatMessage { User = "Error", Text = "Could not connect: " + ex.Message });
        }
    }

    // --- LOGIC: RECEIVING MESSAGES ---
    private async Task ListenToFriend(TcpClient client)
    {
        var reader = new StreamReader(client.GetStream());
        while (client.Connected)
        {
            var message = await reader.ReadLineAsync();
            if (message != null)
            {
                // Dispatch to UI thread so the screen updates safely
                Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                    ChatHistory.Add(new ChatMessage { User = "Friend", Text = message });
                });
            }
        }
    }

    // --- COMMAND 3: SENDING MESSAGES ---
    [RelayCommand]
    public async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            string msg = NewMessageText;
            ChatHistory.Add(new ChatMessage { User = "You", Text = msg });
            NewMessageText = string.Empty;

            // Send this text to EVERYONE in our list
            foreach (var client in _connectedFriends)
            {
                if (client.Connected)
                {
                    var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    await writer.WriteLineAsync(msg);
                }
            }
        }
    }
}

public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
}