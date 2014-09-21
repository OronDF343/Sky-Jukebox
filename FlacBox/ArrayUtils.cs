﻿using System;

namespace FlacBox
{
    static class ArrayUtils
    {
        internal static T[] CutArray<T>(T[] array, int offset, int count)
        {
            if (count > 0)
            {
                var result = new T[count];
                Array.Copy(array, offset, result, 0, count);
                return result;
            }
            else if (count == 0)
                return new T[0];
            else
                return null;
        }
    }
}
