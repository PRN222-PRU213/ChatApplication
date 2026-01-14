using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApplication.Business
{
    public class FileBuffer
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public int TotalChunks { get; set; }
        public string User { get; set; }
        public Dictionary<int, byte[]> Chunks { get; set; }
    }
}