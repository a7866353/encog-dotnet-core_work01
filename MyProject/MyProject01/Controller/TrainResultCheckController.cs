using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MyProject01.Controller.Jobs;

namespace MyProject01.Controller
{
    class TrainResultCheckAsyncController : ICheckJob
    {
        private List<ICheckJob> _jobList;
        private List<TrainerContex> _contextTaskList;
        private Thread _workThread;

        public TrainResultCheckAsyncController()
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
                    try
                    {
                        Thread.Sleep(0);
                    }catch(Exception  e)
                    {
                        System.Console.WriteLine(e.ToString());

                    }
                    continue;
                }

                foreach(ICheckJob job in _jobList)
                {
                    job.Do(context);
                }
                
            }
        }


    }

    class TrainResultCheckSyncController : ICheckJob
    {
        private List<ICheckJob> _jobList;

        public TrainResultCheckSyncController()
        {
            _jobList = new List<ICheckJob>();
        }

        public void Add(ICheckJob job)
        {
            _jobList.Add(job);
        }

        public void Do(TrainerContex context)
        {
            foreach (ICheckJob job in _jobList)
            {
                job.Do(context);
            }
        }
    }

}
