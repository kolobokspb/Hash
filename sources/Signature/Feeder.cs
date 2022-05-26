using System;
using System.IO;

namespace Signature
{
    public abstract class Feeder : IDisposable
    {
        public abstract byte[] ComputeHash(Stream stream);
        public abstract void Dispose();
    }
}