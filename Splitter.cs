using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Kontur.LogPacker
{
    //разбивает байты из стрима на отдельные строки 
    static class Splitter      
    {
        private const int BUFFER_SIZE = 131072; 

        //возможны либо CR+LF, либо LF, у обоих вариантов на конце LF, поэтому считаем его за перевод строки  
        private const int LF = 0x0A;
        
        public static IEnumerable<byte[]> ReadLines(Stream source)
        {
            var buffer = new byte[BUFFER_SIZE];

            var countReadedBytes = source.Read(buffer, 0, BUFFER_SIZE);

            //при увиличении количества элементов в листе, его реализация проводит копирование в новый массив когда число 
            //элементов доходит до Capacity, чтобы минимизировать копирование, лучше сразу установить Capacity побольше
            var line = new List<byte>(200); 
            
            while (countReadedBytes != 0)
            {
                for (int i = 0; i != countReadedBytes; i++)
                {
                    line.Add(buffer[i]);

                    if (buffer[i] == LF)
                    {
                        yield return line.ToArray();
                        line.Clear();
                    }
                }
                
                //если в буфере не было переводов строки,то весь буфер считаем строкой
                if (line.Count == countReadedBytes)
                {
                    yield return line.ToArray();
                    line.Clear();
                }

                countReadedBytes = source.Read(buffer, 0, BUFFER_SIZE);

                //в конце считывания в line может что-нибудь остаться, нужно передать эти байты на обработку
                if (countReadedBytes == 0 && line.Count!=0)
                    yield return line.ToArray();
            }
        }
    }
}
