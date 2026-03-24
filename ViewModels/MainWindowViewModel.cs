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

    public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

    public MainWindowViewModel()
    {
        // Listen to the Service's events
        _chatService.MessageReceived += (user, text) => 
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                ChatHistory.Add(new ChatMessage { User = user, Text = text }));

        _chatService.StatusChanged += (status) => 
            StatusText = status;
    }

    [RelayCommand]
    public async Task StartServer() => await _chatService.StartHosting(8080);

    [RelayCommand]
    public async Task Connect() => await _chatService.ConnectTo(TargetIp, 8080);

    [RelayCommand]
    public async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            // We need to capture the text before clearing the box
            string textToSend = NewMessageText;
            
            await _chatService.SendBroadcast(textToSend);
            
            ChatHistory.Add(new ChatMessage { User = "You", Text = textToSend });
            NewMessageText = string.Empty;
        }
    }
}

// This was missing! The View needs to know what a 'ChatMessage' is.
public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
}