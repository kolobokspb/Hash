using System;

namespace Signature
{
    public class NormalBlockHashCalculator<T>: IHashCalculator where T : Feeder, new()
    {
        private NormalBlockFileReader _normalBlockFileReader;
        private NormalBlockDispatcher<T> _normalBlockDispatcher;
        private Action<IHashCalculator, long, byte[]> _onResultOfCalculate;
        private Action<IHashCalculator, long, Exception> _onError;
        
        public void Calculate(string path, long hashBlockSize, 
            Action<IHashCalculator, long, byte[]> onResultOfCalculate, 
            Action<IHashCalculator, long, Exception> onError)
        {
            if ( hashBlockSize < GetMinBlockSize() || hashBlockSize > GetMaxBlockSize()) 
                throw new Exception($"Block size out of range [{GetMinBlockSize()}B-{GetMaxBlockSize()}GB].");
            
            _onResultOfCalculate = onResultOfCalculate;
            _onError = onError;
            
            var processorCount = Environment.ProcessorCount;

            _normalBlockFileReader = new NormalBlockFileReader(path, hashBlockSize, processorCount, 
                (reader, number, ex) => {_onError?.Invoke(this, number, ex);});
            _normalBlockFileReader.StartReader();
            
            _normalBlockDispatcher = new NormalBlockDispatcher<T>(_normalBlockFileReader, processorCount, 
                (number, hash) => {_onResultOfCalculate?.Invoke(this, number, hash);});
            _normalBlockDispatcher.StartDispatcher();
        }

        public long GetMaxNumberBlock() { return _normalBlockFileReader.MaxNumberBlock(); }

        public long GetMinBlockSize() { return (long)1 << 0; /*1B*/}

        public long GetMaxBlockSize() { return (long)1 << 30; /*1GB*/}
        
        public void Dispose()
        {
            _normalBlockDispatcher?.Dispose();
            _normalBlockDispatcher = null;
            _normalBlockFileReader?.Dispose();
            _normalBlockFileReader = null;
        }
    }
}