using System;
using System.IO;

namespace Signature
{
    internal class LargeBlockStream : Stream
    {
        private FileStream _fileStream;
        private readonly long _hashBlockSize;
        private long _length;
        private readonly long _maxNumberBlock;

        public LargeBlockStream (string path, long hashBlockSize)
        {
            _hashBlockSize = hashBlockSize;
            _fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

            if (_fileStream.Length == 0)
                throw new Exception($"File size 0. : {path}");

            _maxNumberBlock = (_fileStream.Length - 1) / _hashBlockSize;
        }

        public long? Next()
        {
            var numberOfBlock = _fileStream.Position / _hashBlockSize;

            if (_fileStream.Position == _fileStream.Length)
                return null;
            
            Position = 0;

            if (numberOfBlock < _maxNumberBlock)
                _length = _hashBlockSize;
            else
                _length = _fileStream.Length - _maxNumberBlock * _hashBlockSize;

            return numberOfBlock;
        }
        
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var available = Length - Position;
            
            if (available == 0)
                return 0;
            
            var read = _fileStream.Read(buffer, offset, count);
            Position += read;

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        
        public new void Dispose()
        {
            _fileStream.Dispose();
            _fileStream = null;
        }
        
        public long MaxNumberBlock()
        {
            return _maxNumberBlock;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get; set; }
    }
}