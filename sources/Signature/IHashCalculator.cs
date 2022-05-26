using System;

namespace Signature
{
    public interface IHashCalculator : IDisposable
    {
        void Calculate(string path, long hashBlockSize, 
            Action<IHashCalculator, long, byte[]> onResultOfCalculate, 
            Action<IHashCalculator, long, Exception> onError);
        long MaxNumberBlock();
    }
}