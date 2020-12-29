using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn
{

    //类：
    //类名:首字母大写+驼峰式命名 eg:PetShop.cs;AssemblyInfo.cs
    //私有变量名：_首字母小写+驼峰式命名 eg:_publishTime;_rootCategoryId
    //公共属性名：首字母大写+驼峰式命名 eg:Description;PublishTime

    //函数：
    //函数名：首字母大写+驼峰式命名 eg:GetItemByProduct
    //参数名：首字母小写+驼峰式命名 eg:userId,itemInfo

    public class ExpModelData
    {
        public ExpModelData()
        {

        }

        public string WstName;
        public int TestType;//test tpye,1-ISO,2-CD,3-CU,4-K0,5-ConstP
        public EnumClass.TestType CurrentTestType;//test tpye,1-ISO,2-CD,3-CU,4-K0,5-ConstP

        public int SerialNumber;//  experiment data serial number,1~10
        public int IsParticipateCompute;//this group data is paticipate in GA optimized compute

        public double ISOIniConfiningPressure;//The initial confining pressure is known for the isotropic compression phase
        public double ISOIniVoidRatio;//The initial void ratio is known for the isotropic compression phase
        public double b;// intermediate principal stress coefficient

        public double ShearIniConfiningPressure;//Initial confining pressure during shearing
        public double ShearIniVoidRatio; //Initial void ratio during shearing


        public double[,] ExpData;//Experiment data
        public double[] ExpE1,ExpQ, ExpP, ExpEv, ExpU, ExpYita, ExpVoidRatio;
        public double MaxE1,MaxQ,MaxP,MaxEv,MaxU,MaxYita,MaxVoidRatio;//the maximum of e1(maximum principal strain  ε1(%) )

        public double[,] ModelData;//model theory data Compare Experiment data, have the same shape of ExpData
        public double[] ModelE1, ModelQ, ModelP, ModelEv, ModelU, ModelYita, ModelVoidRatio;
        public int[] SameE1Index;//e1(ModelData[ ,0]) index of ModelData , which ensure the e1 of ModelData close to ExpData, and then put the e1 into the ModelDataCE 

        public double[,] ModelDataOutput;//model theory data of different constitive model, usually have 3000 rows





        /// <summary>
        /// find max column element of each group experiment data
        /// </summary>
        /// <param name="arr">a group experiment data</param>
        /// <param name="column">which column of arr to find the max element</param>
        /// <returns>max maxElement</returns>
        public static double FindColumnMax(double[,] arr,int column)
        {
            double maxElement = 0;
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                if (maxElement < arr[i, column])
                {
                    maxElement = arr[i, column];
                }
            }
            return maxElement;
        }



        /// <summary>
        /// After we have calculated the theoretical value, to perform the least squares algorithm, 
        /// we need to find the index of the theoretical e1 value equal to the experimental e1 value 
        /// </summary>
        /// <param name="step">The number of steps to calculate the constitutive theory simulation value</param>
        /// <param name="maxE1">The maximum first principal strain of test value </param>
        /// <param name="excessE1">The calculate  theory e1 value is greater excessE1 than the experiment(test) value</param>
        /// <param name="arr">A group of experiment data</param>
        /// <returns></returns>
        public static int[] findSameE1Index(int step, double maxE1,double excessE1, double[,] arr)
        {
            int rows = arr.GetLength(0);
            int[] index = new int[rows];
            double  maxE1AddexcessE1 = maxE1 + excessE1;// The theoretical e1 value is 5% higher than the experimental e1 value
            double de1 = maxE1AddexcessE1 / step;
            double[] e1 = new double[step];
            e1[0] = 0.0;
            for (int i = 1; i < step; i++)
            {
                e1[i] = e1[i - 1] + de1;
            }

            double diffValue1, diffValue2;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < step-1; j++)
                {
                    diffValue1 = e1[j] - arr[i, 0];
                    diffValue2 = e1[j + 1] - arr[i, 0];
                    if (diffValue1<=0.0 && diffValue2>=0.0)//Squeeze Theorem
                    {
                        // get the theory e1 index that nearst to the given experiment e1 value
                        if (Math.Abs(diffValue1)< Math.Abs(diffValue2))
                        {
                            index[i] = j;
                        }
                        else
                        {
                            index[i] = j+1;
                        }
                        break;
                    }
                }
            }
            return index;
        }

        public static double[,] ReadinDatad100(double[,] arr)
        {
            int rows = arr.GetLength(0);
            int columns = arr.GetLength(1);
            double[,] newArr = new double[rows, columns];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (j==0||j==3)// is e1 or ev
                    {
                        newArr[i, j] = arr[i,j]/100.0;
                    }
                    else
                    {
                        newArr[i, j] = arr[i, j];
                    }
                }
            }
            return newArr;
        }

        /// <summary>
        /// Give a column of a two-dimensional array to a one-dimensional array
        /// </summary>
        /// <param name="column">Specifies the number of columns in a two-dimensional array</param>
        /// <param name="arr">A two-dimensional array</param>
        /// <returns>A one-dimensional array of the columns</returns>
        public static double[] ParseColumn(double[,] arr, int column)
        {
            double[] columnVector = new double[arr.GetLength(0)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                columnVector[i] = arr[i,column];
            }
            return columnVector;
        }



    }
}
