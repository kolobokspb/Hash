using System;

namespace Signature
{
    internal sealed class NormalBlockHashWorker : ThreadWorker
    {
        private Feeder _feeder;
        private NormalBlockStream _normalBlockStream;

        public NormalBlockHashWorker(Action<ThreadWorker> calculate, Feeder feeder) : base(calculate)
        {
            _feeder = feeder;
        }

        public void SetStream(NormalBlockStream normalBlockStream)
        {
            _normalBlockStream = normalBlockStream;
            WakeUp();
        } 
        
        public NormalBlockStream GetStream()
        { 
            return _normalBlockStream;
        }

        public void ComputeHash()
        {
            _normalBlockStream.Hash = _feeder.ComputeHash(_normalBlockStream);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _feeder?.Dispose();
            _feeder = null;
        }
    }
}