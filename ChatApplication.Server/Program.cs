using ChatApplication.Shared;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

List<Socket> clients = new();
Dictionary<Socket, string> groups = new();

Socket server = new Socket(AddressFamily.InterNetwork,
                           SocketType.Stream,
                           ProtocolType.Tcp);

server.Bind(new IPEndPoint(IPAddress.Any, 9999));
server.Listen(10);

// Hiển thị tất cả IP của máy
Console.WriteLine("=== SERVER STARTED ===");
Console.WriteLine($"Listening on port: 9999");
Console.WriteLine("Available IP addresses:");
foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
{
    if (ip.AddressFamily == AddressFamily.InterNetwork)
    {
        Console.WriteLine($"  - {ip}:9999");
    }
}
Console.WriteLine("=======================\n");

while (true)
{
    Socket client = server.Accept();
    clients.Add(client);

    var clientEndpoint = client.RemoteEndPoint as IPEndPoint;
    Console.WriteLine($"[CONNECTED] Client từ {clientEndpoint?.Address}:{clientEndpoint?.Port}");
    Console.WriteLine($"[INFO] Tổng số client: {clients.Count}");

    Task.Run(() => HandleClient(client));
}

void HandleClient(Socket client)
{
    // Tăng buffer size để nhận file chunks lớn
    byte[] buffer = new byte[256 * 1024]; // 256KB buffer
    StringBuilder receiveBuffer = new();
    var clientEndpoint = client.RemoteEndPoint as IPEndPoint;

    try
    {
        while (true)
        {
            int size = client.Receive(buffer);
            if (size <= 0) break;

            receiveBuffer.Append(Encoding.UTF8.GetString(buffer, 0, size));

            while (receiveBuffer.ToString().Contains("\n"))
            {
                var parts = receiveBuffer.ToString().Split('\n', 2);
                string json = parts[0];
                receiveBuffer.Clear();
                receiveBuffer.Append(parts.Length > 1 ? parts[1] : "");

                var msg = JsonSerializer.Deserialize<ChatMessage>(json);

                // JOIN
                if (msg.Type == "JOIN")
                {
                    groups[client] = msg.Group;
                    Console.WriteLine($"[JOIN] {msg.User} joined group '{msg.Group}'");

                    Send(client, new ChatMessage
                    {
                        Type = "JOIN_ACK"
                    });
                    continue;
                }

                // 🔥 FILE_START - Broadcast nguyên vẹn, KHÔNG thay đổi Type
                if (msg.Type == "FILE_START")
                {
                    Console.WriteLine($"[FILE_START] {msg.User} đang gửi: {msg.FileName}");
                    groups[client] = msg.Group;
                    Broadcast(msg);
                    continue;
                }

                // 🔥 FILE_CHUNK - Broadcast nguyên vẹn
                if (msg.Type == "FILE_CHUNK")
                {
                    // Không log từng chunk để tránh spam
                    groups[client] = msg.Group;
                    Broadcast(msg);
                    continue;
                }

                // 🔥 FILE_END - Broadcast nguyên vẹn
                if (msg.Type == "FILE_END")
                {
                    Console.WriteLine($"[FILE_END] {msg.User} đã gửi xong: {msg.FileName}");
                    groups[client] = msg.Group;
                    Broadcast(msg);
                    continue;
                }

                // MESSAGE thường
                msg.Type = "MESSAGE";
                groups[client] = msg.Group;
                Console.WriteLine($"[MESSAGE] {msg.User}: {msg.Message}");
                Broadcast(msg);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {clientEndpoint?.Address}: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"[DISCONNECTED] {clientEndpoint?.Address}");
        clients.Remove(client);
        groups.Remove(client);
        Console.WriteLine($"[INFO] Còn lại {clients.Count} client");
    }
}

void Send(Socket client, ChatMessage msg)
{
    string json = JsonSerializer.Serialize(msg) + "\n";
    byte[] data = Encoding.UTF8.GetBytes(json);
    client.Send(data);
}

void Broadcast(ChatMessage msg)
{
    string json = JsonSerializer.Serialize(msg) + "\n";
    byte[] data = Encoding.UTF8.GetBytes(json);

    foreach (var c in clients.ToList())
    {
        if (!groups.ContainsKey(c)) continue;

        if (groups[c] == msg.Group)
        {
            try
            {
                c.Send(data);
            }
            catch
            {
                clients.Remove(c);
                groups.Remove(c);
            }
        }
    }
}