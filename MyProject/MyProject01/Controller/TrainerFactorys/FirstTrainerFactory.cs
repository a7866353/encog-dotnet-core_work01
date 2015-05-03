using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;
using MyProject01.PopulationFactorys;

namespace MyProject01.Controller.TrainerFactorys
{
    class FirstTrainerFactory
    {
        public string TestCaseName;
        public NetworkController Controller;
        public TrainingData TrainingData;
        public BasicPopulationFactory PopulationFacotry = new NormalPopulationFactory();
        public Trainer GetTrainer()
        {
            NormalTrainer trainer = new NormalTrainer();

            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new UpdataControllerJob());

            TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            subCheckCtrl.Add(new UpdateTestCaseJob() 
            { 
                TestName = TestCaseName, 
                DecisionCtrl = Controller.GetDecisionController(), 
                TestDataBlock = TrainingData.TestDataBlock,
            });

            mainCheckCtrl.Add(subCheckCtrl);
            trainer.CheckCtrl = mainCheckCtrl;
            trainer.DecisionCtrl = Controller.GetDecisionController();
            trainer.TrainDataBlock = TrainingData.TrainDataBlock;
            trainer.PopulationFacotry = PopulationFacotry;

            return trainer;
        }
    }
}
