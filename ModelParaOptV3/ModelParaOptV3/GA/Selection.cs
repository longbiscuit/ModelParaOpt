using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    public delegate int SelectDelegateEventHandler(Population pop);
    public class Selection
    {
        public Selection()
        {
            TournamentNum = 2;
        }
        public int TournamentNum { get; set; }//How many elements(chromosome) are selected for tournament selection?
        public event SelectDelegateEventHandler SelectMethodEvent;

        public int Select(Population pop)
        {
            
            return (int)(SelectMethodEvent?.Invoke(pop));
        }

        /// <summary>
        /// 1. Tournament selection operator
        /// </summary>
        /// <param name="pop">population: all individuals</param>
        /// <returns>A Chromosome</returns>
        public int Select_Tournament(Population pop)
        {
            //随机生成两个不重复的整数，区间为[0,pop.]
            int minFitIndex=0;
            int[] selectIndex; 
            selectIndex = myRandom.NextUnique(0, pop.ChromsOfPop.Count - 1, TournamentNum);
            for (int i = 0; i < TournamentNum-1; i++)
            {
                //越小越好
                if (pop.ChromsOfPop[selectIndex[i]].Fit< pop.ChromsOfPop[selectIndex[i+1]].Fit)
                {
                    minFitIndex = selectIndex[i];
                }
                else
                {
                    minFitIndex = selectIndex[i+1];
                }
            }
            //return pop.ChromsOfPop[minFitIndex];
            return minFitIndex;
        }

    }
}
