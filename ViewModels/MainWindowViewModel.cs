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
    
    // Identity Properties
    [ObservableProperty] private string _myUserName = "User" + Random.Shared.Next(100, 999);
    [ObservableProperty] private string _remotePartnerName = "Waiting for friend...";

    public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

    public MainWindowViewModel()
    {
        // Listen for messages
        _chatService.MessageReceived += (user, text) => 
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                ChatHistory.Add(new ChatMessage { User = user, Text = text }));

        // Listen for status updates
        _chatService.StatusChanged += (status) => 
            StatusText = status;

        // Listen for the "Handshake" to get the friend's name
        _chatService.FriendJoined += (name) => 
            RemotePartnerName = name;
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
            await _chatService.SendBroadcast(NewMessageText);
            ChatHistory.Add(new ChatMessage { User = MyUserName, Text = NewMessageText });
            NewMessageText = string.Empty;
        }
    }
}

public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
    public string Time { get; set; } = DateTime.Now.ToString("HH:mm");
}