using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelParaOptExcelAddIn.GA
{
    // https://zhuanlan.zhihu.com/p/150822796
    public static class MathTestFunc
    {
        /// <summary>
        /// 1. sum of squares
        /// </summary>
        /// <param name="para">all parameters range:[-100,100],best para:(0,0)</param>
        /// <returns>best fitness:0</returns>
        public static double TestFunc1(double[] para)
        {
            int numPara = para.Length;
            double Err = 0.0;
            for (int i = 0; i < numPara; i++)
            {
                Err += para[i] * para[i];
            }
            return Err;
        }

        /// <summary>
        /// 2. 程先云：柴福鑫, 黄诗峰, 程先云. 堰塞湖应急管理三维 GIS 系统开发及应用[J]. 中国水利水电科学研究院学报, 2013, 11(3): 221-225.
        /// </summary>
        /// <param name="x1">para1 [0,50],best para:20.4786412285915</param>
        /// <param name="x2">para2 [0,50],best para:25.0660285675803</param>
        /// <returns>best fitness:-715.775061146147</returns>
        ///https://www.zhihu.com/question/60356140/answer/1280109817
        public static double TestFunc2(double[] para)
        {
            double x1 = para[0];
            double x2 = para[1];
            return -40 * x1 * Math.Cos(x2) - 35 * x2 * Math.Sin(x1)
                + 15 * Math.Pow(x1, 2) - Math.Sin(x1) * 20
                + 12 * Math.Pow(x2, 2) - 25 * x1 * x2
                * Math.Sin(x1 * Math.Pow(x2, 2) * Math.Cos(x1 + x2));
        }

        /// <summary>
        /// 3. Booth Function http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page816.htm
        /// </summary>
        /// <param name="para">2 parameters range：[-10,10],best para:(1,3)</param>
        /// <returns>best fitness:0</returns>
        public static double TestFunc3(double[] para)
        {
            double x1 = para[0];//[-10,10]
            double x2 = para[1];//[-10,10]
            return Math.Pow((x1 + 2 * x2 - 7), 2) + Math.Pow((2 * x1 + x2 - 5), 2);
        }


        //http://www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page1361.htm
        /// <summary>
        /// 4. Easom Function
        /// </summary>
        /// <param name="para">2 parameters range:[-100,100],best para:(PI,PI)</param>
        /// <returns>best fitness:-1</returns>
        public static double TestFunc4(double[] para)
        {
            double x1 = para[0];//[-Math.PI,Math.PI]
            double x2 = para[1];//[-Math.PI,Math.PI]
            return -Math.Cos(x1) * Math.Cos(x2) * Math.Exp(-(x1 - Math.PI) * (x1 - Math.PI)
                - (x2 - Math.PI) * (x2 - Math.PI));
        }

        // https://link.zhihu.com/?target=http%3A//www-optima.amp.i.kyoto-u.ac.jp/member/student/hedar/Hedar_files/TestGO_files/Page1621.htm
        /// <summary>
        /// 5. Hump Function
        /// </summary>
        /// <param name="para">2 parameters range:[-5,5],best para:(0.0898,-0.7126),(-0.0898,0.7126)</param>
        /// <returns>best fitness:0</returns>
        public static double TestFunc5(double[] para)
        {
            double x1 = para[0];//[-Math.PI,Math.PI]
            double x2 = para[1];//[-Math.PI,Math.PI]
            return 1.0316285 + 4 * x1 * x1 - 2.1 * Math.Pow(x1, 4) + 
                Math.Pow(x1, 6) / 3 + x1 * x2 - 4 * x2 * x2 + 4 * Math.Pow(x2, 4);
        }



    }
}
