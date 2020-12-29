using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn.ConstitiveModel
{
    public class DuncanEB
    {
        public static double[] LowerBound = new double[] { 10, 0.01, 0.5, 0.0, 0.01, 0.00, 10, 0.01 };
        public static double[] UpperBound = new double[] { 2000, 0.99, 0.99, 1000.0, 60.0, 20.0, 2000, 0.99 };
        public static int[] DecimalNumber = new int[] { 1, 2, 2, 1, 1, 2, 1, 2 };

        public static string ConstitiveModelShortName = "DuncanEB";
        public static string ConstitiveModelLongName = "Duncan-Chang Elastic-Bulk Modulus model";

        public static int[,] arrParaNum = new int[,] { { 1 }, { 2 }, { 3 }, { 4 }, { 5 }, { 6 }, { 7 }, { 8 } };
        public static string[,] ParaName = new string[,] { { "K" }, { "n" }, { "Rf" }, { "C" }, { "φ" }, { "Δφ" }, { "Kb" }, { "m" } };

        // Evolution of constitutive models

        double C11, C12, C13, C21, C22, C23, C31, C32, C33, dS1, dS2, dS3;
        double de2, de3;
        protected int number, un, num;
        EnumClass.TestType currentTestType;
        double[] de1;//
        double[] e2;
        double[] e3;
        double[] S1;//σ1, real stress in Cauchy space
        double[] S2;//σ2
        double[] S3;//σ3
        double[] S11;//σ1wave, σ1 in Transformational stress space 
        double[] S22;//σ2wave
        double[] S33;//σ3wave
        double[] p;//mean principal stress in Transformational stress space 
        double[] q;//Generalized shear stress in Transformational stress space 
        double[] ed;//剪切体变
        double[] n;//应力比


        private double[] e1;//ε1, Maximum principal strain
        public double[] ModelE1
        {
            get { return e1; }
            set { e1 = value; }
        }
        private double[] qz;// Generalized shear stress in Cauchy space
        public double[] ModelQz
        {
            get { return qz; }
            set { qz = value; }
        }

        private double[] pz;//mean principal stress in Cauchy space (pz=p is equal forever)
        public double[] ModelPz
        {
            get { return pz; }
            set { pz = value; }
        }

        private double[] ev;// Volume strain εv
        public double[] ModelEv
        {
            get { return ev; }
            set { ev = value; }
        }
        private double[] u;// Pore pressure
        public double[] ModelU
        {
            get { return u; }
            set { u = value; }
        }
        double[] nz;// yita=η=qz/pz;
        public double[] ModelYita
        {
            get { return nz; }
            set { nz = value; }
        }

        private double[] eVoidRatio;//void ratio
        public double[] ModelVoidRatio
        {
            get { return eVoidRatio; }
            set { eVoidRatio = value; }
        }


        /// <summary>
        /// initialize the variable of CSUH constitive model
        /// </summary>
        /// <param name="totale1">the value of total ε1</param>
        /// <param name="number">evolution steps</param>
        /// <param name="testType">give the test type</param>
        public void ModelIni(double totale1, int number, EnumClass.TestType testType)
        {
            de1 = new double[number];
            e2 = new double[number];
            e3 = new double[number];
            S1 = new double[number];
            S2 = new double[number];
            S3 = new double[number];
            S11 = new double[number];
            S22 = new double[number];
            S33 = new double[number];
            p = new double[number];
            q = new double[number];
            ed = new double[number];
            n = new double[number];


            e1 = new double[number];
            qz = new double[number];
            pz = new double[number];
            ev = new double[number];
            u = new double[number];
            nz = new double[number];
            eVoidRatio = new double[number];

            e1[0] = 0.0;

            double tempDouble = totale1 / number;
            for (int i = 0; i < de1.Length; i++)
            {
                de1[i] = tempDouble;
            }
            currentTestType = testType;
            this.number = number;
        }

        /// <summary>
        /// according parameters of CSUH and initial conditions(sc, e0 and b),
        /// calculate the evolution of the remaining strains(ε2,ε3) and 
        /// stresses(σ1,σ2,σ3) with the maximum principal strain ε1, 
        /// and then get the values of the other derived variables(p,q,u,void ratio,yita)
        /// </summary>
        /// <param name="para">parameters of CSUH</param>
        /// <param name="sc">surrounding confining pressure(kPa)</param>
        /// <param name="e0">initial void ratio</param>
        /// <param name="b">principal stress coefficient,b=(σ2-σ3)/(σ1-σ3)=(dσ2-dσ3)/(dσ1-dσ3)</param>
        public void ModelCompute(double[] para, double sc, double e0, double b)
        {
            double K = para[0]; double n = para[1]; double Rf = para[2]; double C = para[3]; double PHI = para[4];
            double DPHI = para[5]; double Kb = para[6]; double m = para[7];
            double PA = 101.325, Kur = 2.0 * Kb, SSmax = 0.0;

            double PHIP, A, SL, Et, Eur, B, SS, Etp, v, CII, CIJ;
            S1[0] = sc; S2[0] = sc; S3[0] = sc;
            eVoidRatio[0] = e0;
            for (int i = 0; i < number - 1; i++)
            {
                qz[i] = 1.0 / Math.Sqrt(2.0) * Math.Sqrt((Math.Pow(S1[i] - S2[i], 2)) + (Math.Pow(S2[i] - S3[i], 2)) + (Math.Pow(S1[i] - S3[i], 2)));
                pz[i] = (S1[i] + S2[i] + S3[i]) / 3.0;
                nz[i] = qz[i] / pz[i];
                PHIP = PHI - DPHI * Math.Log10(S3[0] / PA);
                SL = ((1 - Math.Sin(PHIP / 180.0 * Math.PI)) * (S1[i] - S3[i])) / (2.0 * C * Math.Cos(PHIP / 180.0 * Math.PI) + 2 * S3[i] * Math.Sin(PHIP / 180.0 * Math.PI));
                Et = K * PA * Math.Pow(S3[i] / PA, n) * Math.Pow(1 - Rf * SL, 2);
                Eur = Kur * PA * Math.Pow(S3[i] / PA, n);
                B = Kb * PA * Math.Pow(S3[i] / PA, m);
                SS = SL * Math.Pow(S3[i] / PA, 0.25);
                if (SS >= SSmax)
                {
                    Etp = Et;
                }
                else if (SS < 0.75 * SSmax)
                {
                    Etp = Eur;
                }
                else// (0.75 * SSmax <= SS && SS < SSmax)
                {
                    Etp = Et + ((SSmax - SS) / (0.25 * SSmax)) * (Eur - Et);
                }

                if (SSmax < SS) SSmax = SS;
                v = 0.5 * (1 - Etp / (3.0 * B));

                // 修正泊松比
                if (v < 0.001) v = 0.001;
                if (v > 0.499) v = 0.499;


                A = 1.0 / Etp;
                CII = 1 * A;
                CIJ = -v * A;
                C11 = CII;// 柔度矩阵
                C12 = CIJ;
                C13 = CIJ;
                C21 = CIJ;
                C22 = CII;
                C23 = CIJ;
                C31 = CIJ;
                C32 = CIJ;
                C33 = CII;
                //判断是哪种实验    
                switch (currentTestType)
                {
                    case EnumClass.TestType.CU:
                        {
                            // Conventional triaxial undrained shear test
                            //常规三轴固结不排水剪切试验（先等向压缩固结排水到指定围压，再进行不排水剪切试验到破坏），常规三轴是指σ2=σ3
                            dS1 = de1[i] / ((C11 + b * C12) - (C11 + C21 + C31 + b * (C12 + C22 + C32)) * ((1 - b) * C12 + C13) / (C13 + C23 + C33 + (1 - b) * (C12 + C22 + C32)));
                            dS3 = -(C11 + C21 + C31 + b * (C12 + C22 + C32)) * dS1 / (C13 + C23 + C33 + (1 - b) * (C12 + C22 + C32));
                            dS2 = b * dS1 + (1 - b) * dS3;//这个根据b的定义求导可得
                            de2 = (C21 + b * C22) * dS1 + ((1 - b) * C22 + C23) * dS3;
                            de3 = (C31 + b * C32) * dS1 + ((1 - b) * C32 + C33) * dS3;
                        }
                        break;
                    case EnumClass.TestType.ConstP:
                        {
                            //Conventional triaxial consolidation drainage shear test with  Const mean principal pressure p
                            //常规三轴固结排水等p剪切试验
                            dS1 = (b - 2) * de1[i] / (-2 * C11 + b * C11 + C12 - 2 * b * C12 + C13 + b * C13);
                            dS2 = (2 * b - 1) * de1[i] / (2 * C11 - b * C11 - C12 + 2 * b * C12 - C13 - b * C13);
                            dS3 = (1 + b) * de1[i] / (-2 * C11 + b * C11 + C12 - 2 * b * C12 + C13 + b * C13);
                            de2 = C21 * dS1 - C22 * dS1 - C22 * dS3 + C23 * dS3;
                            de3 = C31 * dS1 - C32 * dS1 - C32 * dS3 + C33 * dS3;

                        }
                        break;
                    case EnumClass.TestType.CD:
                        {
                            //Conventional triaxial Consolidation Drainage shear test with  Const surrounding confine pressure σ3
                            //常规三轴固结排水围压不变的剪切试验
                            dS1 = de1[i] / (C11 + b * C12);
                            dS2 = b * de1[i] / (C11 + b * C12);
                            dS3 = 0.0;
                            de2 = (C21 + b * C22) * dS1;
                            de3 = (C31 + b * C32) * dS1;
                        }
                        break;
                    case EnumClass.TestType.K0:
                        {
                            //Lateral compression drainage test
                            //侧限压缩固结试验
                            dS1 = (C22 - b * C22 + C23) * de1[i] / (b * (C12 * C21 - C11 * C22 - C13 * C22 + C12 * C23) - C13 * C21 + C11 * C22 + C11 * C23 - C12 * C21);
                            dS2 = (-C21 + b * (C21 + C23)) * de1[i] / (b * (C12 * C21 - C11 * C22 - C13 * C22 + C12 * C23) - C13 * C21 + C11 * C22 + C11 * C23 - C12 * C21);
                            dS3 = (C21 + b * C22) * de1[i] / (b * (C12 * C21 + C11 * C22 + C13 * C22 - C12 * C23) + C13 * C21 - C11 * C22 - C11 * C23 + C12 * C21);
                            de2 = 0;
                            de3 = 0;
                        }
                        break;
                    case EnumClass.TestType.IsoCom:
                        {
                            //Isotropic compression drainage test, principal stress coefficient b must be 0.0;
                            //等向压缩固结试验
                            dS1 = de1[i] / (C11 + C12 + C13);
                            dS2 = dS1;
                            dS3 = dS1;
                            de2 = (C21 + C22 + C23) * dS1;
                            de3 = (C31 + C32 + C33) * dS1;
                        }
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("The type of experiment was not given !");
                        break;
                }

                S1[i + 1] = S1[i] + dS1;
                S2[i + 1] = S2[i] + dS2;
                S3[i + 1] = S3[i] + dS3;

                e1[i + 1] = e1[i] + de1[i];
                e2[i + 1] = e2[i] + de2;
                e3[i + 1] = e3[i] + de3;
                ev[i + 1] = e1[i + 1] + e2[i + 1] + e3[i + 1];
                ed[i + 1] = Math.Sqrt(2.0) / 3.0 * Math.Sqrt(Math.Pow((e1[i + 1] - e2[i + 1]), 2) + Math.Pow((e1[i + 1] - e3[i + 1]), 2) + Math.Pow((e3[i + 1] - e2[i + 1]), 2));
                eVoidRatio[i + 1] = e0 - ev[i + 1] * (1 + e0);//孔隙比

                //qz[i+1] = 1 / Math.Sqrt(2) * Math.Sqrt(Math.Pow((S1[i+1] - S2[i+1]), 2) + Math.Pow((S2[i+1] - S3[i+1]), 2) + Math.Pow((S1[i+1] - S3[i+1]), 2));//真实应力空间广义剪应力
                //pz[i+1] = (S1[i+1] + S2[i+1] + S3[i+1]) / 3.0;//
                if (currentTestType == EnumClass.TestType.CU)
                {
                    //method 1:u=p0'-p'=s30'-s3
                    u[i + 1] = S3[0] - S3[i + 1];

                    //method 2:u=qz/3+sc-pz
                    //u[i + 1] = qz[i + 1] / 3.0 + sc - pz[i + 1];
                }
                else
                {
                    u[i + 1] = 0.0;
                }

                if (pz[i] < 0 || eVoidRatio[i] < 0)
                {
                    break;
                }

                //for (un = 1; un < number; un++)
                //{
                //    if (pz[un] < 0)
                //    {
                //        num = un - 1;
                //        break;
                //    }
                //}
                //if (un == number - 1)
                //{
                //    num = number - 1;
                //}
            } //for
        }//compute
    }//class 
}
