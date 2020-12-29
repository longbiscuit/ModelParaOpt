using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn.GA
{
    //http://www.liangshunet.com/ca/201307/337451622.htm
    public static class myRandom
    {
        /// <summary>
        /// generate int array that have n unique elements
        /// </summary>
        /// <param name="minValue">element lower boundary</param>
        /// <param name="maxValue">element upper boundary</param>
        /// <param name="n">number of element</param>
        /// <returns> int array that have n unique elements</returns>
        public static int[] NextUnique(int minValue, int maxValue, int n)
        {
            //如果生成随机数个数大于指定范围的数字总数，则最多只生成该范围内数字总数个随机数
            if (n > maxValue - minValue + 1)
                n = maxValue - minValue + 1;

            int maxIndex = maxValue - minValue + 2;// 索引数组上限
            int[] indexArr = new int[maxIndex];
            for (int i = 0; i < maxIndex; i++)
            {
                indexArr[i] = minValue - 1;
                minValue++;
            }

            Random ran = new Random(Guid.NewGuid().GetHashCode());
            int[] randNum = new int[n];
            int index;
            for (int j = 0; j < n; j++)
            {
                index = ran.Next(1, maxIndex - 1);// 生成一个随机数作为索引

                //根据索引从索引数组中取一个数保存到随机数数组
                randNum[j] = indexArr[index];

                // 用索引数组中最后一个数取代已被选作随机数的数
                indexArr[index] = indexArr[maxIndex - 1];
                maxIndex--; //索引上限减 1
            }
            return randNum;
        }


        /// <summary>
        /// Generate a random floating point array  between two arrays
        /// </summary>
        /// <param name="minValue">minimum double array</param>
        /// <param name="maxValue">maximum double array</param>
        /// <returns></returns>
        public static double[] NextDouble(double[] minValue, double[] maxValue)
        {
         
            if (minValue.Length == maxValue.Length)
            {
                double[] NextDoubleArray = new double[minValue.Length];
                Random ran = new Random(Guid.NewGuid().GetHashCode());//http://www.splaybow.com/post/csharp-generate-random-num.html
                for (int i = 0; i < minValue.Length; i++)
                {
                    if (maxValue[i]>= minValue[i])
                    {

                        NextDoubleArray[i] = ran.NextDouble() * (maxValue[i] - minValue[i]) + minValue[i];
                    }
                    else
                    {
                        throw new MyException("The elements of maximum array are not greater than the elements of minimum array !");
                    }
                }
                return NextDoubleArray;
            }
            else
            {
                throw new MyException("The maximum and minimum arrays do not have the same number of elements!");
            }
        }

        /// <summary>
        /// Generate a random floating point array  with specify decimal between two arrays
        /// </summary>
        /// <param name="minValue">minimum double array</param>
        /// <param name="maxValue">maximum double array</param>
        /// <param name="decimalPlace">Specified number of decimal places</param>
        /// <returns></returns>
        public static double[] NextDouble(double[] minValue, double[] maxValue,int[] decimalPlace)
        {

            if (minValue.Length == maxValue.Length)
            {
                double[] NextDoubleArray = new double[minValue.Length];
                double randNum;
                Random ran = new Random(Guid.NewGuid().GetHashCode());//http://www.splaybow.com/post/csharp-generate-random-num.html
                for (int i = 0; i < minValue.Length; i++)
                {
                   
                    if (maxValue[i] >= minValue[i])
                    {
                        randNum = ran.NextDouble() * (maxValue[i] - minValue[i]) + minValue[i];
                        NextDoubleArray[i]=Convert.ToDouble(randNum.ToString("f" + decimalPlace[i]));
                    }
                    else
                    {
                        throw new MyException("The elements of maximum array are not greater than the elements of minimum array !");
                    }
                }
                return NextDoubleArray;
            }
            else
            {
                throw new MyException("The maximum and minimum arrays do not have the same number of elements!");
            }
        }




        /// <summary>
        /// 生成一个在[minValue,maxValue]区间的随机浮点数
        /// </summary>
        /// <param name="ran">随机数生成器对象</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns></returns>
        public static double NextDouble(  double minValue, double maxValue)
        {
            Random ran =new Random(Guid.NewGuid().GetHashCode());//http://www.splaybow.com/post/csharp-generate-random-num.html
            return ran.NextDouble() * (maxValue - minValue) + minValue;
        }
        /// <summary>
        /// 生成一个在[minValue,maxValue]区间的随机浮点数，并保留decimalPlace位小数
        /// </summary>
        /// <param name="ran">随机数生成器对象</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="decimalPlace">有效位数</param>
        /// <returns></returns>
        public static double NextDouble( double minValue, double maxValue, int decimalPlace)
        {
            Random ran = new Random(Guid.NewGuid().GetHashCode());
            double randNum = ran.NextDouble() * (maxValue - minValue) + minValue;
            return Convert.ToDouble(randNum.ToString("f" + decimalPlace));
        }

        public static double[] NextDouble(double minValue, double maxValue, int decimalPlace,int n)
        {

            Random ran = new Random(Guid.NewGuid().GetHashCode());//http://www.splaybow.com/post/csharp-generate-random-num.html
            double[] NextDoubleArray = new double[n];
            for (int i = 0; i < n; i++)
            {
                NextDoubleArray[i] = myRandom.NextDouble( minValue,  maxValue,  decimalPlace);
            }
            return NextDoubleArray;
        }

    }
}
