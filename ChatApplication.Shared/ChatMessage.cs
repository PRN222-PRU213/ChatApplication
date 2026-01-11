using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Shared
{
    public class ChatMessage
    {
        // 🔥 PHÂN BIỆT LOẠI MESSAGE
        // "JOIN" | "JOIN_ACK" | "MESSAGE"
        public string Type { get; set; }

        public string User { get; set; }
        public string Group { get; set; }

        // Nội dung chat (chỉ dùng khi Type = MESSAGE)
        public string Message { get; set; }
    }
}