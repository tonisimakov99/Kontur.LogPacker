using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace Kontur.LogPacker
{
    //контейнер для строки формата сжатого лога
    class CompressedLog
    {
        public double DeltaDateTime { get; set; }
        public ulong DeltaRunTime { get; set; }
        public byte[] Message { get; set; }

        public CompressedLog(double deltaDateTime, ulong deltaRunTime, byte[] message)
        {
            this.DeltaDateTime = deltaDateTime;
            this.DeltaRunTime = deltaRunTime;
            this.Message = message;
        }

        public static bool TryParse(byte[] line, out CompressedLog log)
        {
            log = null;

            if (NumberHandler.TryParseNumber(line, 0, out ulong deltaDateTime, out var indexEndDeltaTime))
            {
                if (NumberHandler.TryParseNumber(line, indexEndDeltaTime + 2, out ulong deltaRunTime, out var indexEndDeltaRunTime))
                {
                    if (line[indexEndDeltaTime + 1] == ' ' && line[indexEndDeltaRunTime + 1] == ' ' && line[indexEndDeltaRunTime + 5 + 2] == ' ')
                    {
                        var message = new byte[line.Length - (indexEndDeltaRunTime + 2)];
                        Array.Copy(line, indexEndDeltaRunTime + 2, message, 0, line.Length - (indexEndDeltaRunTime + 2));
                        log = new CompressedLog(deltaDateTime, deltaRunTime, message);
                        return true;
                    }
                }
            }
            return false;
        }

        public byte[] ToBytes()
        {
            var bytes = new List<byte>(150);
            bytes.AddRange(NumberHandler.NumberToBytesArray((ulong)this.DeltaDateTime));
            bytes.Add((byte)' ');
            bytes.AddRange(NumberHandler.NumberToBytesArray(this.DeltaRunTime));
            bytes.Add((byte)' ');
            bytes.AddRange(this.Message);
           
            return bytes.ToArray();
        }
    }
}
