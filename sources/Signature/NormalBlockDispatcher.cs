using System;
using System.Collections.Generic;

namespace Signature
{
    internal class NormalBlockDispatcher<T> : IDisposable where T : Feeder, new()
    {
        private readonly Action<long, byte[]> _onResultOfCalculate;
        private NormalBlockFileReader _fileReader;
        private ThreadWorker _jobDistributor ;
        private readonly int _maxJobs;
        private readonly Queue<NormalBlockHashWorker> _freeWorkers = new Queue<NormalBlockHashWorker>();
        private readonly List<NormalBlockHashWorker> _allWorkers = new List<NormalBlockHashWorker>();

        public NormalBlockDispatcher(NormalBlockFileReader reader, int maxJobs, Action<long, byte[]> onResultOfCalculate)
        {
            _onResultOfCalculate = onResultOfCalculate;
            _fileReader = reader;
            _maxJobs = maxJobs;
        }

        public void StartDispatcher()
        {
            for (var i = 0; i != _maxJobs; i++)
            {
                var workers = new NormalBlockHashWorker(HashCalculator, new T());
                lock (_freeWorkers)
                    _freeWorkers.Enqueue(workers);
                _allWorkers.Add(workers);
            }

            _jobDistributor = new ThreadWorker(AssignJob);
            _jobDistributor.WakeUp();
            
            _fileReader.OnFilledStream += WakeUp;
        }

        private NormalBlockHashWorker GetFreeHashWorker()
        {
            lock (_freeWorkers)
                return _freeWorkers.Dequeue();
        }
        
        private int GetFreeHashWorkersCount()
        {
            lock (_freeWorkers)
                return _freeWorkers.Count;
        }
        private void AssignJob(ThreadWorker owner)
        {
            if (GetFreeHashWorkersCount() == 0)
                return;
            
            var stream = _fileReader.GetFilledStream();

            if (stream == null)
                return;
            
            var freeHashWorker = GetFreeHashWorker();
            
            freeHashWorker.SetStream(stream);
        }

        private void ReleaseStream(NormalBlockStream stream)
        {
            _onResultOfCalculate?.Invoke(stream.Number, stream.Hash);
            _fileReader?.ReleaseFilledStream(stream);
            WakeUp();
        }

        private void ReleaseWorker(NormalBlockHashWorker threadWorker)
        {
            lock (_freeWorkers) 
                _freeWorkers.Enqueue(threadWorker);        
        }
        
        private void HashCalculator(ThreadWorker owner)
        {
            var worker = (NormalBlockHashWorker) owner;
            
            worker.ComputeHash();
            
            ReleaseStream(worker.GetStream());
            ReleaseWorker(worker);
        }

        private void WakeUp()
        {
            _jobDistributor?.WakeUp();        
        }

        public void Dispose()
        {
            if(_fileReader != null) 
                _fileReader.OnFilledStream -= WakeUp;
            
            _jobDistributor?.Dispose(); 
            _jobDistributor = null;
             
            for (var i = 0; i != _allWorkers.Count; i++)
                _allWorkers[i].Dispose();
            _allWorkers.Clear();
            
            _fileReader = null;
        }
    }
}