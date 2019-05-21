using System;
using System.IO;
using System.IO.Compression;

namespace Kontur.LogPacker
{
    internal static class EntryPoint
    {
        private const string SELECTION_FLAG = "-d";

        public static void Main(string[] args)
        {
            var i = 0;
            var selectedCompress = args[0] != SELECTION_FLAG;

            if (!selectedCompress)
                i++;

            using (FileStream source = new FileStream(args[i], FileMode.Open))
            {
                using (FileStream target = new FileStream(args[i+1], FileMode.Create))
                {
                    if (selectedCompress)
                        Compressor.Start(source, target, CompressMode.Compress);
                    else
                        Compressor.Start(source, target, CompressMode.Decompress);
                }
            }
        }
    }
}