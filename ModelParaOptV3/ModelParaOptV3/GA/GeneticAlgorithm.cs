using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ModelParaOptExcelAddIn.GA
{
    //author:Zhu Binglong, email: sdszbl@163.com, blzhu@buaa.edu.cn ,date:2020-12-11 
    public class GeneticAlgorithm
    {
        #region about chromosome(individual)
        public int ChromosomeSize { get; set; }//Number of parameter(gene) in this chromosome(individual)

        public double[] LowerBd { get; set; }// The lower boundary of each parameter

        public double[] UpperBd { get; set; }// The upper boundary of each parameter

        public int[] DecimalPlace { get; set; }//Number of decimal points for each parameter
        #endregion

        #region about population
        public int[] PopulationSize { get; set; }//Number of chromosome(individual) in this population
        public double[] CrossoverProbability { get; set; }//
        public double[] MutationProbability { get; set; }
        #endregion


        #region about genetic Algorithm
        public int NumPopulation;//Number of populations in this GA
        public int CurrentIteration { get; set; }
        public int MaxIteration { get; set; }
        public List<Population> PopsOfGa { get; set; }
        public List<Population> PopsOfGaCX { get; set; }
        public List<Population> PopsOfGaMU { get; set; }
        public List<Chromosome> HistBestChromOfPops { get; set; }//The best individual in the evolution of a population
        //public List<Chromosome> HistBestChromOfGa{ get; set; }////Record the best individuals of the Ga in history with CurrentIteration
        #endregion

        #region local variable
        public int[] numOffspringsOfCross;
        public int[] numPointOfMute;
        #endregion

        public GeneticAlgorithm(int ChromosomeSize, double[] LowerBd, double[] UpperBd, int[] DecimalPlace,
            int[] PopulationSize, double[] CrossoverProbability, double[] MutationProbability,
            int NumPopulation, int CurrentIteration, int MaxIteration)
        {
            this.ChromosomeSize = ChromosomeSize;
            this.LowerBd = LowerBd;
            this.UpperBd = UpperBd;
            this.DecimalPlace = DecimalPlace;
            this.PopulationSize = PopulationSize;
            this.CrossoverProbability = CrossoverProbability;
            this.MutationProbability = MutationProbability;
            this.NumPopulation = NumPopulation;
            this.CurrentIteration = CurrentIteration;
            this.MaxIteration = MaxIteration;

            this.numOffspringsOfCross = new int[NumPopulation];
            this.numPointOfMute = new int[NumPopulation];
            PopsOfGa = new List<Population>(NumPopulation);
            PopsOfGaCX = new List<Population>(NumPopulation);
            PopsOfGaMU = new List<Population>(NumPopulation);
            HistBestChromOfPops = new List<Chromosome>(NumPopulation);
            //HistBestChromOfGa = new List<Chromosome>(MaxIteration);

        }

        //初始化各种群
        public void PopsOfGaIni()
        {
            // Initialize all origin populations
            for (int i = 0; i < NumPopulation; i++)
            {
                Population popofGa = new Population(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                             PopulationSize[i], CrossoverProbability[i], MutationProbability[i],
                                             CurrentIteration, MaxIteration);// new a population
                popofGa.ChromsOfPopIni();//initialize the population's individuals
                //Console.WriteLine("PopsOfGa ini:");
                popofGa.ChromsOfPopSolveFit();//compute every individuals' Fitness
                //popofGa.ChromsOfPopCWFitAndChroms();
                //Console.WriteLine("PopsOfGa ini sort:");
                popofGa.FitSort();// sort individuals with fitness
                //popofGa.ChromsOfPopCWFitAndChroms();
                PopsOfGa.Add(popofGa);//add the population in the population list
            }


            // compute the Cross mating pool population's number(CrossoverProbability) and mute pool population's number(numPointOfMute)
            for (int i = 0; i < NumPopulation; i++)
            {
                numOffspringsOfCross[i] = Convert.ToInt32(2.0 * Math.Round(CrossoverProbability[i] * PopulationSize[i] / 2.0));
                numPointOfMute[i] = Convert.ToInt32(Math.Round(MutationProbability[i] * PopulationSize[i]));
            }

            // Initialize all cross mating pool populations
            for (int i = 0; i < NumPopulation; i++)
            {
                Population popofGacx = new Population(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                             numOffspringsOfCross[i], CrossoverProbability[i], MutationProbability[i],
                                             CurrentIteration, MaxIteration);
                popofGacx.ChromsOfPopIni();
                //Console.WriteLine("PopsOfGaCX ini:");
                popofGacx.ChromsOfPopSolveFit();
                // popofGacx.ChromsOfPopCWFitAndChroms();
                //Console.WriteLine("PopsOfGaCX ini sort:");
                popofGacx.FitSort();
                //popofGacx.ChromsOfPopCWFitAndChroms();
                PopsOfGaCX.Add(popofGacx);
            }

            // Initialize all mute pool populations
            for (int i = 0; i < NumPopulation; i++)
            {
                Population popofGamu = new Population(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                             numPointOfMute[i], CrossoverProbability[i], MutationProbability[i],
                                             CurrentIteration, MaxIteration);
                popofGamu.ChromsOfPopIni();
                //Console.WriteLine("PopsOfGaMU ini:");
                popofGamu.ChromsOfPopSolveFit();
                //popofGamu.ChromsOfPopCWFitAndChroms();
                //Console.WriteLine("PopsOfGaMU ini sort:");
                popofGamu.FitSort();
                //popofGamu.ChromsOfPopCWFitAndChroms();
                PopsOfGaMU.Add(popofGamu);
            }


            // Initialize HistBestChromOfGa
            Population popsBestHist = new Population(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                             NumPopulation, CrossoverProbability[0], MutationProbability[0],
                                             CurrentIteration, MaxIteration);
            popsBestHist.ChromsOfPopIni();
            //Console.WriteLine("popsBestHist ini:");
            popsBestHist.ChromsOfPopSolveFit();
            //popsBestHist.ChromsOfPopCWFitAndChroms();
            //Console.WriteLine("popsBestHist ini sort:");
            popsBestHist.FitSort();
            //popsBestHist.ChromsOfPopCWFitAndChroms();
            HistBestChromOfPops.AddRange(popsBestHist.ChromsOfPop);


            // Initialize HistBestChromOfGa
            Population popofGaHist = new Population(ChromosomeSize, LowerBd, UpperBd, DecimalPlace,
                                             MaxIteration, CrossoverProbability[0], MutationProbability[0],
                                             CurrentIteration, MaxIteration);
            popofGaHist.ChromsOfPopIni();
            //Console.WriteLine("popsBestHist ini:");
            popofGaHist.ChromsOfPopSolveFit();
            //popofGaHist.ChromsOfPopCWFitAndChroms();
            //Console.WriteLine("popsBestHist ini sort:");
            popofGaHist.FitSort();
            //popofGaHist.ChromsOfPopCWFitAndChroms();
            //diedaiList.HistBestChromOfGa.AddRange(popofGaHist.ChromsOfPop);
            //foreach (var item in collection)
            //{

            //}
            diedaiList.HistBestChromOfGa.AddRange(popofGaHist.ChromsOfPop);


        }






        public List<Chromosome> Execute()

        {
            //using select method 
            Selection SL = new Selection();
            SL.SelectMethodEvent += SL.Select_Tournament;
            //Chromosome result=SL.Select(Population pop);

            //using cross method
            Crossover CX = new Crossover();
            CX.CrossMethodEvent += CX.CrossOver_MIX;
            // List<Chromosome> result=CX.Cross(Chromosome mom, Chromosome dad);

            //using mute method
            Mutation MU = new Mutation();
            MU.MuteMethodEvent += MU.DRM;
            //Chromosome result = MU.Mute(Chromosome chrom);

            //using chaotic local search
            ChaoticLocalSearch LS = new ChaoticLocalSearch();
            LS.CLSMethodEvent += LS.ChaoticLS;





            #region All population evolutionary algebraic processes
            //Iterative evolution 
            Random ran = new Random(Guid.NewGuid().GetHashCode());
            List<Chromosome> result1 = new List<Chromosome>();
            Chromosome result2 = new Chromosome();
            int maxSameIt = 0;
            for (int it = 0; it < MaxIteration; it++)
            {
                #region one population
                for (int popi = 0; popi < NumPopulation; popi++)
                {
                    // Update iteration times
                    PopsOfGa[popi].CurrentIteration = it;//Current iteration of evolution
                    PopsOfGaCX[popi].CurrentIteration = it;
                    PopsOfGaMU[popi].CurrentIteration = it;
                    for (int k = 0; k < PopsOfGa[popi].PopulationSize; k++)
                    {
                        PopsOfGa[popi].ChromsOfPop[k].CurrentIteration = it;
                    }
                    //for (int k = 0; k < PopsOfGaCX[popi].ChromsOfPop.Count; k++)
                    for (int k = 0; k < PopsOfGaCX[popi].PopulationSize; k++)
                    {
                        PopsOfGaCX[popi].ChromsOfPop[k].CurrentIteration = it;
                    }
                    for (int k = 0; k < PopsOfGaMU[popi].PopulationSize; k++)
                    {
                        PopsOfGaMU[popi].ChromsOfPop[k].CurrentIteration = it;
                    }


                    //select and cross
                    for (int k = 0; k < numOffspringsOfCross[popi] - 1; k = k + 2)
                    {
                        //select operator
                        int Chroms1Index = SL.Select(PopsOfGa[popi]);
                        int Chroms2Index = SL.Select(PopsOfGa[popi]);

                        //cross operator
                        result1 = CX.Cross(PopsOfGa[popi].ChromsOfPop[Chroms1Index], PopsOfGa[popi].ChromsOfPop[Chroms2Index]);
                        PopsOfGaCX[popi].ChromsOfPop.AddRange(result1);

                    }
                    PopsOfGaCX[popi].ChromsOfPop.RemoveRange(0, numOffspringsOfCross[popi]);

                    //Console.WriteLine("PopsOfGaCX before solve:");
                    //PopsOfGaCX[popi].ChromsOfPopCWFitAndChroms();
                    PopsOfGaCX[popi].ChromsOfPopSolveFit();// * solve every individuals fitness
                    //Console.WriteLine("PopsOfGaCX after solve:");
                    //PopsOfGaCX[popi].ChromsOfPopCWFitAndChroms();

                    //mute operator
                    for (int k = 0; k < numPointOfMute[popi]; k++)
                    {
                        int Chroms1IndexMu = ran.Next(0, PopsOfGa[popi].ChromsOfPop.Count);
                        result2 = MU.Mute(PopsOfGa[popi].ChromsOfPop[Chroms1IndexMu]);
                        PopsOfGaMU[popi].ChromsOfPop.Add(result2);
                    }
                    PopsOfGaMU[popi].ChromsOfPop.RemoveRange(0, numPointOfMute[popi]);

                    //Console.WriteLine("PopsOfGaMU before solve:");
                    //PopsOfGaMU[popi].ChromsOfPopCWFitAndChroms();
                    //Console.WriteLine("PopsOfGaMU after solve:");
                    PopsOfGaMU[popi].ChromsOfPopSolveFit();
                    //PopsOfGaMU[popi].ChromsOfPopCWFitAndChroms();

                    //combine three population
                    for (int iCx = 0; iCx < PopsOfGaCX[popi].PopulationSize; iCx++)
                    {
                        PopsOfGa[popi].ChromsOfPop.Add(Chromosome.Clone<Chromosome>(PopsOfGaCX[popi].ChromsOfPop[iCx]));
                    }
                    for (int iMu = 0; iMu < PopsOfGaMU[popi].PopulationSize; iMu++)
                    {

                        PopsOfGa[popi].ChromsOfPop.Add(Chromosome.Clone<Chromosome>(PopsOfGaMU[popi].ChromsOfPop[iMu]));
                    }
                    //Console.WriteLine($"PopsOfGa+PopsOfGaCX+PopsOfGaMU[{popi}] solve but before sort:");
                    //PopsOfGa[popi].ChromsOfPopCWFitAndChroms();
                    //Console.WriteLine($"sort:");
                    //sort(ascending) with fitness
                    PopsOfGa[popi].FitSort();//
                    //PopsOfGa[popi].ChromsOfPopCWFitAndChroms();
                    // remove the bad ones
                    PopsOfGa[popi].ChromsOfPop.RemoveRange(PopulationSize[popi], (numOffspringsOfCross[popi] + numPointOfMute[popi]));
                    //Console.WriteLine($"PopsOfGa[{popi}]  after remove:");
                    //PopsOfGa[popi].ChromsOfPopCWFitAndChroms();

                    //Local search
                    if (it<0.333*MaxIteration || maxSameIt > (int)Math.Round(MaxIteration * 0.10))
                    {
                        PopsOfGa[popi].ChromsOfPop[0] = Chromosome.Clone<Chromosome>(LS.LocalSearch(PopsOfGa[popi].ChromsOfPop[0]));
                    }
                    PopsOfGa[popi].ChromsOfPop[0].CurrentIteration = it;

                    //elitism in a population
                    if (HistBestChromOfPops[popi].Fit > PopsOfGa[popi].ChromsOfPop[0].Fit)
                    {
                        HistBestChromOfPops[popi] = Chromosome.Clone<Chromosome>(PopsOfGa[popi].ChromsOfPop[0]);
                    }
                    else
                    {
                        //PopsOfGa[popi].ChromsOfPop[0] = (Chromosome)DeepClone(HistBestChromOfPops[popi]);//elitism
                        PopsOfGa[popi].ChromsOfPop[0] = Chromosome.Clone<Chromosome>(HistBestChromOfPops[popi]);//elitism
                    }
                    HistBestChromOfPops[popi].CurrentIteration = it;
                    HistBestChromOfPops[popi].averageOfPop = PopsOfGa[popi].averageOfPop;
                    HistBestChromOfPops[popi].standardDeviationOfPop = PopsOfGa[popi].standardDeviationOfPop;
                }
                #endregion popi


                //Take turns to swap the best:The best value of the next population gives the best value of the previous population
                if (NumPopulation == 1)
                {
                    //HistBestChromOfGa[it] = (Chromosome)DeepClone(HistBestChromOfPops[0]);
                    diedaiList.HistBestChromOfGa[it] = Chromosome.Clone<Chromosome>(HistBestChromOfPops[0]);
                }
                else
                {
                    //Chromosome tmepChromo = (Chromosome)DeepClone(PopsOfGa[0].ChromsOfPop[0]);
                    Chromosome tmepChromo = Chromosome.Clone<Chromosome>(PopsOfGa[0].ChromsOfPop[0]);
                    for (int popi = 0; popi < NumPopulation; popi++)
                    {
                        if (diedaiList.HistBestChromOfGa[it].Fit > PopsOfGa[popi].ChromsOfPop[0].Fit)
                        {
                            //HistBestChromOfGa[it] = (Chromosome)DeepClone(PopsOfGa[popi].ChromsOfPop[0]);
                            diedaiList.HistBestChromOfGa[it] = Chromosome.Clone<Chromosome>(PopsOfGa[popi].ChromsOfPop[0]);
                        }
                        else
                        {
                            //PopsOfGa[popi].ChromsOfPop[0] = (Chromosome)DeepClone(HistBestChromOfGa[it]);
                            PopsOfGa[popi].ChromsOfPop[0] = Chromosome.Clone<Chromosome>(diedaiList.HistBestChromOfGa[it]);
                        }

                        //if (PopsOfGa[popi + 1] != null)
                        if ((popi + 1) < NumPopulation)
                        {
                            //PopsOfGa[popi].ChromsOfPop[0] = (Chromosome)DeepClone(PopsOfGa[popi + 1].ChromsOfPop[0]);
                            PopsOfGa[popi].ChromsOfPop[0] = Chromosome.Clone<Chromosome>(PopsOfGa[popi + 1].ChromsOfPop[0]);
                        }
                        if (popi == (NumPopulation - 1) && popi != 0)
                        {
                            //PopsOfGa[NumPopulation - 1].ChromsOfPop[0] = (Chromosome)DeepClone(tmepChromo);
                            PopsOfGa[NumPopulation - 1].ChromsOfPop[0] = Chromosome.Clone<Chromosome>(tmepChromo);
                        }
                    }
                }

                if (it > (int)Math.Round(MaxIteration * 0.333))
                {
                    maxSameIt = 0;
                    for (int itsame = 0; itsame < (int)Math.Round(MaxIteration * 0.333); itsame++)
                    {
                        if (Math.Abs(diedaiList.HistBestChromOfGa[it].Fit - diedaiList.HistBestChromOfGa[it - itsame - 1].Fit) < 1e-30)
                        {
                            maxSameIt++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (maxSameIt > (int)Math.Round(MaxIteration * 0.15))
                    {
                        break;
                    }
                }

                //
                diedaiList.HistBestChromOfGaq.Enqueue(diedaiList.HistBestChromOfGa[it]);
                diedaiList.CurrentBestChrom = Chromosome.Clone<Chromosome>(diedaiList.HistBestChromOfGa[it]);



                //Ribbon1.formShowGAInfor.richTextBox1.AppendText($"iteration:{it},bestFit:{diedaiList.HistBestChromOfGa[it].Fit},parameters:");

                //foreach (double para in diedaiList.HistBestChromOfGa[it].chromosome)
                //{
                //    Ribbon1.formShowGAInfor.richTextBox1.AppendText($"{para} \t");
                //}
                //Ribbon1.formShowGAInfor.richTextBox1.AppendText($"\r\n");
                //Ribbon1.formShowGAInfor.richTextBox1.Update();

                //chart
                //Ribbon1.formShowGAInfor.chart1.Series[0].ChartType = SeriesChartType.Line;
                //Ribbon1.formShowGAInfor.chart1.Series[0].MarkerStyle = MarkerStyle.Circle;
                //Ribbon1.formShowGAInfor.chart1.Series[0].MarkerSize = 6;  //标志大小
                //Ribbon1.formShowGAInfor.chart1.Series[0].LegendText = "Error";
                //Ribbon1.formShowGAInfor.chart1.Series[0].ToolTip = "XValue：#VALX\n,YValue：#VALY";
                //Ribbon1.formShowGAInfor.chart1.ChartAreas[0].AxisX.Title = "Generation";
                //Ribbon1.formShowGAInfor.chart1.ChartAreas[0].AxisY.Title = " Error";
                ////Ribbon1.formShowGAInfor.chart1.Titles.Add("S01");
                //Ribbon1.formShowGAInfor.chart1.Titles[0].Text = "Iteration generation  vs. Error Cost";
                
                //this.chart1.Series[0].ChartType = SeriesChartType.Line;
                //this.chart1.Series[0].MarkerStyle = MarkerStyle.Circle;
                //this.chart1.Series[0].MarkerSize = 6;  //标志大小
                //this.chart1.Series[0].LegendText = "Error";
                //this.chart1.Series[0].ToolTip = "XValue：#VALX\n,YValue：#VALY";
                //this.chart1.ChartAreas[0].AxisX.Title = "Generation";
                //this.chart1.ChartAreas[0].AxisY.Title = " Error";
                ////Ribbon1.formShowGAInfor.chart1.Titles.Add("S01");
                //this.chart1.Titles[0].Text = "Iteration generation  vs. Error Cost";


                //Ribbon1.formShowGAInfor.chart1.Series[0].Points.AddXY(it, diedaiList.HistBestChromOfGa[it].Fit);
                //Ribbon1.formShowGAInfor.chart1.Update();

                //Console.WriteLine($"iteration:{it},bestFit:{HistBestChromOfGa[it].Fit}");
                //foreach (double para in HistBestChromOfGa[it].chromosome)
                //{
                //    Console.Write($"{para} ");
                //}
                //Console.WriteLine();
                //Console.WriteLine();


            }
            #endregion

            return diedaiList.HistBestChromOfGa;

        }




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


    public static class diedaiList
    {
        public static List<Chromosome> HistBestChromOfGa = new List<Chromosome>();////Record the best individuals of the Ga in history with CurrentIteration
        public static Queue<Chromosome> HistBestChromOfGaq = new Queue<Chromosome>();
        public static Chromosome CurrentBestChrom = new Chromosome();

    }

}
