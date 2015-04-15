using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller.Jobs
{
    interface ICheckJob
    {
        void Do(TrainerContex context);
    }
}
