using Encog.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    [Serializable]
    public class ControllerPacker
    {
        private ISensor _sensor;
        private IActor _actor;
        private IMLRegression _neuroNetwork;

        static public ControllerPacker FromBinary(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            ControllerPacker obj = (ControllerPacker)formatter.Deserialize(stream);
            return obj;
        }

        public ControllerPacker(ISensor sensor, IActor actor, IMLRegression net)
        {
            _sensor = sensor;
            _actor = actor;
            _neuroNetwork = net;
        }

        public byte[] GetData()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            byte[] data = stream.ToArray();
            stream.Close();
            return data;
        }

        public BasicController GetController()
        {
            BasicController ctrl = new BasicController(_sensor, _actor);
            ctrl.UpdateNetwork(_neuroNetwork);
            return ctrl;
        }


    }
}
