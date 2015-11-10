using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.DataSources
{
    public class DataBlock
    {
        static public void Copy(DataBlock src, int srcOffset, DataBlock dst, int dstOffset, int length)
        {
            Array.Copy(src.Data, srcOffset, dst.Data, dstOffset, length);
        }
        public double[] Data;
        public DataBlock(int length)
        {
            Data = new double[length];
        }
        public int Length
        {
            get { return Data.Length; }
        }
        public double this[int index]
        {
            set { Data[index] = value; }
            get { return Data[index]; }
        }
    }
}
