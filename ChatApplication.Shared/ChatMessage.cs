using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Shared
{
    public class ChatMessage
    {
        public string Type { get; set; }

        public string User { get; set; }
        public string Group { get; set; }
        public string Message { get; set; }

        // === THÔNG TIN FILE ===
        public string FileId { get; set; }          // ID duy nhất cho mỗi file

        public string FileName { get; set; }        // Tên file gốc
        public long FileSize { get; set; }          // Tổng kích thước file (bytes)
        public int ChunkIndex { get; set; }         // Thứ tự chunk hiện tại
        public int TotalChunks { get; set; }        // Tổng số chunks
        public string ChunkData { get; set; }       // Dữ liệu chunk (Base64)
    }
}