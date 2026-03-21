using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; 
using System.Collections.ObjectModel;

namespace chatstar.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _newMessageText;

    public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

    // The [RelayCommand] attribute automatically creates "SendMessageCommand" for you!
    [RelayCommand]
    public void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessageText))
        {
            ChatHistory.Add(new ChatMessage { User = "You", Text = NewMessageText });
            NewMessageText = string.Empty;
        }
    }
}

public class ChatMessage
{
    public string? User { get; set; }
    public string? Text { get; set; }
}