using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller.Jobs
{
    class UpdataControllerJob : ICheckJob
    {
        private NetworkController _ctrl;
        public UpdataControllerJob(NetworkController ctrl)
        {
            this._ctrl = ctrl;
        }
        public void Do(TrainerContex context)
        {
            if (context.IsChanged == false)
                return;
            _ctrl.BestNetwork = context.BestNetwork;
            _ctrl.Save();
        }
    }
}
