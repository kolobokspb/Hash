using System;

namespace Signature
{
    public class LargeBlockHashCalculator<T> : IHashCalculator where T : Feeder, new()
    {
        //https://www.merriam-webster.com/dictionary/hasher
        private ThreadWorker _hasher; 
        private Action<IHashCalculator, long, byte[]> _onResultOfCalculate;
        private Action<IHashCalculator, long, Exception> _onError;
        private LargeBlockStream _stream;
        private bool _errorWhileReading;
        private Feeder _feeder;
        
        public void Calculate(string path, long hashBlockSize, 
            Action<IHashCalculator, long, byte[]> onResultOfCalculate, 
            Action<IHashCalculator, long, Exception> onError)
        {
            _onResultOfCalculate = onResultOfCalculate;
            _onError = onError;
            _feeder = new T();
            _stream = new LargeBlockStream(path, hashBlockSize);
            _hasher = new ThreadWorker(Feed);
            _hasher.WakeUp();
        }

        public long MaxNumberBlock()
        {
            return _stream.MaxNumberBlock();    
        }
        
        private void Feed(ThreadWorker owner)
        {
            if(_errorWhileReading)
                return; 
            
            var blockNumber = _stream.Next();
            if (blockNumber == null)
                return;

            byte[] hash;
            try
            {
                hash = _feeder.ComputeHash(_stream);
            }
            catch (Exception ex)
            {
                _errorWhileReading = true;
                _onError?.Invoke(this, blockNumber.Value, ex);
                return;
            }
            
            _onResultOfCalculate.Invoke(this, blockNumber.Value , hash);
            _hasher.WakeUp();
        }

        public void Dispose()
        {
            _hasher.Dispose();
            _hasher = null;
            
            _stream?.Dispose();
            _stream = null;
            
            _feeder?.Dispose();
            _feeder = null;
        }
    }
}