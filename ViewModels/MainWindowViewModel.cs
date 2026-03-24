using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks; 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using chatstar.Services;

namespace chatstar.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ChatService _chatService = new();

    [ObservableProperty] private string _targetIp = "127.0.0.1";
    [ObservableProperty] private string _newMessageText = "";
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _myUserName = "User" + Random.Shared.Next(10, 99);

    // This collection powers the Dark Green Sidebar
    public ObservableCollection<ChatMessage> ChatHistory { get; } = new();
    public ObservableCollection<string> ConnectedUsers { get; } = new();

    public MainWindowViewModel()
    {
        // 1. Listen for new messages
        _chatService.MessageReceived += (user, text) => 
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                ChatHistory.Add(new ChatMessage { User = user, Text = text }));

        // 2. Listen for status updates (e.g., "Connected")
        _chatService.StatusChanged += (status) => 
            StatusText = status;

        // 3. Listen for when someone joins to add them to the Sidebar
        _chatService.FriendJoined += (name) => 
            Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                if (!ConnectedUsers.Contains(name)) 
                {
                    ConnectedUsers.Add(name);
                }
            });
    }

    [RelayCommand]
    public async Task StartServer()
    {
        try { await _chatService.StartHosting(8080, MyUserName); }
        catch (Exception ex) { StatusText = "Host Error: " + ex.Message; }
    }

    [RelayCommand]
    public async Task Connect()
    {
        try 
        { 
            StatusText = "Connecting...";
            await _chatService.ConnectTo(TargetIp, 8080, MyUserName); 
        }
        catch (Exception) { StatusText = "Error: Check IP!"; }
    }

    [RelayCommand]
    public async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            // Send to everyone else
            await _chatService.SendBroadcast(NewMessageText);
            
            // Add to our own screen
            ChatHistory.Add(new ChatMessage { User = "You", Text = NewMessageText });
            NewMessageText = string.Empty;
        }
    }

    [RelayCommand]
    public void Leave()
    {
        _chatService.Disconnect();
        
        // Reset the UI
        ChatHistory.Clear();
        ConnectedUsers.Clear();
        StatusText = "Disconnected.";
    }
}

public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
    public string Time { get; set; } = DateTime.Now.ToString("HH:mm");
}