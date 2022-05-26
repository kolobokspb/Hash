using System;
using System.Collections.Generic;

namespace Signature
{
    public class Logger
    {
        private readonly PriorityQueue<object, long> _messages = new PriorityQueue<object, long>();
        private long _previousIndex = -1;

        private void WriteAllMessages()
        {
            while (true)
            {
                _messages.TryPeek(out var data, out var index);

                if (_previousIndex + 1 != index)
                    break;

                _messages.Dequeue();

                _previousIndex++;
                
                Console.Write($"number: {index}   ");
                
                if (data is byte[] sha)
                {
                    PrintByteArray(sha);
                    Console.WriteLine();
                }
                else if (data is Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }        
        }

        public void Write(long number, Exception ex)
        {
            lock(_messages)
            {
                _messages.Enqueue(ex, number);
                WriteAllMessages();
            }
        }

        public void Write(long number, byte[] hash)
        {
            lock (_messages)
            {
                _messages.Enqueue(hash, number);
                WriteAllMessages();
            }
        }

        private static void PrintByteArray(byte[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                Console.Write($"{array[i]:X2}");
                if (i % 4 == 3) 
                    Console.Write(" ");
            }
        }
    }
}