using ChatApplication.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApplication.Data
{
    public class SocketClient
    {
        private Socket _socket;
        private StringBuilder _receiveBuffer = new();

        public event Action<ChatMessage> MessageReceived;

        public void Connect(string ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork,
                                 SocketType.Stream,
                                 ProtocolType.Tcp);

            // Tăng buffer size cho socket
            _socket.ReceiveBufferSize = 256 * 1024; // 256KB
            _socket.SendBufferSize = 256 * 1024;

            _socket.Connect(ip, port);
            Task.Run(ReceiveLoop);
        }

        public void Send(ChatMessage msg)
        {
            string json = JsonSerializer.Serialize(msg) + "\n";
            byte[] data = Encoding.UTF8.GetBytes(json);
            _socket.Send(data);
        }

        private void ReceiveLoop()
        {
            // Tăng buffer để nhận file chunks
            byte[] buffer = new byte[128 * 1024]; // 128KB buffer

            while (true)
            {
                try
                {
                    int size = _socket.Receive(buffer);
                    if (size <= 0) break;

                    _receiveBuffer.Append(Encoding.UTF8.GetString(buffer, 0, size));

                    while (_receiveBuffer.ToString().Contains("\n"))
                    {
                        var parts = _receiveBuffer.ToString().Split('\n', 2);
                        string json = parts[0];
                        _receiveBuffer.Clear();
                        _receiveBuffer.Append(parts.Length > 1 ? parts[1] : "");

                        var msg = JsonSerializer.Deserialize<ChatMessage>(json);
                        MessageReceived?.Invoke(msg);
                    }
                }
                catch
                {
                    break;
                }
            }
        }
    }
}