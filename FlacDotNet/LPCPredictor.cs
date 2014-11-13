namespace FlacDotNet
{
    internal static class LpcPredictor
    {
        public static void RestoreSignal(int[] residual, int dataLen, int[] qlpCoeff, int order, int lpQuantization,
                                         ref int[] data, int startAt)
        {
            //System.out.println("Q="+lpQuantization);
            for (int i = 0; i < dataLen; i++)
            {
                int sum = 0;
                for (int j = 0; j < order; j++)
                {
                    sum += qlpCoeff[j]*data[startAt + i - j - 1];
                }
                //System.out.print((sum >> lpQuantization)+" ");
                data[startAt + i] = residual[i] + (sum >> lpQuantization);
            }
        }

        public static void RestoreSignalWide(int[] residual, int dataLen, int[] qlpCoeff, int order, int lpQuantization,
                                             ref int[] data, int startAt)
        {
            for (int i = 0; i < dataLen; i++)
            {
                long sum = 0;
                for (int j = 0; j < order; j++)
                    sum += qlpCoeff[j]*(long) (data[startAt + i - j - 1]);
                data[startAt + i] = residual[i] + (int) (sum >> lpQuantization);
            }
        }
    }
}