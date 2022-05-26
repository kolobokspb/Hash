using System;
using System.Collections.Generic;
using System.IO;

namespace Signature
{
    internal class NormalBlockFileReader : IDisposable
    {
        public event Action OnFilledStream;
        private readonly long _hashBlockSize;
        private FileStream _fileStream;
         
        private ThreadWorker _reader;
        private readonly string _path;
        
        private readonly int _waitingQueueLength;
        private bool _errorWhileReading;
        
        private long _maxNumberBlock;
        private readonly Action<NormalBlockFileReader, long, Exception> _onError;
        
        private readonly Queue<NormalBlockStream> _freeStreams = new Queue<NormalBlockStream>();
        private readonly Queue<NormalBlockStream> _filledStreams = new Queue<NormalBlockStream>();
        
        public NormalBlockFileReader (string path, long hashBlockSize, int waitingQueueLength, Action<NormalBlockFileReader, long, Exception> onError)
        {
            _onError = onError;
            _hashBlockSize = hashBlockSize;
            _waitingQueueLength = waitingQueueLength;
            _path = path;
            
        }
        public void StartReader()
        {
            int NoBuffering = 0;
            //int NoBuffering = 0x20000000;
            _fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | (FileOptions)NoBuffering);
            
            if (_fileStream.Length == 0)
                throw new Exception($"File size 0. : {_path}");
            
            _maxNumberBlock = (_fileStream.Length - 1) / _hashBlockSize;
            _reader = new ThreadWorker(ReadFile);
            _reader.WakeUp();
        }

        private NormalBlockStream GetFreeBlockStream()
        {
            lock (_freeStreams)
            {
                if (_freeStreams.Count != 0)
                    return _freeStreams.Dequeue();
            }
            
            return new NormalBlockStream(_hashBlockSize);
        }

        public NormalBlockStream GetFilledStream()
        {
            NormalBlockStream normalBlockStream = null;
            
            lock (_filledStreams)
            {
                if (_filledStreams.Count != 0)
                    normalBlockStream = _filledStreams.Dequeue();
            }

            return normalBlockStream;
        }

        public void ReleaseFilledStream(NormalBlockStream normalBlockStream)
        {
            lock (_freeStreams)
                _freeStreams.Enqueue(normalBlockStream);
            _reader.WakeUp();
        }
        
        private void ReadBlock(NormalBlockStream stream)
        {
            var blockNumber = _fileStream.Position / _hashBlockSize;
            var readBytes = 0;
            
            while (true)
            {
                int read;
                try
                {
                    read = _fileStream.Read(stream.Buffer, readBytes, stream.Buffer.Length - readBytes);
                }
                catch (Exception ex)
                {
                    _errorWhileReading = true;
                    ReleaseFilledStream(stream);
                    _onError.Invoke(this, blockNumber, new Exception($"Error while reading file. : {_path}", ex));
                    return;
                }

                readBytes += read;
                if (read == 0 || readBytes == stream.Buffer.Length)
                    break;
            }

            stream.Initialize(blockNumber, readBytes);

            lock (_filledStreams)
                _filledStreams.Enqueue(stream);
        }

        private bool IsEndFile()
        {
            return _fileStream.Position == _fileStream.Length;
        }

        private int GetFullBlockCount()
        {
            lock (_filledStreams)
                return _filledStreams.Count;
        }
        
        private void ReadFile(ThreadWorker owner)
        {
            while (GetFullBlockCount() < _waitingQueueLength && !IsEndFile() && !_errorWhileReading) 
            { 
                var stream = GetFreeBlockStream(); 
                ReadBlock(stream);
                OnFilledStream?.Invoke();
            }        
        }
        
        public long MaxNumberBlock()
        {
            return _maxNumberBlock;
        }
        
        public void Dispose()
        {
            _reader?.Dispose(); 
            _reader = null;
            
            _fileStream?.Dispose();
            _fileStream = null;
        }
    }
}