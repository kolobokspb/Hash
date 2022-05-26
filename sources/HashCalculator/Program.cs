using System;
using System.Collections.Generic;
using System.Threading;
using Signature;

namespace HashCalculator
{
    internal static class Program
    {
        private static string GetPath(IReadOnlyList<string> args)
        {
            if (args.Count == 0)
                throw new Exception("File path not set!");

            return args[0];
        }

        private static long GetBlockSize(IReadOnlyList<string> args)
        {
            var blockSize = (long)1 << 24; //default 8Mb
            const long minBlockSize = (long)1 << 0; //1B
            const long maxBlockSize = (long)1 << 35; //32GB
            
            if (args.Count > 1)
            {
                try
                {
                    blockSize = long.Parse(args[1]);
                }
                catch(Exception ex)
                {
                    throw new Exception("Block size is not a number.", ex);
                }        
            }
            
            if ( blockSize < minBlockSize || blockSize > maxBlockSize) 
                throw new Exception("Block size out of range [1B-32GB].");

            return blockSize;
        }
        
        private static void Main(string[] args)
        {
            var wait = new AutoResetEvent(false);
        
            Console.CancelKeyPress += delegate { wait.Set(); };

            Console.WriteLine("This is a signature calculate program!");

            try
            {            
                var path = GetPath(args);
                var blockSize = GetBlockSize(args);

                var logger = new Logger(); 
                
                const long largeBlockSize = (long)1 << 30; //1GB
                
                using IHashCalculator calculator = blockSize > largeBlockSize ? 
                    new LargeBlockHashCalculator<FeederSha256> (): 
                    new NormalBlockHashCalculator<FeederSha256>();
                
                calculator.Calculate(path, blockSize,
                (hashCalculator, number, hash) =>
                {
                    logger.Write(number, hash);
                    if (number == hashCalculator.MaxNumberBlock())
                        wait.Set();
                },
                (_, number, ex) =>
                { 
                    logger.Write(number, ex); 
                    wait.Set();
                });
                
                wait.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            Console.WriteLine("Program completed.");
        }
    }
}