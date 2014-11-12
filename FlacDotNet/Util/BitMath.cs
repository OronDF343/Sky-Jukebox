namespace FlacDotNet.Util
{
    internal static class BitMath
    {
        public static int Ilog2(this int v)
        {
            int l = 0;
            while ((v >>= 1) != 0) l++;
            return l;
        }

        public static int Silog2(this int v)
        {
            while (true)
            {
                if (v == 0) return 0;
                if (v > 0)
                {
                    int l = 0;
                    while (v != 0)
                    {
                        l++;
                        v >>= 1;
                    }
                    return l + 1;
                }
                if (v == -1) return 2;
                v++;
                v = -v;
            }
        }

        public static int Silog2Wide(this long v)
        {
            while (true)
            {
                if (v == 0) return 0;
                if (v > 0)
                {
                    int l = 0;
                    while (v != 0)
                    {
                        l++;
                        v >>= 1;
                    }
                    return l + 1;
                }
                if (v == -1) return 2;
                v++;
                v = -v;
            }
        }
    }
}