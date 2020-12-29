using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    /// <summary>
    /// the Collection of gene, chromosome is individuals（因为基于十进制遗传算法）
    /// </summary>
    [Serializable]
    public class Chromosome :IComparable<Chromosome>
    {
        #region about chromosome(individual)
        public int ChromosomeSize { get; set; }//Number of parameter(gene) in this chromosome(individual)

        public double[] LowerBd { get; set; }// The lower boundary of each parameter

        public double[] UpperBd { get; set; }// The upper boundary of each parameter

        public int[] DecimalPlace { get; set; }//Number of decimal points for each parameter
        #endregion

        #region about population
        public int PopulationSize { get; set; }//Number of chromosome(individual) in this population
        public double CrossoverProbability { get; set; }//
        public double MutationProbability { get; set; }
        #endregion

        #region about genetic Algorithm
        public int NumPopulation { get; set; }//Number of populations in this GA
        public int CurrentIteration { get; set; }
        public int MaxIteration { get; set; }

        public double averageOfPop;
        public double standardDeviationOfPop;
        #endregion


        public double Fit { get;  set; }// fitness(Errors) of chromosome
        public double[] chromosome { get; set; }//a set of genes(parameters)

        public Chromosome()
        {

        }
        public Chromosome(int ChromosomeSize, double[] LowerBd, double[] UpperBd, int[] DecimalPlace,
                        int PopulationSize, double CrossoverProbability, double MutationProbability,
                        int CurrentIteration, int MaxIteration)
        {
            this.ChromosomeSize = ChromosomeSize;
            this.LowerBd = LowerBd;
            this.UpperBd = UpperBd;
            this.DecimalPlace = DecimalPlace;

            this.PopulationSize = PopulationSize;
            this.CrossoverProbability = CrossoverProbability;
            this.MutationProbability = MutationProbability;

            this.CurrentIteration = CurrentIteration;
            this.MaxIteration = MaxIteration;
        }
        /// <summary>
        /// Initialize  genes(parameters) of chromosome(individual)
        /// </summary>
        public void ChromosomeIni()
        {
                chromosome = myRandom.NextDouble(LowerBd, UpperBd, DecimalPlace);
        }

        /// <summary>
        /// solve the fitness of chromosome(individual)
        /// </summary>
        public void SolveFitness()
        {
            Fit = Fitness.SolveErr(chromosome);

            //Console.Write($"Fit:{Fit} ,para:");
            //foreach (double para in chromosome)
            //{
            //    Console.Write($"{para} ");
            //}
            //Console.WriteLine();

        }


        /// <summary>
        /// 设置染色体各基因要保留的小数位数
        /// </summary>
        public void SetChromDecimalPlace()
        {
            for (int i = 0; i < ChromosomeSize; i++)
            {
                //chromosome[i] = Math.Round(chromosome[i], GetDecimalNum(StepSize[i]));
                chromosome[i] = Math.Round(chromosome[i], DecimalPlace[i]);
            }
        }

        public void CheckBoundary()
        {
            Random ran = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < ChromosomeSize; i++)
            {
                if (chromosome[i]>UpperBd[i])
                {
                    if (ran.NextDouble()<0.5)
                    {
                        chromosome[i] = UpperBd[i];
                    }
                    else
                    {
                        chromosome[i] = LowerBd[i] + ran.NextDouble() * (UpperBd[i] - LowerBd[i]);
                    }
                }
                else if (chromosome[i] < LowerBd[i])
                {
                    if (ran.NextDouble() < 0.5)
                    {
                        chromosome[i] = LowerBd[i];
                    }
                    else
                    {
                        chromosome[i] = LowerBd[i] + ran.NextDouble() * (UpperBd[i] - LowerBd[i]);
                    }
                }
                else if (Double.IsNaN(chromosome[i]) )
                {
                    chromosome[i] = LowerBd[i] + ran.NextDouble() * (UpperBd[i] - LowerBd[i]);
                }
            }
        }

        //https://www.cnblogs.com/SouthBegonia/p/12055328.html
        public int CompareTo(Chromosome other)
        {
            //if (this.Name != other.Name)
            //{
            //    return this.Name.CompareTo(other.Name);
            //}
            //else if (this.Age != other.Age)
            //{
            //    return this.Age.CompareTo(other.Age);
            //}
            //else return 0;
            if (this.Fit != other.Fit)
            {
                return this.Fit.CompareTo(other.Fit);
            }
            else return 0;
        }

        public bool GeneEqual(Chromosome other)
        {
            bool isGeneEqual=true;
            for (int i = 0; i < other.ChromosomeSize; i++)
            {
                if (this.chromosome[i]!= other.chromosome[i])
                {
                    isGeneEqual = false;
                    break;
                }
            }
            return isGeneEqual;
        }






        /// <summary>
        /// 获得num的小数位数 https://zhidao.baidu.com/question/328965055.html?qbl=relate_question_0
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public int GetDecimalNum(double num)
        {
            int result = 0;
            double newNum = 0;
            do
            {
                newNum = num * Math.Pow(10, result);

                if ((int)newNum == newNum)
                {
                    break;
                }

            } while (++result < int.MaxValue - 1);
            return result;
        }

        //https://blog.csdn.net/XJAVASunjava/article/details/7648242 l333.Add(Clone<tt>(lsttt[0]));
        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }


    }
}
