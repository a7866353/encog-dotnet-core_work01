using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;
using System.Threading;

namespace MyProject01.Controller
{
    class TrainResultCheckController : ICheckJob
    {
        private List<ICheckJob> _jobList;
        private List<TrainerContex> _contextTaskList;
        private Thread _workThread;

        public TrainResultCheckController()
        {
            _jobList = new List<ICheckJob>();
            _contextTaskList = new List<TrainerContex>();

            _workThread = new Thread(new ThreadStart(WorkTask));
            _workThread.Start();
        }

        public void Add(ICheckJob job)
        {
            _jobList.Add(job);
        }

        public void Do(TrainerContex context)
        {
            lock (_contextTaskList)
            {
                _contextTaskList.Add(context.Clone());
            }
            _workThread.Interrupt();
        }

        private void WorkTask()
        {
            while(true)
            {
                TrainerContex context;
                lock(_contextTaskList)
                {
                    if (_contextTaskList.Count == 0)
                        context = null;
                    else
                    {
                        context = _contextTaskList[0];
                        _contextTaskList.RemoveAt(0);
                    }
                }

                if( context == null )
                {
                    Thread.Sleep(0);
                    continue;
                }

                foreach(ICheckJob job in _jobList)
                {
                    job.Do(context);
                }
                
            }
        }


    }
}
