using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller
{
    interface ISensor
    {
        int CurrentPosition { get; set; }
        int SkipCount { get; }
        int TotalLength { get; }
        int DataLength { get; }
        int Copy(double[] buffer, int startIndex);
    }
    class SensorGroup : List<ISensor>, ISensor
    {
        private int _currentPosition;
        public int CurrentPosition
        {
            set
            {
                int pos = Math.Min(value, TotalLength);
                _currentPosition = pos;
                foreach(ISensor sen in this)
                {
                    sen.CurrentPosition = pos;
                }

            }
            get
            {
                return _currentPosition;
            }
        }
        public int SkipCount
        {
            get
            {
                int maxValue = this[0].SkipCount;
                foreach(ISensor sen in this)
                {
                    maxValue = Math.Max(sen.SkipCount, maxValue);
                }
                return maxValue;
            }
        }

        public int TotalLength
        {
            get
            {
                int minValue = this[0].SkipCount;
                foreach (ISensor sen in this)
                {
                    minValue = Math.Min(sen.SkipCount, minValue);
                }
                return minValue;

            }
        }

        public int DataLength
        {
            get
            {
                int sum = 0;
                foreach (ISensor sen in this)
                {
                    sum += sen.DataLength;
                }
                return sum;
            }
        }
        public int Copy(double[] buffer, int startIndex)
        {
            int index = 0;
            foreach(ISensor sen in this)
            {
                sen.Copy(buffer, index);
                index += sen.DataLength;
            }
            return index;
        }
    }

}
