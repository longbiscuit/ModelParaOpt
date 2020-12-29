using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    public static class Fitness
    {
        public static double Err;
        //public static double Fit;

       

        /// <summary>
        /// 1.测试函数的误差函数
        /// </summary>
        /// <param name="chrom">parameters</param>
        /// <returns>Error</returns>
        public static double SolveErr(double[] chrom)
        {
            //Err = MathTestFunc.TestFunc1(chrom);
            //Err =MathTestFunc.TestFunc2(chrom);
            //Err =MathTestFunc.TestFunc3(chrom);
            //Err = MathTestFunc.TestFunc4(chrom);
            //Err = MathTestFunc.TestFunc5(chrom);

            //ExpModelERF ERFClass = new ExpModelERF();
            //Err=ERFClass.ComputeAllError(chrom);

            //最好有个判断，如果当前chrom等于记录的历史上最好chrom，则直接取其适应度函数。

            Err=ExpModelERF.ComputeAllError(chrom);
            if (Double.IsNaN(Err))
            {
                Err = 1e5;
            }
            return Err;
        }




        

    }
}
