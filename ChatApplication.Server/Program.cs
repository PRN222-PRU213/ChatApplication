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

Console.WriteLine("Server started...");

while (true)
{
    Socket client = server.Accept();
    clients.Add(client);

    Task.Run(() => HandleClient(client));
}

void HandleClient(Socket client)
{
    byte[] buffer = new byte[1024];
    StringBuilder receiveBuffer = new();

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

            // 🔥 XỬ LÝ JOIN
            if (msg.Type == "JOIN")
            {
                groups[client] = msg.Group;

                Send(client, new ChatMessage
                {
                    Type = "JOIN_ACK"
                });

                continue;
            }

            // 🔥 MESSAGE
            msg.Type = "MESSAGE";
            groups[client] = msg.Group;
            Broadcast(msg);
        }
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
