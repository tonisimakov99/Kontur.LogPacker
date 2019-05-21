using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Kontur.LogPacker
{
    //копит обработанные данные в буфер и отправляет их в поток
    static class Connector
    {
        private const int BLOCK_SIZE = 131072; 
        static List<byte> buffer = new List<byte>(BLOCK_SIZE);

        public static void Write(Stream stream, byte[] line)
        {
            buffer.AddRange(line);
            if (buffer.Count > BLOCK_SIZE || line.Length == 0)
            {
                stream.Write(buffer.ToArray());
                buffer.Clear();
            }
        }
    }
}
