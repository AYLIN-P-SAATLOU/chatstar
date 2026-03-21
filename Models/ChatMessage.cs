
using System;

namespace chatstar.Models;

public class ChatMessage
{
    public string User { get; set; } = "User";
    public string Text { get; set; } = string.Empty;
    public string Time { get; set; } = DateTime.Now.ToString("t");
}