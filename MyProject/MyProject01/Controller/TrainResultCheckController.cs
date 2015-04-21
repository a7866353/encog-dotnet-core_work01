using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;

namespace MyProject01.Controller
{
    class TrainResultCheckController : ICheckJob
    {
        private List<ICheckJob> _jobList;
        private List<TrainerContex> _contextTaskList;

        public TrainResultCheckController()
        {
            _jobList = new List<ICheckJob>();
            _contextTaskList = new List<TrainerContex>();
        }

        public void Add(ICheckJob job)
        {
            _jobList.Add(job);
        }

        public void Do(TrainerContex context)
        {
            _contextTaskList.Add(context.Clone());
        }



    }
}
