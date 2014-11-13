using System;

namespace FlacDotNet
{
    internal static class FixedPredictor
    {
        private const double MLn2 = 0.69314718055994530942;

        [Obsolete("Possible loss accurate", false)]
        public static int ComputeBestPredictor(int[] data, int dataLen, double[] residualBitsPerSample)
        {
            int lastError0 = data[1];
            int lastError1 = data[1] - data[2];
            int lastError2 = lastError1 - (data[2] - data[3]);
            int lastError3 = lastError2 - (data[2] - 2*data[3] + data[4]);
            int totalError0 = 0, totalError1 = 0, totalError2 = 0, totalError3 = 0, totalError4 = 0;
            int i, order;

            for (i = 0; i < dataLen; i++)
            {
                int error = data[i];
                totalError0 += Math.Abs(error);
                int save = error;
                error -= lastError0;
                totalError1 += Math.Abs(error);
                lastError0 = save;
                save = error;
                error -= lastError1;
                totalError2 += Math.Abs(error);
                lastError1 = save;
                save = error;
                error -= lastError2;
                totalError3 += Math.Abs(error);
                lastError2 = save;
                save = error;
                error -= lastError3;
                totalError4 += Math.Abs(error);
                lastError3 = save;
            }

            if (totalError0 < Math.Min(Math.Min(Math.Min(totalError1, totalError2), totalError3), totalError4))
                order = 0;
            else if (totalError1 < Math.Min(Math.Min(totalError2, totalError3), totalError4))
                order = 1;
            else if (totalError2 < Math.Min(totalError3, totalError4))
                order = 2;
            else if (totalError3 < totalError4)
                order = 3;
            else
                order = 4;

            // Estimate the expected number of bits per residual signal sample.
            // 'total_error*' is linearly related to the variance of the residual
            // signal, so we use it directly to compute E(|x|)
            residualBitsPerSample[0] = (totalError0 > 0) ? Math.Log(MLn2*totalError0/dataLen)/MLn2 : 0.0;
            residualBitsPerSample[1] = ((totalError1 > 0) ? Math.Log(MLn2*totalError1/dataLen)/MLn2 : 0.0);
            residualBitsPerSample[2] = ((totalError2 > 0) ? Math.Log(MLn2*totalError2/dataLen)/MLn2 : 0.0);
            residualBitsPerSample[3] = ((totalError3 > 0) ? Math.Log(MLn2*totalError3/dataLen)/MLn2 : 0.0);
            residualBitsPerSample[4] = ((totalError4 > 0) ? Math.Log(MLn2*totalError4/dataLen)/MLn2 : 0.0);

            return order;
        }

        [Obsolete("Possible loss accurate", false)]
        public static int ComputeBestPredictorWide(int[] data, int dataLen, double[] residualBitsPerSample)
        {
            int lastError0 = data[1];
            int lastError1 = data[1] - data[2];
            int lastError2 = lastError1 - (data[2] - data[3]);
            int lastError3 = lastError2 - (data[2] - 2*data[3] + data[4]);

            // totalError* are 64-bits to avoid overflow when encoding
            // erratic signals when the bits-per-sample and blocksize are
            // large.
            long totalError0 = 0, totalError1 = 0, totalError2 = 0, totalError3 = 0, totalError4 = 0;
            int i, order;

            for (i = 0; i < dataLen; i++)
            {
                int error = data[i];
                totalError0 += Math.Abs(error);
                int save = error;
                error -= lastError0;
                totalError1 += Math.Abs(error);
                lastError0 = save;
                save = error;
                error -= lastError1;
                totalError2 += Math.Abs(error);
                lastError1 = save;
                save = error;
                error -= lastError2;
                totalError3 += Math.Abs(error);
                lastError2 = save;
                save = error;
                error -= lastError3;
                totalError4 += Math.Abs(error);
                lastError3 = save;
            }

            if (totalError0 < Math.Min(Math.Min(Math.Min(totalError1, totalError2), totalError3), totalError4))
                order = 0;
            else if (totalError1 < Math.Min(Math.Min(totalError2, totalError3), totalError4))
                order = 1;
            else if (totalError2 < Math.Min(totalError3, totalError4))
                order = 2;
            else if (totalError3 < totalError4)
                order = 3;
            else
                order = 4;

            // Estimate the expected number of bits per residual signal sample.
            // 'total_error*' is linearly related to the variance of the residual
            // signal, so we use it directly to compute E(|x|)
            // with VC++ you have to spoon feed it the casting
            residualBitsPerSample[0] = (totalError0 > 0) ? Math.Log(MLn2*totalError0/dataLen)/MLn2 : 0.0;
            residualBitsPerSample[1] = (totalError1 > 0) ? Math.Log(MLn2*totalError1/dataLen)/MLn2 : 0.0;
            residualBitsPerSample[2] = (totalError2 > 0) ? Math.Log(MLn2*totalError2/dataLen)/MLn2 : 0.0;
            residualBitsPerSample[3] = (totalError3 > 0) ? Math.Log(MLn2*totalError3/dataLen)/MLn2 : 0.0;
            residualBitsPerSample[4] = (totalError4 > 0) ? Math.Log(MLn2*totalError4/dataLen)/MLn2 : 0.0;

            return order;
        }

