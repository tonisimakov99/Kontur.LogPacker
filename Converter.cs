using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.IO.Compression;

namespace Kontur.LogPacker
{
    public enum ConvertMode
    {
        Convert,
        Deconvert
    }

    internal static class Converter
    {
        //здесь реализована логика алгоритма
        //Сжатие:
        //первая строка в нужном формате опорная, относительно нее все считается и она просто переписывается как есть
        //затем берется две соседних строки, если текущая строка больше или равна предыдущей, берется разность дат и времени работы
        //и записывается в новую строку, если же нет, то текущая строка считается опорной, 
        //строки которые не подходят под формат лога, просто переписываются как есть 
        //Разжатие:
        //В целом аналогично сжатию, но в обратную сторону, из двух строк одна уже была переведена в исходное состояние или она опорная
        //к ней прибавляется разница дат и времени работы записанная в следующей строке
        public static void Convert(Stream source, Stream target, ConvertMode mode)
        {
            Log previous = null;
            var isFirstLine = true;

            foreach (var line in Splitter.ReadLines(source))
            {
                if (isFirstLine)                                      
                {
                    if (Log.TryParse(line, out var current))         
                    {
                        Connector.Write(target, current.ToBytes());
                        previous = current;
                        isFirstLine = false;
                    }
                    else
                        Connector.Write(target, line);
                }
                else
                {
                    if (mode == ConvertMode.Convert)
                        Convert(line);
                    else
                        Deconvert(line);
                }
            }

            Connector.Write(target, new byte[0]);

            void Convert(byte[] line)
            {
                if (Log.TryParse(line, out var current))
                {
                    if ((current.DateTime >= previous.DateTime) && (current.RunTime >= previous.RunTime))
                    {
                        var currentCompressed = new CompressedLog((current.DateTime - previous.DateTime).TotalMilliseconds,
                                                                   (current.RunTime - previous.RunTime),
                                                                   current.Message);
                        Connector.Write(target, currentCompressed.ToBytes());
                    }
                    else
                        Connector.Write(target, current.ToBytes());

                    previous = current;
                }
                else
                    Connector.Write(target, line);
            }

            void Deconvert(byte[] line)
            {
                if (CompressedLog.TryParse(line, out var currentCompressed))
                {
                    var currentUncompressed = new Log(previous.DateTime.AddMilliseconds(currentCompressed.DeltaDateTime),
                                                      previous.RunTime + currentCompressed.DeltaRunTime,
                                                      currentCompressed.Message);
                    Connector.Write(target, currentUncompressed.ToBytes());
                    previous = currentUncompressed;
                }
                else
                {
                    if (Log.TryParse(line, out var current))
                        previous = current;
                    Connector.Write(target, line);
                }
            }
        }
    }
}
