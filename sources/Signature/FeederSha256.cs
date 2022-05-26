using System.IO;
using System.Security.Cryptography;

namespace Signature
{
    public sealed class FeederSha256 : Feeder
    {
        private SHA256 _sha;

        public FeederSha256()
        {
            _sha = SHA256.Create();
        }

        public override byte[] ComputeHash(Stream stream)
        {
            return _sha.ComputeHash(stream);
        }

        public override void Dispose()
        {
            _sha.Dispose();
            _sha = null;
        }
    }
}