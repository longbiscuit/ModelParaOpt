using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    public class Population
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
        public int NumPopulation;//Number of populations in this GA
        public int CurrentIteration { get; set; }
        public int MaxIteration { get; set; }
        #endregion

        public List<Chromosome> ChromsOfPop { get; set; }
        public double averageOfPop;
        public double standardDeviationOfPop;

        //public List<Chromosome> HistBestChromOfPop { get; set; }//The best individual in the population

        public Population()
        {

        }
        public Population(int ChromosomeSize, double[] LowerBd, double[] UpperBd, int[] DecimalPlace,
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
            ChromsOfPop = new List<Chromosome>(PopulationSize);//创建一个PopulationSize个元素的列表
            //HistBestChromOfPop = new List<Chromosome>(MaxIteration);//Record the best individuals of the population in history
        }

        public void ChromsOfPopIni()
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                // Initialize every chromosome(individual)
                Chromosome chromsofPop = new Chromosome(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                                 PopulationSize, CrossoverProbability, MutationProbability,
                                                 CurrentIteration, MaxIteration);
                chromsofPop.ChromosomeIni();
                ChromsOfPop.Add(chromsofPop);
            }
        }

        ////sigle thread
        //public void ChromsOfPopSolveFit()
        //{
        //    for (int i = 0; i < ChromsOfPop.Count; i++)
        //    {
        //        //solve fitness of every chromosome
        //        ChromsOfPop[i].SolveFitness();
        //    }
        //}

        // multi-thread
        public void ChromsOfPopSolveFit()
        {
            int popSize = ChromsOfPop.Count;
            double sumFit = 0.0;
            double[] fitArr = new double[popSize];

            //for(int i = 0; i < popSize; i++)
            //{
            //    //solve fitness of every chromosome
            //    ChromsOfPop[i].SolveFitness();
            //}

            //https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/how-to-write-a-simple-parallel-for-loop
            Parallel.For(0, popSize, i =>
            {
                //solve fitness of every chromosome
                ChromsOfPop[i].SolveFitness();

            });
            for (int i = 0; i < popSize; i++)
            {
                fitArr[i] = ChromsOfPop[i].Fit;
                sumFit += fitArr[i];
            }


            averageOfPop = sumFit / popSize;
            for (int i = 0; i < popSize; i++)
            {
                standardDeviationOfPop += Math.Pow(fitArr[i] - averageOfPop, 2.0);
            }
            standardDeviationOfPop = Math.Sqrt(standardDeviationOfPop / popSize);
        }

        public void ChromsOfPopCWFitAndChroms()
        {
            //for (int i = 0; i < ChromsOfPop.Count; i++)
            //{
            //    Console.Write($"Fit:{ChromsOfPop[i].Fit} ,para:");
            //    foreach (double para in ChromsOfPop[i].chromosome)
            //    {
            //        Console.Write($"{para} ");
            //    }
            //    Console.WriteLine();

            //}
        }




        /// <summary>
        /// Get an individual by index of Pop,for example: pop[i]
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>one chromosome</returns>
        //public Chromosome this[int i]
        //{
        //    get
        //    {
        //        if (i < 0 || i > PopulationSize-1) throw new MyException("The given index is out of range!");
        //        return ChromsOfPop[i];
        //    }
        //    set
        //    {
        //        if (i < 0 || i > PopulationSize-1) throw new MyException("The given index is out of range!");
        //        ChromsOfPop[i]= value;
        //    }
        //}




        public void FitSort()
        {
            //ChromsOfPop.Sort((x, y) =>
            //{
            //    if (x.Fit != y.Fit)
            //    {
            //        return x.Fit.CompareTo(y.Fit);
            //    }
            //    else return 0;

            //});//https://blog.csdn.net/jimo_lonely/article/details/51711821
            ChromsOfPop.Sort();
        }








    }
}
