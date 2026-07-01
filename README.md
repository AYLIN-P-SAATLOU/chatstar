readme_content = """# ChatStar

ChatStar is a multi-user desktop chat app where a central host dynamically relays real-time messages to all connected users in a star-shaped network.

## 🚀 Technologies Used
* **C# / .NET** (Core logic)
* **Avalonia UI** (Desktop interface)
* **TCP/IP Sockets** (Networking)

## 🏗️ Architecture: MVVM Pattern
This project follows the **Model-View-ViewModel (MVVM)** architectural design pattern to completely separate user interface logic from application/networking engines:
* **View (`MainWindow.axaml`):** The face of the application—handles the visual UI components, color layouts, and structural inputs.
* **ViewModel (`MainWindowViewModel.cs`):** The data bridge—coordinates commands, maintains observable UI states, and processes events.
* **Model/Service (`ChatService.cs`):** The engine room—manages standard TCP/IP network sockets, listens on background streams, and executes message broadcasts.

## 🌟 Key Features
* **Star Topology Relay Engine:** A single instance serves as the central server node, managing concurrent incoming client sockets and intelligently distributing messages to all other room members.
* **Sender Echo Exclusion:** Fixed relay loop tracking ensuring that broadcast relays exclude the original sending socket, preventing duplicate message echoes.
* **Custom Communication Protocol:** Implements an implicit handshake mechanism using command signatures like `NAME_IDENTIFY:[UserName]` to exchange peer identities and update room member states across nodes dynamically.
* **Thread-Safe UI Operations:** Safely offloads continuous blocking stream listener tasks onto dedicated background tasks (`Task.Run`) and routes incoming UI-bound message packets cleanly using Avalonia's `Dispatcher.UIThread.Post`.
* **Graceful Disconnection Handlers:** Explicit 'Leave' command sequence to close streams cleanly, notify neighboring peers instantly, and flush memory pools to ensure no ghost connections remain.
* **Focus Priority Startup:** Configured window presentation properties to pop up on top of operational windows immediately upon execution.

## 💻 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later installed on your system.

### How to Run
1. Open your terminal in the root directory of the project.
2. Run the application using the .NET CLI:
   Kod çıkışı
File written successfully.

```bash
   dotnet run
```

##To test the group chat functionality locally, open multiple instances of the terminal and run dotnet run inside each.
Designate one instance as the Host and the others as clients connecting to 127.0.0.1. 
