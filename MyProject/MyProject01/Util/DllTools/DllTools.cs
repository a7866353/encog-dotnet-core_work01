using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject01.Util.DllTools
{
     [StructLayout(LayoutKind.Sequential)]
    struct FWTParam
	{
		public IntPtr input;
        public IntPtr output;
        public IntPtr temp;
        public int inputLength;

        public IntPtr h;
        public IntPtr g;
        public int filterLength;
	};   
    class DllTools
    {
        private static double[] h = new double[]{.332670552950, .806891509311, .459877502118, -.135011020010,   
                    -.085441273882, .035226291882};
        private static double[] g = new double[]{.035226291882, .085441273882, -.135011020010, -.459877502118,  
                    .806891509311, -.332670552950};
        public static void FTW(double[] input, double[] output, double[] temp)
        {
            int level = 1;
            // Calculate level;
            while (true)
            {
                if (Math.Pow(2, level) > input.Length)
                    break;
                level++;
            }
            level--;

            lock (h)
            {
                if (false)
                {
                    FWTParam param;
                    // param.input = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);
                    // param.output = Marshal.UnsafeAddrOfPinnedArrayElement(output, 0);
                    // param.temp = Marshal.UnsafeAddrOfPinnedArrayElement(temp, 0);
                    param.inputLength = input.Length;
                    int len = Marshal.SizeOf(typeof(double)) * input.Length;
                    param.input = Marshal.AllocHGlobal(len);
                    Marshal.Copy(input, 0, param.input, len);

                    param.output = Marshal.AllocHGlobal(len);
                    Marshal.Copy(output, 0, param.output, len);

                    param.temp = Marshal.AllocHGlobal(len);
                    Marshal.Copy(temp, 0, param.temp, len);

                    param.h = Marshal.UnsafeAddrOfPinnedArrayElement(h, 0);
                    param.g = Marshal.UnsafeAddrOfPinnedArrayElement(g, 0);
                    param.filterLength = h.Length;

                    DllTools_DWT1D(ref param);

                    Marshal.FreeHGlobal(param.input);
                    Marshal.FreeHGlobal(param.output);
                    Marshal.FreeHGlobal(param.temp);
                }
                else
                {
                    FWTParam param;
                    param.input = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);
                    param.output = Marshal.UnsafeAddrOfPinnedArrayElement(output, 0);
                    param.temp = Marshal.UnsafeAddrOfPinnedArrayElement(temp, 0);
                    param.inputLength = input.Length;

                    param.h = Marshal.UnsafeAddrOfPinnedArrayElement(h, 0);
                    param.g = Marshal.UnsafeAddrOfPinnedArrayElement(g, 0);
                    param.filterLength = h.Length;

                    DllTools_DWT1D(ref param);
                }
            }
            return;
         }

        public static void FTW_2(double[] input, double[] output, double[] temp)
        {
            FWTParam param;
            param.temp = IntPtr.Zero;
            param.h = IntPtr.Zero;
            param.g = IntPtr.Zero;
            param.filterLength = 0;

#if false // non copy
            param.input = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);
            param.output = Marshal.UnsafeAddrOfPinnedArrayElement(output, 0);
            param.inputLength = input.Length;
            DllTools_DWT1D_V2(ref param);

#else
            param.inputLength = input.Length;
            int len = Marshal.SizeOf(typeof(double)) * input.Length;
            param.input = Marshal.AllocHGlobal(len);
            Marshal.Copy(input, 0, param.input, input.Length);

            param.output = Marshal.AllocHGlobal(len);

            DllTools_DWT1D_V2(ref param);
            Marshal.Copy(param.output, output, 0, output.Length);

            Marshal.FreeHGlobal(param.input);
            Marshal.FreeHGlobal(param.output);

#endif

        }

        public static void FTW_3(double[] input, double[] output, double[] temp)
        {
            IntPtr signal, dataOut;
            signal = Marshal.UnsafeAddrOfPinnedArrayElement(input, 0);
            dataOut = Marshal.UnsafeAddrOfPinnedArrayElement(output, 0);

            OneDFwt(signal, (uint)input.Length, dataOut);
            
        }

        [DllImport("DllTools.dll", EntryPoint = "DllTools_DWT1D")]
        private static extern void DllTools_DWT1D(ref FWTParam param);

        [DllImport("DllTools.dll", EntryPoint = "DllTools_DWT1D_V2")]
        private static extern void DllTools_DWT1D_V2(ref FWTParam param);

        [DllImport("dwtHaar1D.dll", EntryPoint = "OneDFwt")]
        private static extern void OneDFwt(IntPtr signal, uint slength, IntPtr output);
    }
}
