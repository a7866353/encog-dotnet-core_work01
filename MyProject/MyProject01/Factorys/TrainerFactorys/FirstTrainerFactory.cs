using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;
using MyProject01.Factorys.PopulationFactorys;
using MyProject01.Controller;
using MyProject01.TrainerFactorys;

namespace MyProject01.Factorys.TrainerFactorys
{
    class FirstTrainerFactory : BasicTrainerFactory
    {
        public string TestCaseName;
        public NetworkController Controller;
        public TrainingData TrainingData;
        public BasicPopulationFactory PopulationFacotry;

        protected override Trainer Create()
        {
            NormalTrainer trainer = new NormalTrainer();

            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new UpdataControllerJob(Controller));

            TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            // subCheckCtrl.Add(new UpdateTestCaseJob() 
            mainCheckCtrl.Add(new UpdateTestCaseJob()
            {
                TestName = TestCaseName,
                DecisionCtrl = Controller.GetDecisionController(),
                TestDataBlock = TrainingData.TestDataBlock,
            });

            // mainCheckCtrl.Add(subCheckCtrl);

            trainer.CheckCtrl = mainCheckCtrl;
            trainer.DecisionCtrl = Controller.GetDecisionController();
            trainer.TrainDataBlock = TrainingData.TrainDataBlock;
            trainer.PopulationFacotry = PopulationFacotry;

            return trainer;
        }

        public override string Description
        {
            get { return "NormalTrainer"; }
        }
    }
}
