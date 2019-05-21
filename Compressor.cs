using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO.Compression;

namespace Kontur.LogPacker
{
    public enum CompressMode
    {
        Compress,
        Decompress
    }

    internal static class Compressor
    {        
        public static void Start(FileStream source, FileStream target, CompressMode mode)
        {
            if (mode == CompressMode.Compress)
            {
                using (GZipStream gzipStream = new GZipStream(target, CompressionLevel.Optimal, true))
                {
                    Converter.Convert(source, gzipStream, ConvertMode.Convert);
                }
            }
            else
            {
                using (var gzipStream = new GZipStream(source, CompressionMode.Decompress, true))
                {
                    Converter.Convert(gzipStream, target, ConvertMode.Deconvert);
                }
            }
        }
    }
}
