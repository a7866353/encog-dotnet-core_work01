using MyProject01.Controller.TrainerFactorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TrainerFactorys
{
    class NormalTrainerFactory : BasicTrainerFactory
    {
        protected override Controller.Trainer Create()
        {
            return null;
        }
    }
}
