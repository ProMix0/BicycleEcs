using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BicycleEcs.Internal
{
    internal static class Utils
    {
        public static void EnsureSize<T>(ref T[] array, int size)
        {
            if (size < array.Length) return;

            int newSize = array.Length << 1;
            while (size >= newSize) newSize <<= 1;
            Array.Resize(ref array, newSize);
        }
    }
}
