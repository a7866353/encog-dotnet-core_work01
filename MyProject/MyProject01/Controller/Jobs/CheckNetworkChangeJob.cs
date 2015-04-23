using Encog.Neural.NEAT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller.Jobs
{
    class CheckNetworkChangeJob : ICheckJob
    {
        private byte[] LastNetData;
        public void Do(TrainerContex context)
        {
            NEATNetwork episodeNet = (NEATNetwork)context.train.CODEC.Decode(context.train.BestGenome);
            byte[] netData = NetworkToByte(episodeNet);
            if (ByteArrayCompare(netData, LastNetData) == false)
            {
                context.BestNetwork = episodeNet;
                LastNetData = netData;
                context.IsChanged = true;
            }
            else
            {
                context.IsChanged = false;
            }

        }

        private byte[] NetworkToByte(NEATNetwork network)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, network);

            byte[] res = stream.ToArray();
            stream.Close();
            return res;
        }

        private bool ByteArrayCompare(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
                return false;

            if (arr1.Length != arr2.Length)
                return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }
    }
}
