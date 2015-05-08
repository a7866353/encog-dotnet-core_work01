using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.ControllerFactorys
{
    abstract class BasicControllerFactory : IControllerFactory
    {
        protected string _name;
        protected int _inputLength;
 
        public int InputLength
        {
            get
            {
                return _inputLength;
            }
            set
            {
                _inputLength = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public NetworkController Get()
        {
            NetworkController controller = NetworkController.Open(Name);
            if( controller == null)
            {
                controller = Create();
            }
            return controller;
        }
        abstract protected NetworkController Create();
        abstract public string GetDesc();
    }
}
