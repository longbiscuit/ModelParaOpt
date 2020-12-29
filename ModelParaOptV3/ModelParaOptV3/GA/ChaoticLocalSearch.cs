using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    public delegate Chromosome CLSDelegateEventHandler(Chromosome chrom);
    public class ChaoticLocalSearch
    {
        public event CLSDelegateEventHandler CLSMethodEvent;
        public Chromosome LocalSearch(Chromosome chrom)
        {
            return CLSMethodEvent?.Invoke(chrom);
        }


        public Chromosome ChaoticLS(Chromosome chrom)//Jia D , Zheng G , Khan M K . An effective memetic differential evolution algorithm based on chaotic local search[J]. Information ences, 2011, 181(15):3175-3187.
        {
            Chromosome betterChrom = Chromosome.Clone<Chromosome>(chrom);//use this method doing deep copy
            double beta = myRandom.NextDouble(0.0, 1.0);
            double u = 4.0, m = 1000;
            double betac, lambda;

            int Sl = (int)Math.Round(chrom.ChromosomeSize / 5.0);
            if (Sl < 1) Sl = 1;

            int[] loc = myRandom.NextUnique(0, chrom.ChromosomeSize - 1, Sl);
            //Random ran = new Random(Guid.NewGuid().GetHashCode());

            while (true)
            {
                if (beta != 0.25 && beta != 0.5 && beta != 0.75)
                {
                    break;
                }
                beta = myRandom.NextDouble(0.0, 1.0);
            }

            if (chrom.CurrentIteration<10)
            {
                //meathod 1
                for (int i = 0; i < chrom.PopulationSize * 5; i++)
                {
                    beta = u * beta * (1 - beta);

                    foreach (var j in loc)
                    {
                        betac = chrom.LowerBd[j] + beta * (chrom.UpperBd[j] - chrom.LowerBd[j]);
                        if (chrom.Fit > 0.0)
                        {
                            lambda = 1 - Math.Pow(Math.Abs((chrom.Fit - 1) / chrom.Fit), m);
                        }
                        else
                        {
                            lambda = 1 - Math.Pow(Math.Abs((chrom.Fit + 1) / chrom.Fit), m);
                        }

                        betterChrom.chromosome[j] = (1 - lambda) * chrom.chromosome[j] + lambda * betac;
                    }
                    betterChrom.CheckBoundary();
                    betterChrom.SetChromDecimalPlace();
                    betterChrom.SolveFitness();

                    if (betterChrom.Fit < chrom.Fit)
                    {
                        //Console.WriteLine("local search find a better individuals!");
                        break;
                    }
                }

            }
            else
            {
                //meathod 2
                loc = myRandom.NextUnique(0, chrom.ChromosomeSize - 1, 1);
                int chaoticNum = chrom.PopulationSize * 3;
                List<Chromosome> betterChromList = new List<Chromosome>();
                for (int i = 0; i < chaoticNum; i++)
                {
                    beta = u * beta * (1 - beta);
                    betterChromList.Add(Chromosome.Clone<Chromosome>(chrom));
                    foreach (var j in loc)
                    {
                        betac = chrom.LowerBd[j] + beta * (chrom.UpperBd[j] - chrom.LowerBd[j]);
                        if (chrom.Fit > 0.0)
                        {
                            lambda = 1 - Math.Pow(Math.Abs((chrom.Fit - 1) / chrom.Fit), m);
                        }
                        else
                        {
                            lambda = 1 - Math.Pow(Math.Abs((chrom.Fit + 1) / chrom.Fit), m);
                        }
                        betterChromList[i].chromosome[j] = (1 - lambda) * chrom.chromosome[j] + lambda * betac;
                    }
                    betterChromList[i].CheckBoundary();
                    betterChromList[i].SetChromDecimalPlace();
                }

                //https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/how-to-cancel-a-parallel-for-or-foreach-loop
                Parallel.For(0, chaoticNum, i =>
                {
                    betterChromList[i].SolveFitness();
                });

                int minFitIndex = 0;
                double minFit = double.MaxValue;
                for (int ii = 0; ii < betterChromList.Count; ii++)
                {
                    if (minFit > betterChromList[ii].Fit)
                    {
                        minFit = betterChromList[ii].Fit;
                        minFitIndex = ii;
                    }
                }
                betterChrom = Chromosome.Clone<Chromosome>(betterChromList[minFitIndex]);

            }

            if (betterChrom.Fit < chrom.Fit)
            {
                return betterChrom;
            }
            else
            {
                return chrom;
            }


        }

    }





}
