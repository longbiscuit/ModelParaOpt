using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    public delegate Chromosome MuteDelegateEventHandler(Chromosome chrom);
    public class Mutation
    {
        public event MuteDelegateEventHandler MuteMethodEvent;
        public Chromosome Mute(Chromosome chrom)
        {
            return MuteMethodEvent?.Invoke(chrom); 
        }

        /// <summary>
        /// 1. Dynamic rate mute operator
        /// </summary>
        /// <param name="chrom">The individual to mutate</param>
        /// <returns>A mutated individual</returns>
        public Chromosome DRM(Chromosome chrom)
        {
            int nVar,nmu,it,MaxIt;
            int[] muLocation;//
            double a, b, sm;
            nVar = chrom.chromosome.Length;
            //double[] chromosomeTemp = chrom.chromosome;
            double[] Fai0;

            //Chromosome chromTemp =(Chromosome)DeepClone(chrom);// deep clone not work
            Chromosome chromTemp = Chromosome.Clone<Chromosome>(chrom);//use this method doing deep copy

            nmu = Convert.ToInt32( Math.Ceiling(chrom.MutationProbability*nVar));
            if (nmu>nVar) nmu = nVar;
            muLocation = myRandom.NextUnique(1, nVar, nmu); //生成nmu个1~nVar之间的随机整数，突变的位置，突变一个或多个元素
            Fai0= myRandom.NextDouble(-1, 1, 10,nVar); //产生[-1, 1]之间随机数行向量
            b = 1.0;a = 10.0;
            it = chrom.CurrentIteration;
            MaxIt = chrom.MaxIteration;
            double waveNum = 3;
            //sm = 1.0 / 3.0 * Math.Atan(a * Math.Pow((1.0 - 2.0 * (it/ MaxIt)), b)) + 0.5;//% 数 ,3 * it决定有3个波段
            sm = 1.0 / 3.0 * Math.Atan(a * Math.Pow(1.0 - 2.0 * (((waveNum * it)% MaxIt) / MaxIt),b)) + 0.5;//% 数 ,3 * it决定有3个波段
            for (int i = 0; i < muLocation.Length; i++)
            {
                for (int j = 0; j < nVar; j++)
                {
                    if (muLocation[i] == j)
                    {
                        //chromosomeTemp[i] = chrom.chromosome[i] + 0.5 * sm * Fai0[i]* (chrom.UpperBd[i] - chrom.LowerBd[i]);// 0.5 *[1, 0] *[-1, 1] *[区间长度];
                        chromTemp.chromosome[i] = chrom.chromosome[i] + 0.5 * sm * Fai0[i] * (chrom.UpperBd[i] - chrom.LowerBd[i]);// 0.5 *[1, 0] *[-1, 1] *[区间长度];
                    }
                    //chrom.chromosome[i] = chromosomeTemp[i];//
                }
            }
            chromTemp.Fit = double.MaxValue;
            chromTemp.CheckBoundary();
            chromTemp.SetChromDecimalPlace();
            return chromTemp;
        }//DRM




        ///// <summary>
        ///// Deep copy(new object but have same properties and methods)
        ///// </summary>
        ///// <param name="obj">object to deep copy, need have a nonParameter construction method</param>
        ///// <returns>new object</returns>
        //public static object DeepClone(object obj)
        //{
        //    Type type = obj.GetType();
        //    object newObj = Activator.CreateInstance(type);
        //    foreach (var prop in type.GetProperties())
        //    {
        //        if (prop.CanRead && prop.CanWrite)
        //        {
        //            object value = prop.GetValue(obj);
        //            prop.SetValue(newObj, value);
        //        }
        //    }
        //    return newObj;
        //}


    }
}
