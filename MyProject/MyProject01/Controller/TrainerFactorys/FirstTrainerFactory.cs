using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;

namespace MyProject01.Controller.TrainerFactorys
{
    class FirstTrainerFactory
    {
        public Trainer GetTrainer()
        {
            NormalTrainer trainer = new NormalTrainer();


            TrainResultCheckController checkCtrl = new TrainResultCheckController();
            checkCtrl.Add(new CheckNetworkChangeJob());
            checkCtrl.Add(new UpdataControllerJob());
            checkCtrl.Add(new UpdateTestCaseJob());
            trainer.CheckCtrl = checkCtrl;

            return trainer;
        }
    }
}
