using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TrainerFactorys
{
    abstract class BasicTrainerFactory
    {
        public string Name;

        public Trainer Get()
        {
            return Create();
        }

        abstract protected Trainer Create();
    }
}
