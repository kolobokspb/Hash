using System;
using System.Threading;

namespace Signature
{
    internal class ThreadWorker : IDisposable
    {
        private Thread _thread;
        private AutoResetEvent _wait = new AutoResetEvent(false);
        private readonly Action<ThreadWorker> _calculate;
        private bool _dispose;

        public ThreadWorker(Action<ThreadWorker> calculate)
        {
            _calculate = calculate;
            _thread = new Thread(ThreadFunction);
            _thread.Start();
        }

        public virtual void Dispose()
        {
            _dispose = true;
            
            _wait.Set();
            
            _thread?.Join();
            _thread = null;
            
            _wait?.Dispose();
            _wait = null;
        }
        
        private void ThreadFunction()
        {
            while (true)
            {
                _wait.WaitOne();

                if(_dispose)
                    break;
                
                _calculate?.Invoke(this);
            }        
        }

        public void WakeUp()
        {
            _wait.Set();
        } 
    }
}