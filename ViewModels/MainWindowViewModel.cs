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
    public async Task StartServer()
    {
        try 
        {
            await _chatService.StartHosting(8080);
        }
        catch (Exception ex)
        {
            StatusText = "Host Error: " + ex.Message;
        }
    }

    [RelayCommand]
    public async Task Connect()
    {
        try 
        {
            StatusText = "Connecting to " + TargetIp + "...";
            await _chatService.ConnectTo(TargetIp, 8080);
        }
        catch (Exception)
        {
            StatusText = "Error: Could not find Host. Check IP!";
        }
    }

    [RelayCommand]
    public async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            try 
            {
                string textToSend = NewMessageText;
                
                await _chatService.SendBroadcast(textToSend);
                
                ChatHistory.Add(new ChatMessage { User = "You", Text = textToSend });
                NewMessageText = string.Empty;
            }
            catch (Exception)
            {
                StatusText = "Failed to send message.";
            }
        }
    }
}

// Updated Data Model with Timestamps for extra "Pro" points
public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
    public string Time { get; set; } = DateTime.Now.ToString("HH:mm");
}