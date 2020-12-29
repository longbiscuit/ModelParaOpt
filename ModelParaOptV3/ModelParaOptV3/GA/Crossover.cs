using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{

    /// <summary>
    /// Declares a delegate that references some crossover method
    /// </summary>
    /// <param name="mom">parent 1</param>
    /// <param name="dad">parent 2</param>
    /// <returns>two children</returns>
    public delegate List<Chromosome> CrossDelegateEventHandler(Chromosome mom, Chromosome dad);
    public class Crossover
    {
        /// <summary>
        /// Encapsulate the delegate with an event
        /// </summary>
        public event CrossDelegateEventHandler CrossMethodEvent;
        /// <summary>
        /// All kinds of cross operations call this function
        /// </summary>
        /// <param name="mom">parent 1</param>
        /// <param name="dad">parent 2</param>
        /// <returns>two children</returns>
        public List<Chromosome> Cross(Chromosome mom, Chromosome dad)
        {
            List<Chromosome> result = new List<Chromosome>();

            result = CrossMethodEvent?.Invoke(mom, dad);// "?.Invoke" means that the CrossMethodEvent will executed if CrossMethodEvent!=null 
            return result;
        }

        public List<Chromosome> CrossOver_MIX(Chromosome mom, Chromosome dad)
        {
            List<Chromosome> result ;
            if (myRandom.NextDouble(0,1)<mom.CrossoverProbability)
            {
                if (mom.GeneEqual(dad))
                {
                    result = CrossOver_SPX(mom, dad);
                }
                else
                {
                    result = CrossOver_SBX(mom, dad);
                }
            }
            else
            {
                result = CrossOver_SPX(mom, dad);
            }

            return result;
            
        }

            /// <summary>
            /// 1. Simulate binary crossover operator
            /// </summary>
            /// <param name="mom">parent 1</param>
            /// <param name="dad">parent 2</param>
            /// <returns>two children</returns>
            public List<Chromosome> CrossOver_SBX(Chromosome mom, Chromosome dad)
        {
            List<Chromosome> result = new List<Chromosome>();
            //if (mom == dad)
            //    Debug.WriteLine("Oh shet! are the mom and dad same!?");


            //Chromosome child1 = (Chromosome)DeepClone(mom);//This method doesn't work
            //Chromosome child2 = (Chromosome)DeepClone(dad);

            Chromosome child1 = Chromosome.Clone<Chromosome>(mom);//use this method doing deep copy
            Chromosome child2 = Chromosome.Clone<Chromosome>(dad);

            double EPS = 1E-14;
            double etaC = mom.CurrentIteration/ mom.MaxIteration * 5.0;//etaC=2~5
            Random ran = new Random(Guid.NewGuid().GetHashCode());//http://www.splaybow.com/post/csharp-generate-random-num.html
            double y1, y2, yl, yu, rnd, beta, alpha, betaq, c1, c2;
            for (int qq = 0; qq < mom.chromosome.Length; qq++)
            {
                if (ran.NextDouble() < 0.5)
                {
                    if (Math.Abs(mom.chromosome[qq] - dad.chromosome[qq]) > EPS)
                    {
                        if (mom.chromosome[qq] < dad.chromosome[qq])
                        {
                            y1 = mom.chromosome[qq];
                            y2 = dad.chromosome[qq];
                        }

                        else
                        {
                            y1 = dad.chromosome[qq];
                            y2 = mom.chromosome[qq];
                        }
                        yl = mom.LowerBd[qq];
                        yu = mom.UpperBd[qq];
                        rnd = ran.NextDouble();
                        beta = 1.0 + (2.0 * (y1 - yl) / (y2 - y1));
                        alpha = 2.0 - Math.Pow(beta, (-(etaC + 1.0)));
                        if (rnd <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rnd * alpha), (1.0 / (etaC + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rnd * alpha)), (1.0 / (etaC + 1.0)));
                        }
                        c1 = 0.5 * ((y1 + y2) - betaq * (y2 - y1));
                        beta = 1.0 + (2.0 * (yu - y2) / (y2 - y1));
                        alpha = 2.0 - Math.Pow( beta, (-(etaC + 1.0)));

                        if (rnd <= (1.0 / alpha))
                        {
                            betaq = Math.Pow((rnd * alpha), (1.0 / (etaC + 1.0)));
                        }
                        else
                        {
                            betaq = Math.Pow((1.0 / (2.0 - rnd * alpha)), (1.0 / (etaC + 1.0)));
                        }
                        c2 = 0.5 * ((y1 + y2) + betaq * (y2 - y1));
                        if (c1 < yl) c1 = yl;
                        if (c2 < yl) c2 = yl;
                        if (c1 > yu) c1 = yu;
                        if (c2 > yu) c2 = yu;

                        if (ran.NextDouble() <= 0.5)
                        {
                            child1.chromosome[qq] = c2;
                            child2.chromosome[qq] = c1;
                        }

                        else
                        {
                            child1.chromosome[qq] = c1;
                            child2.chromosome[qq] = c2;
                        }



                    }
                    else
                    {
                        child1.chromosome[qq] = mom.chromosome[qq];
                        child2.chromosome[qq] = dad.chromosome[qq];
                    }
                }
                else
                {
                    child1.chromosome[qq] = mom.chromosome[qq];
                    child2.chromosome[qq] = dad.chromosome[qq];
                }

            }
            child1.Fit = double.MaxValue;//Fit need re-compute,so here give a great value
            child2.Fit = double.MaxValue;
            //child1.CheckBoundary();
            child1.SetChromDecimalPlace();
            //child2.CheckBoundary();
            child2.SetChromDecimalPlace();

            result.Add(child1);
            result.Add(child2);
            return result;
        }

        /// <summary>
        /// 2. Simplex crossover:Da Ronco C C, Benini E. GeDEA-II: A simplex crossover based evolutionary algorithm
        /// including the genetic diversity as objective[J]. Engineering Letters, 2013, 21(1): 23-35.
        /// </summary>
        /// <param name="mom">parnet 1</param>
        /// <param name="dad">parent 2</param>
        /// <returns>two children</returns>
        public List<Chromosome> CrossOver_SPX(Chromosome mom, Chromosome dad)
        {
            List<Chromosome> result = new List<Chromosome>();
            Chromosome child1 = Chromosome.Clone<Chromosome>(mom);//use this method doing deep copy
            Chromosome child2 = Chromosome.Clone<Chromosome>(dad);
            double Refl = myRandom.NextDouble(0,0.5);//Generates a uniform random number between 0 and 0.5
            double Refl2 = myRandom.NextDouble(0.5, 1);
            double n = 2.0;
            double M;
            if (mom.Fit<dad.Fit)
            {
                for (int i = 0; i < mom.ChromosomeSize; i++)
                {
                    M = mom.chromosome[i] / n;
                    child1.chromosome[i] = (1 + Refl) * M - Refl * dad.chromosome[i];
                    child2.chromosome[i] = (1 + Refl2) * M - Refl2 * dad.chromosome[i];
                }
            }
            else
            {
                for (int i = 0; i < mom.ChromosomeSize; i++)
                {
                    M = dad.chromosome[i] / n;
                    child1.chromosome[i] = (1 + Refl) * M - Refl * mom.chromosome[i];
                    child2.chromosome[i] = (1 + Refl2) * M - Refl2 * mom.chromosome[i];
                }
            }
            child1.Fit = double.MaxValue;//Fit need re-compute,so here give a great value
            child2.Fit = double.MaxValue;
            child1.CheckBoundary();
            child1.SetChromDecimalPlace();
            child2.CheckBoundary();
            child2.SetChromDecimalPlace();

            result.Add(child1);
            result.Add(child2);
            return result;
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
}
