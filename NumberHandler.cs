using System;
using System.Collections.Generic;
using System.Text;

namespace Kontur.LogPacker
{
    //.Net содержит множество методов чтобы получить число из байтов или, наоборот, перевести в байты, но все эти способы работают 
    //в общем случае, что заставляет их работать дольше, здесь некоторые методы и их перегрузки для парсинга чисел для данной задачи, все они используют кодировку UTF-8
    public static class NumberHandler
    {
        const int MAX_INT_LENGTH = 10;
        const int MAX_ULONG_LENGTH = 20;
        const int DELTA_UTF_8 = 48;

        public static bool TryParseNumber(byte[] line, int indexBegin, int length, out int number)
        {
            number = 0;
            for (int i = indexBegin; i != indexBegin + length; i++)
            {
                if (IsNumber(line[i]))
                {
                    number = number * 10 + line[i] - DELTA_UTF_8;
                }
                else
                    return false;
            }
            return true;
        }

        public static bool TryParseNumber(byte[] line, int indexBegin, int length, out ulong number)
        {
            number = 0;
            for (int i = indexBegin; i != indexBegin + length; i++)
            {
                if (IsNumber(line[i]))
                {
                    number = number * 10 + line[i] - DELTA_UTF_8;
                }
                else
                    return false;
            }
            return true;
        }
        
        public static bool TryParseNumber(byte[] line, int indexBegin, out int number, out int indexEnd)
        {
            number = 0;
            
            var i = indexBegin;
            while (IsNumber(line[i]))
            {
                number = number * 10 + line[i] - DELTA_UTF_8;
                i++;
            }

            int length = i - indexBegin;

            indexEnd = i-1;

            if ((length > MAX_INT_LENGTH) || (length == 0))
                return false;

            return true;
        }

        public static bool TryParseNumber(byte[] line, int indexBegin, out ulong number, out int indexEnd)
        {
            number = 0;

            var i = indexBegin;
            while (IsNumber(line[i]))
            {
                number = number * 10 + line[i] - DELTA_UTF_8;
                i++;
            }

            int length = i - indexBegin;

            indexEnd = i-1;

            if ((length > MAX_INT_LENGTH) || (length == 0))
                return false;

            return true;
        }

        public static void InsertNumberToBytesArray(int number, int index, int length, byte[] line)
        {
            for (int i = 0; i != length; i++)
            {
                line[index + length - i - 1] = (byte)((number % 10) + 48);
                number /= 10;
            }
        }

        public static byte[] PaddedNumberToBytesArray(ulong value, int totalWidth, char paddingChar)
        {
            var bytes = new List<byte>();

            while (value != 0)
            {
                DivideBy10WithRemainder(value, out value, out var remainder);
                bytes.Add((byte)(remainder + DELTA_UTF_8));
            }

            bytes.Reverse();

            if (bytes.Count == 0)
                bytes.Add(0 + DELTA_UTF_8);

            if (bytes.Count < totalWidth)
            {
                var count = bytes.Count;
                for (var i = 0; i != totalWidth - count; i++)
                    bytes.Add((byte)paddingChar);
            }
            return bytes.ToArray();
        }

        public static byte[] NumberToBytesArray(ulong value)
        {
            var bytes = new List<byte>();

            while (value != 0)
            {
                DivideBy10WithRemainder(value, out value, out var remainder);
                bytes.Add((byte)(remainder + DELTA_UTF_8));
            }

            bytes.Reverse();

            if (bytes.Count == 0)
                bytes.Add(0 + DELTA_UTF_8);

            return bytes.ToArray();
        }

        private static bool IsNumber(byte digit)
        {
            return DELTA_UTF_8 <= digit && digit <= DELTA_UTF_8 + 10 - 1;
        }

        private static void DivideBy10WithRemainder(ulong divident, out ulong quotient, out ulong remainder)
        {
            quotient = (ulong)(divident * 0.1);
            remainder = divident - quotient * 10;
        }

    }
}
