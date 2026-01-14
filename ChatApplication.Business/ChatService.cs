using ChatApplication.Data;
using ChatApplication.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Business
{
    public class ChatService
    {
        private SocketClient _client;

        // Events
        public event Action<ChatMessage> MessageReceived;

        public event Action<string, string, int, int> FileProgressChanged;  // FileId, FileName, Current, Total

        public event Action<string, string, byte[]> FileReceived;           // FileId, FileName, Data

        public bool IsReady { get; private set; } = false;

        private string _user;
        private string _group;

        public string ServerIP { get; set; } = "172.16.16.1";
        public int ServerPort { get; set; } = 9999;

        // Buffer để ghép các chunks file nhận được
        private Dictionary<string, FileBuffer> _fileBuffers = new();

        // Kích thước mỗi chunk (64KB)
        private const int CHUNK_SIZE = 64 * 1024;

        public ChatService()
        {
            _client = new SocketClient();

            _client.MessageReceived += msg =>
            {
                if (msg.Type == "JOIN_ACK")
                {
                    IsReady = true;
                    return;
                }

                // Xử lý file
                if (msg.Type == "FILE_START" || msg.Type == "FILE_CHUNK" || msg.Type == "FILE_END")
                {
                    HandleFileMessage(msg);
                    return;
                }

                MessageReceived?.Invoke(msg);
            };
        }

        public void Connect(string user, string group)
        {
            _user = user;
            _group = group;

            _client.Connect(ServerIP, ServerPort);

            _client.Send(new ChatMessage
            {
                Type = "JOIN",
                User = user,
                Group = group
            });
        }

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

        /// <summary>
        /// Gửi file lớn bằng cách chia thành nhiều chunks
        /// </summary>
        public async Task SendFileAsync(string filePath, Action<int> progressCallback = null)
        {
            if (!IsReady) return;

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) return;

            string fileId = Guid.NewGuid().ToString("N")[..12];
            string fileName = fileInfo.Name;
            long fileSize = fileInfo.Length;

            // Tính số chunks
            int totalChunks = (int)Math.Ceiling((double)fileSize / CHUNK_SIZE);

            // Gửi FILE_START
            _client.Send(new ChatMessage
            {
                Type = "FILE_START",
                User = _user,
                Group = _group,
                FileId = fileId,
                FileName = fileName,
                FileSize = fileSize,
                TotalChunks = totalChunks
            });

            await Task.Delay(100);

            // Đọc và gửi từng chunk
            byte[] buffer = new byte[CHUNK_SIZE];
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            for (int i = 0; i < totalChunks; i++)
            {
                int bytesRead = await fs.ReadAsync(buffer, 0, CHUNK_SIZE);
                string chunkData = Convert.ToBase64String(buffer, 0, bytesRead);

                _client.Send(new ChatMessage
                {
                    Type = "FILE_CHUNK",
                    User = _user,
                    Group = _group,
                    FileId = fileId,
                    FileName = fileName,
                    ChunkIndex = i,
                    TotalChunks = totalChunks,
                    ChunkData = chunkData
                });

                // Callback tiến trình
                progressCallback?.Invoke((i + 1) * 100 / totalChunks);

                await Task.Delay(30);
            }

            await Task.Delay(100);

            // Gửi FILE_END
            _client.Send(new ChatMessage
            {
                Type = "FILE_END",
                User = _user,
                Group = _group,
                FileId = fileId,
                FileName = fileName,
                Message = $"📎 Đã gửi file: {fileName} ({FormatFileSize(fileSize)})"
            });

            await Task.Delay(200);
        }

        /// <summary>
        /// Xử lý tin nhắn file nhận được
        /// </summary>
        private void HandleFileMessage(ChatMessage msg)
        {
            // 🔥 BỎ QUA FILE DO CHÍNH MÌNH GỬI - KHÔNG CẦN LƯU LẠI
            if (msg.User == _user)
            {
                // Chỉ hiển thị thông báo khi gửi xong (FILE_END)
                if (msg.Type == "FILE_END")
                {
                    MessageReceived?.Invoke(msg);
                }
                return;
            }

            switch (msg.Type)
            {
                case "FILE_START":
                    // Tạo buffer mới cho file (chỉ cho file của người khác)
                    _fileBuffers[msg.FileId] = new FileBuffer
                    {
                        FileName = msg.FileName,
                        FileSize = msg.FileSize,
                        TotalChunks = msg.TotalChunks,
                        User = msg.User,
                        Chunks = new Dictionary<int, byte[]>()
                    };
                    FileProgressChanged?.Invoke(msg.FileId, msg.FileName, 0, msg.TotalChunks);
                    break;

                case "FILE_CHUNK":
                    if (_fileBuffers.TryGetValue(msg.FileId, out var buffer))
                    {
                        // Lưu chunk
                        byte[] chunkData = Convert.FromBase64String(msg.ChunkData);
                        buffer.Chunks[msg.ChunkIndex] = chunkData;

                        // Cập nhật tiến trình
                        FileProgressChanged?.Invoke(msg.FileId, msg.FileName, buffer.Chunks.Count, msg.TotalChunks);
                    }
                    break;

                case "FILE_END":
                    if (_fileBuffers.TryGetValue(msg.FileId, out var completedBuffer))
                    {
                        // Ghép tất cả chunks thành file hoàn chỉnh
                        using var ms = new MemoryStream();
                        for (int i = 0; i < completedBuffer.TotalChunks; i++)
                        {
                            if (completedBuffer.Chunks.TryGetValue(i, out var chunk))
                            {
                                ms.Write(chunk, 0, chunk.Length);
                            }
                        }

                        // Notify file đã nhận xong
                        FileReceived?.Invoke(msg.FileId, completedBuffer.FileName, ms.ToArray());

                        // Xóa buffer
                        _fileBuffers.Remove(msg.FileId);
                    }

                    // Hiển thị thông báo
                    MessageReceived?.Invoke(msg);
                    break;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }

        // Class để lưu trữ các chunks đang nhận
        private class FileBuffer
        {
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public int TotalChunks { get; set; }
            public string User { get; set; }
            public Dictionary<int, byte[]> Chunks { get; set; }
        }
    }
}