using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FlacBox
{
    /// <summary>
    /// Contains estimation algorithms for FlacWriter.
    /// 
    /// TODO add more mathematical analysis to improve performance.
    /// </summary>
    abstract class FlacWriterEstimator
    {
        internal abstract FlacMethod FindBestMethod(int[] channelSamples, int bitsPerSample);
        internal abstract FlacMethodAndDataPair[] FindBestMethods(int[][] samples, int bitsPerSamples);
        internal abstract SoundChannelAssignmentType FindBestMethods(int[] leftSamples, int[] rightSamples, int bitsPerSample, out FlacMethodAndDataPair[] methods);
    }

    sealed class FlacWriterEstimatorImpl : FlacWriterEstimator
    {
        ParallelExecution parallel;
        FlacEncodingPolicy policy;

        internal FlacWriterEstimatorImpl(FlacEncodingPolicy policy)
        {
            this.policy = policy;
            parallel = ParallelExecutionFactory.CreateParallelExecution(policy.UseParallelExtensions);
        }

        internal override FlacMethod FindBestMethod(int[] channelSamples, int bitsPerSample)
        {
            var i = 1;
            var firstSample = channelSamples[0];
            while (i < channelSamples.Length && channelSamples[i] == firstSample)
                i++;

            if (i == channelSamples.Length)
            {
                // constant method will be better than other methods
                return new FlacConstantMethod(bitsPerSample);
            }

            FlacMethod verbatimMethod;
            verbatimMethod = new FlacVerbatimMethod(
                channelSamples.Length, bitsPerSample);

            FlacMethod fixedMethod = null, lpcMethod = null;
            
            parallel.Invoke(
                delegate
                {
                    fixedMethod = FindBestFixedMethod(channelSamples, bitsPerSample, policy);
                },
                delegate
                {
                    lpcMethod = FindBestLpcMethod(channelSamples, bitsPerSample, policy);
                }
            );

            return FindBestMethod(new FlacMethod[] { 
                verbatimMethod, fixedMethod, lpcMethod });
        }

        private FlacMethod FindBestLpcMethod(int[] channelSamples, int bitsPerSample, FlacEncodingPolicy policy)
        {
            if (!policy.LpcOrder.HasValue) return null;

            var minLpcOrder = policy.LpcOrder.Value.MinValue;
            var maxLpcOrder = Math.Min(policy.LpcOrder.Value.MaxValue, 
                channelSamples.Length - 1);

            var r = new double[maxLpcOrder + 1];
            parallel.For(0, r.Length, i =>
            {
                double sum = 0;
                for (int j = 0, q = channelSamples.Length - i; j < i; j++, q++)
                    sum += (double)channelSamples[j] * channelSamples[q];
                for (int j = i, q = 0; j < channelSamples.Length; j++, q++)
                    sum += (double)channelSamples[j] * channelSamples[q];
                r[i] = sum;
            });

            var methods = new FlacMethod[maxLpcOrder];            
            parallel.For(minLpcOrder, maxLpcOrder + 1, order =>
            {
                var coef = SolveLpc(r, order);

                int[] integerCoefficients;
                int shift;
                int precision;
                ConvertLpcCoeficientsToIntegers(coef, out integerCoefficients, out precision, out shift);

                var predictor = PredictorFactory.CreateLpcPredictor(integerCoefficients, shift, ArrayUtils.CutArray(channelSamples, 0, order - 1));
                var residual = FindBestResidual(channelSamples, order, predictor, policy);
                var lpcCoefficients = new FlacLpcMethodCoefficeints(
                    precision, integerCoefficients, shift);

                FlacMethod method = new FlacLpcMethod(bitsPerSample, lpcCoefficients, residual);
                methods[order - 1] = method;
            });

            return FindBestMethod(methods);
        }

        private int GetPrecisionForSigned(int[] array)
        {
            int min, max;
            min = max = array[0];
            for (var i = 1; i < array.Length; i++)
            {
                if (min > array[i]) min = array[i];
                if (max < array[i]) max = array[i];
            }
            return GetBitsForSignedNumbersInRange(min, max);
        }

        private void ConvertLpcCoeficientsToIntegers(double[] coef, out int[] integerCoef, out int precision, out int shift)
        {
            double maxOrder = 0;
            for (var i = 0; i < coef.Length; i++)
            {
                var order = Math.Log(Math.Abs(coef[i]), 2);
                if (maxOrder < order) maxOrder = order;
            }

            const int ActualBitsInCoeficients = 14;
            shift = ActualBitsInCoeficients - (int)Math.Ceiling(maxOrder);

            integerCoef = new int[coef.Length];

            do
            {
                var coeficientMultiplier = Math.Pow(2, shift);
                for (var i = 0; i < coef.Length; i++)
                {
                    integerCoef[i] = (int)Math.Round(coef[i] * coeficientMultiplier);
                }

                precision = GetPrecisionForSigned(integerCoef);

                if (precision < 16) break;

                shift--;
            } while (precision >= 16);
        }

        private double[] SolveLpc(double[] r, int p)
        {
            var y = new double[p];
            var t = new double[p * 2 - 1];
            for (var i = 0; i < p; i++)
            {
                y[i] = r[i + 1];
            }

            t[p - 1] = r[0];
            for (var i = 1; i < p; i++)
            {
                t[p + i - 1] = r[i];
                t[p - i - 1] = r[i];
            }

            return LevinsonRecursion.Solve(p, t, y);
        }

        private FlacMethod FindBestFixedMethod(int[] channelSamples, int bitsPerSample, FlacEncodingPolicy policy)
        {
            if (!policy.FixedOrder.HasValue) return null;

            var minFixedOrder = policy.FixedOrder.Value.MinValue;
            var maxFixedOrder = Math.Min(channelSamples.Length - 1, 
                policy.FixedOrder.Value.MaxValue);

            var methods = new FlacMethod[maxFixedOrder + 1];
            parallel.For(minFixedOrder, maxFixedOrder + 1, order =>
            {
                var predictor = PredictorFactory.CreateFixedPredictor(order, ArrayUtils.CutArray(channelSamples, 0, order - 1));
                var residual = FindBestResidual(channelSamples, order, predictor, policy);

                FlacMethod method = new FlacFixedMethod(bitsPerSample, order, residual);
                methods[order] = method;
            });

            return FindBestMethod(methods);
        }

        private FlacMethod FindBestMethod(IList<FlacMethod> methods)
        {
            var bestMethodIndex = 0;
            while (bestMethodIndex < methods.Count && methods[bestMethodIndex] == null)
                bestMethodIndex++;

            if (bestMethodIndex == methods.Count) throw new FlacException("No valid method");

            for (var i = bestMethodIndex + 1; i < methods.Count; i++)
            {                
                if (methods[i] != null && methods[i].EstimatedSize < methods[bestMethodIndex].EstimatedSize)
                    bestMethodIndex = i;
            }
            return methods[bestMethodIndex];
        }

        private FlacResidualCoefficeints FindBestResidual(int[] channelSamples, int order, IPredictor predictor, FlacEncodingPolicy policy)
        {
            int[] residual;
            if (order > 0)
            {
                residual = new int[channelSamples.Length];
                var lastSample = channelSamples[order - 1];
                for (var i = order; i < residual.Length; i++)
                {
                    var nextSample = channelSamples[i];
                    residual[i] = nextSample - predictor.Next(lastSample);
                    lastSample = nextSample;
                }
            }
            else
                residual = channelSamples;

            var minRiceOrder = policy.RicePartionOrder.MinValue;
            var maxRiceOrder = policy.RicePartionOrder.MaxValue;
            var rices = new List<FlacResidualCoefficeints>();
            var samplesPerPartition = channelSamples.Length >> minRiceOrder;

            if (samplesPerPartition << minRiceOrder != channelSamples.Length)
            {
                minRiceOrder = maxRiceOrder = 0; // reset minRiceOrder to zero;
            }

            for (var riceOrder = minRiceOrder; riceOrder <= maxRiceOrder; riceOrder++)
            {
                if (samplesPerPartition <= order) break;

                var partitionCount = 1 << riceOrder;

                var parameters = new int[partitionCount];
                var totalPartitionDataSize = 0;
                var j = order;
                for (var i = 0; i < partitionCount; i++)
                {
                    var skipAmount = i == 0 ? order : 0;
                    int estimatedPartitionSize;
                    int riceParameter;
                    FindBestResidual(residual, samplesPerPartition * i + skipAmount, samplesPerPartition - skipAmount,
                        out estimatedPartitionSize, out riceParameter);
                    totalPartitionDataSize += estimatedPartitionSize;
                    parameters[i] = riceParameter;
			    }

                const int NormalPrecision = 4;
                const int ExtendedPrecision = 5;
                const int MinValueForExtendedParameters = 15;

                var isExtended = Array.Exists(parameters, delegate(int x) {
                    return x >= MinValueForExtendedParameters;
                });

                var totalSize = 4 + totalPartitionDataSize +
                    partitionCount * (isExtended ? ExtendedPrecision : NormalPrecision);

                var rice = new FlacResidualCoefficeints();
                rice.EstimatedSize = totalSize;
                rice.IsExtended = isExtended;
                rice.RiceParameters = parameters;
                rice.Order = riceOrder;

                rices.Add(rice);

                if ((samplesPerPartition & 1) != 0) break;
                samplesPerPartition >>= 1;
            }

            var bestRicePartition = 0;
            for (var i = 1; i < rices.Count; i++)
            {
                if (rices[bestRicePartition].EstimatedSize > rices[i].EstimatedSize)
                    bestRicePartition = i;
            }
            return rices[bestRicePartition];
        }

        private void FindBestResidual(int[] residual, int offset, int count, out int estimatedSize, out int riceParameter)
        {
            var bitsForSignedNumber = GetPrecisionForSigned(residual);

            var escapedVersionSize = bitsForSignedNumber * count + 5;

            if (bitsForSignedNumber < 2)
            {
                estimatedSize = escapedVersionSize;
                riceParameter = ~bitsForSignedNumber;
                return;
            }

            var maxRiceParameter = bitsForSignedNumber - 2;
            var riceVersionSizes = new int[maxRiceParameter + 1];
            for (var m = 0; m <= maxRiceParameter; m++)
            {
                riceVersionSizes[m] = (m + 1) * count;
            }

            for (var i = 0; i < count; i++)
            {
                var sample = residual[offset + i];
                if (sample < 0)
                {
                    riceVersionSizes[0]++;
                    sample = ~sample;
                }
                riceVersionSizes[0] += sample << 1;

                for (var m = 1; m <= maxRiceParameter; m++)
                {
                    riceVersionSizes[m] += sample;
                    sample >>= 1;
                }
            }

            var bestRiceIndex = 0;
            for (var m = 1; m <= maxRiceParameter; m++)
            {
                if (riceVersionSizes[bestRiceIndex] > riceVersionSizes[m])
                    bestRiceIndex = m;
            }

            if (riceVersionSizes[bestRiceIndex] <= escapedVersionSize)
            {
                estimatedSize = riceVersionSizes[bestRiceIndex];
                riceParameter = bestRiceIndex;
            }
            else
            {
                estimatedSize = escapedVersionSize;
                riceParameter = ~bitsForSignedNumber;
            }
        }

        // Tried Math.Log(Log2 * residualAbsDataSum / residual.Length, 2);
        // Performance is sampe as full search

        private int GetBitsForSignedNumbersInRange(int min, int max)
        {
            Debug.Assert(min <= max);

            if (min >= 0 || max >= ~min)
                return GetBitsForNumber((uint)max) + 1;
            else 
                return GetBitsForNumber((uint)~min) + 1;
        }

        private int GetBitsForNumber(uint n)
        {
            var bits = 0;
            uint mask = 1;
            if (n >= 0x100)
            {
                bits += 8;
                if (n >= 0x10000)
                {
                    bits += 8;
                    if (n >= 0x1000000)
                        bits += 8;
                }
                mask <<= bits;
            }
            while (mask <= n)
            {
                mask <<= 1;
                bits++;
            }
            return bits;
        }

        internal override FlacMethodAndDataPair[] FindBestMethods(int[][] samples, int bitsPerSamples)
        {
            var samplesAndMethods = new FlacMethodAndDataPair[samples.Length];
            parallel.For(0, samples.Length, i =>
            {
                samplesAndMethods[i] = new FlacMethodAndDataPair(
                    FindBestMethod(samples[i], bitsPerSamples),
                    bitsPerSamples, 
                    samples[i]);
            });
            return samplesAndMethods;
        }

        internal override SoundChannelAssignmentType FindBestMethods(int[] leftSamples, int[] rightSamples, int bitsPerSample, out FlacMethodAndDataPair[] methods)
        {
            if (policy.StereoEncoding == StereoEncodingPolicy.AsIs)
            {
                methods = FindBestMethods(new int[2][] { leftSamples, rightSamples }, bitsPerSample);
                return SoundChannelAssignmentType.LeftRight;
            }

            var samplesPerChannel = leftSamples.Length;
            int[] differenceLeftMinusRight = null, average = null;

            FlacMethod methodForLeft = null, methodForRight = null, methodForSide = null, methodForAverage = null;

            parallel.Invoke(
                delegate
                {
                    methodForLeft = FindBestMethod(leftSamples, bitsPerSample);
                },
                delegate
                {
                    methodForRight = FindBestMethod(rightSamples, bitsPerSample);
                },
                delegate
                {
                    differenceLeftMinusRight = new int[samplesPerChannel];
                    for (var i = 0; i < samplesPerChannel; i++)
                    {
                        differenceLeftMinusRight[i] = leftSamples[i] - rightSamples[i];
                    }
                    methodForSide = FindBestMethod(differenceLeftMinusRight, bitsPerSample + 1);
                },
                delegate
                {
                    if (policy.StereoEncoding == StereoEncodingPolicy.TrySidesAndAverage)
                    {
                        average = new int[samplesPerChannel];
                        for (var i = 0; i < samplesPerChannel; i++)
                        {
                            average[i] = (leftSamples[i] + rightSamples[i]) >> 1;
                        }
                        methodForAverage = FindBestMethod(average, bitsPerSample);
                    }
                }
            );

            var independentEstimation = methodForLeft.EstimatedSize + methodForRight.EstimatedSize;
            var leftSideEstimation = methodForLeft.EstimatedSize + methodForSide.EstimatedSize;
            var rightSideEstimation = methodForSide.EstimatedSize + methodForRight.EstimatedSize;
            var averageEstimation = average == null ? int.MaxValue :
                methodForAverage.EstimatedSize + methodForSide.EstimatedSize;

            SoundChannelAssignmentType type;
            if (Math.Min(independentEstimation, leftSideEstimation) < Math.Min(rightSideEstimation, averageEstimation))
            {
                if (independentEstimation <= leftSideEstimation)
                {
                    type = SoundChannelAssignmentType.LeftRight;
                    methods = new FlacMethodAndDataPair[] {
                        new FlacMethodAndDataPair(methodForLeft, bitsPerSample, leftSamples), 
                        new FlacMethodAndDataPair(methodForRight, bitsPerSample, rightSamples)
                    };
                }
                else
                {
                    type = SoundChannelAssignmentType.LeftSide;
                    methods = new FlacMethodAndDataPair[] {
                        new FlacMethodAndDataPair(methodForLeft, bitsPerSample, leftSamples), 
                        new FlacMethodAndDataPair(methodForSide, bitsPerSample + 1, differenceLeftMinusRight)
                    };
                }
            }
            else
            {
                if (rightSideEstimation <= averageEstimation)
                {
                    type = SoundChannelAssignmentType.RightSide;
                    methods = new FlacMethodAndDataPair[] {
                        new FlacMethodAndDataPair(methodForSide, bitsPerSample + 1, differenceLeftMinusRight), 
                        new FlacMethodAndDataPair(methodForRight, bitsPerSample, rightSamples)
                    };
                }
                else
                {
                    type = SoundChannelAssignmentType.MidSide;
                    methods = new FlacMethodAndDataPair[] {
                        new FlacMethodAndDataPair(methodForAverage, bitsPerSample, average), 
                        new FlacMethodAndDataPair(methodForSide, bitsPerSample + 1, differenceLeftMinusRight)
                    };
                }
            }
            return type;
        }
    }
}