        public static void ComputeResidual(int[] data, int dataLen, int order, int[] residual)
        {
            int idataLen = dataLen;

            switch (order)
            {
                case 0:
                    for (int i = 0; i < idataLen; i++)
                    {
                        residual[i] = data[i];
                    }
                    break;
                case 1:
                    for (int i = 0; i < idataLen; i++)
                    {
                        residual[i] = data[i] - data[i - 1];
                    }
                    break;
                case 2:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* == data[i] - 2*data[i-1] + data[i-2] */
                        residual[i] = data[i] - (data[i - 1] << 1) + data[i - 2];
                    }
                    break;
                case 3:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* == data[i] - 3*data[i-1] + 3*data[i-2] - data[i-3] */
                        residual[i] = data[i] - (((data[i - 1] - data[i - 2]) << 1) + (data[i - 1] - data[i - 2])) -
                                      data[i - 3];
                    }
                    break;
                case 4:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* == data[i] - 4*data[i-1] + 6*data[i-2] - 4*data[i-3] + data[i-4] */
                        residual[i] = data[i] - ((data[i - 1] + data[i - 3]) << 2) +
                                      ((data[i - 2] << 2) + (data[i - 2] << 1)) + data[i - 4];
                    }
                    break;
            }
        }

        public static void RestoreSignal(int[] residual, int dataLen, int order, ref int[] data, int startAt)
        {
            int idataLen = dataLen;

            switch (order)
            {
                case 0:
                    for (int i = 0; i < idataLen; i++)
                    {
                        data[i + startAt] = residual[i];
                    }
                    break;
                case 1:
                    for (int i = 0; i < idataLen; i++)
                    {
                        data[i + startAt] = residual[i] + data[i + startAt - 1];
                    }
                    break;
                case 2:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* == residual[i] + 2*data[i-1] - data[i-2] */
                        data[i + startAt] = residual[i] + (data[i + startAt - 1] << 1) - data[i + startAt - 2];
                    }
                    break;
                case 3:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* residual[i] + 3*data[i-1] - 3*data[i-2]) + data[i-3] */
                        data[i + startAt] = residual[i] +
                                            (((data[i + startAt - 1] - data[i + startAt - 2]) << 1) +
                                             (data[i + startAt - 1] - data[i + startAt - 2])) + data[i + startAt - 3];
                    }
                    break;
                case 4:
                    for (int i = 0; i < idataLen; i++)
                    {
                        /* == residual[i] + 4*data[i-1] - 6*data[i-2] + 4*data[i-3] - data[i-4] */
                        data[i + startAt] = residual[i] + ((data[i + startAt - 1] + data[i + startAt - 3]) << 2) -
                                            ((data[i + startAt - 2] << 2) + (data[i + startAt - 2] << 1)) -
                                            data[i + startAt - 4];
                    }
                    break;
            }
        }
    }
}