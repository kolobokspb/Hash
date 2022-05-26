using System;
using System.IO;

namespace Signature
{
    internal class NormalBlockStream : Stream
    {
        public long Number;
        public readonly byte[] Buffer;
        public byte[] Hash;
        private long _length;
        
        public NormalBlockStream (long size)
        {
            Buffer = new byte[size];
        }

        public void Initialize(long number, long length)
        {
            Number = number;
            _length = length;
            Position = 0;
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

            count = count > available ? (int)available : count; 
            
            var span = new Span<byte>(Buffer, (int) Position, count);
            span.CopyTo(new Span<byte>(buffer, offset, count));
            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get; set; }
    }
}