using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Kontur.LogPacker
{
    //класс представляет контейнер над строкой формата корректного лога
    class Log
    {
        public DateTime DateTime { get; set; }
        public ulong RunTime { get; set; }
        public byte[] Message { get; set; }

        public Log(DateTime dateTime, ulong runTime, byte[] message)
        {
            DateTime = dateTime;
            RunTime = runTime;
            Message = message;
        }

        private const int MIN_LENGHT_LOG = 37;

        //здесь много намертво вшитых констант, все они выбраны исходя из формата корректного лога, это плохо, но производительно
        public static bool TryParse(byte[] line, out Log log)
        {
            log = null;

            if (line.Length >= MIN_LENGHT_LOG)
            {
                if (TryParseDateTime(line, out var dateTime))
                {
                    if (NumberHandler.TryParseNumber(line, 24, out ulong runtime, out var indexRunTimeEnd))
                    {
                        var indexBeginMessage = indexRunTimeEnd + 1 - 24 <= 6 ? 31 : indexRunTimeEnd + 2;

                        if (line[indexBeginMessage - 1] == ' ' && line[indexBeginMessage + 5] == ' ')
                        {
                            var message = new byte[line.Length - indexBeginMessage];
                            Array.Copy(line, indexBeginMessage, message, 0, line.Length - indexBeginMessage);
                            log = new Log(dateTime, runtime, message);
                            return true;
                        }
                    }
                }   
            }
            return false;
        }

        private static bool TryParseDateTime(byte[] line, out DateTime dateTime)
        {
            dateTime = default(DateTime);

            if (line[4] == '-' && line[7] == '-' && line[10] == ' ' && line[13] == ':' && line[16] == ':' && line[19] == ',')
            {
                var hasYear = NumberHandler.TryParseNumber(line, 0, 4, out int year);
                var hasMonth = NumberHandler.TryParseNumber(line, 5, 2, out int month);
                var hasDay = NumberHandler.TryParseNumber(line, 8, 2, out int day);

                var hasHour = NumberHandler.TryParseNumber(line, 11, 2, out int hour);
                var hasMinute = NumberHandler.TryParseNumber(line, 14, 2, out int minute);
                var hasSecond = NumberHandler.TryParseNumber(line, 17, 2, out int second);
                var hasMillisecond = NumberHandler.TryParseNumber(line, 20, 3, out int millisecond);

                var hasDate = hasYear && hasMonth && hasDay;
                var hasTime = hasHour && hasMinute && hasSecond && hasMillisecond;

                if (hasDate && hasTime)
                {
                    try
                    {
                        dateTime = new DateTime(year, month, day, hour, minute, second, millisecond);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public byte[] ToBytes()
        {
            var bytes = new List<byte>(150);
            var dateTime = new byte[23];
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Year, 0, 4, dateTime);
            dateTime[4] = (byte)'-';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Month, 5, 2, dateTime);
            dateTime[7] = (byte)'-';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Day, 8, 2, dateTime);
            dateTime[10] = (byte)' ';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Hour, 11, 2, dateTime);
            dateTime[13] = (byte)':';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Minute, 14, 2, dateTime);
            dateTime[16] = (byte)':';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Second, 17, 2, dateTime);
            dateTime[19] = (byte)',';
            NumberHandler.InsertNumberToBytesArray(this.DateTime.Millisecond, 20, 3, dateTime);

            bytes.AddRange(dateTime);
            bytes.Add((byte)' ');
            bytes.AddRange(NumberHandler.PaddedNumberToBytesArray(this.RunTime, 6, ' '));
            bytes.Add((byte)' ');
            bytes.AddRange(this.Message);
            
            return bytes.ToArray();
        }
    }
}
