using ChatApplication.Data;
using ChatApplication.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Business
{
    public class ChatService
    {
        private SocketClient _client;

        public event Action<ChatMessage> MessageReceived;

        // 🔥 FLAG QUAN TRỌNG – KHÔNG READY THÌ KHÔNG GỬI CHAT
        public bool IsReady { get; private set; } = false;

        private string _user;
        private string _group;

        public ChatService()
        {
            _client = new SocketClient();

            _client.MessageReceived += msg =>
            {
                // ✅ SERVER XÁC NHẬN JOIN
                if (msg.Type == "JOIN_ACK")
                {
                    IsReady = true;
                    return;
                }

                MessageReceived?.Invoke(msg);
            };
        }

        // 🔹 CONNECT + JOIN
        public void Connect(string user, string group)
        {
            _user = user;
            _group = group;

            _client.Connect("127.0.0.1", 9999);

            // 🔥 JOIN GROUP – BẮT BUỘC
            _client.Send(new ChatMessage
            {
                Type = "JOIN",
                User = user,
                Group = group
            });
        }

        // 🔹 SEND MESSAGE (CHỈ KHI READY)
        public void SendMessage(string message)
        {
            if (!IsReady) return;

            _client.Send(new ChatMessage
            {
                Type = "MESSAGE",
                User = _user,
                Group = _group,
                Message = message
            });
        }
    }
}
