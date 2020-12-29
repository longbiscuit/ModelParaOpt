using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn.ConstitiveModel
{
    public class UH
    {
        public static double[] LowerBound = new double[] { 0.5, 0.1, 0.0001, 0.001, 0.400 };
        public static double[] UpperBound = new double[] { 2.00, 0.4, 0.2000, 0.650, 3.500 };
        public static int[] DecimalNumber = new int[] { 2, 1, 4, 3, 3 };

        public static string ConstitiveModelShortName = "UH";
        public static string ConstitiveModelLongName = "Unified Harding model for Clays";

        public static int[,] arrParaNum = new int[,] { { 1 }, { 2 }, { 3 }, { 4 }, { 5 } };
        public static string[,] ParaName = new string[,] { { "M" }, { "ν" }, { "κ" }, { "λ" }, { "N" } };


        // Evolution of constitutive models
        double ck, cp, Zb, ps;
        double E, C11, C12, C13, C21, C22, C23, C31, C32, C33, dS1, dS2, dS3;
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
        double[] kesi;
        double[] Mf;
        double[] Mc;

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
        /// initialize the variable of UH constitive model
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
            kesi = new double[number];
            Mf = new double[number];
            Mc = new double[number];

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
            double M = para[0]; double v = para[1]; double kapa = para[2]; double landa = para[3]; double N = para[4];
            double Zc = N; double Ze = Zc; double x = 0.0; double m = 0.0;
            S1[0] = sc + 0.00000001; //
            S2[0] = sc;
            S3[0] = sc;//
            eVoidRatio[0] = e0;//initial void ratio
            ck = kapa / (1 + e0);
            cp = (landa - kapa) / (1 + e0);
            Zb = Zc - b * (Zc - Ze);//in condition of compress test
            ps = Math.Exp((N - Zb) / landa) - 1;//
            double I1, I2, I3, denominatorOfF, F, qc, dqdI1, dqdI2, dqdI3, dI1dS1, dI1dS2, dI1dS3, dI2dS1, dI2dS2, dI2dS3;
            double dI3dS1, dI3dS2, dI3dS3, dfdp, dfdq, dqdS1, dqdS2, dqdS3, dpdS1, dpdS2, dpdS3, dfdS1, dfdS2, dfdS3;
            double dgdp, dgdq, dpdS11, dpdS22, dpdS33, dqdS11, dqdS22, dqdS33, dgdS11, dgdS22, dgdS33, A;
            for (int i = 0; i < number - 1; i++)
            {
                I1 = S1[i] + S2[i] + S3[i];//
                I2 = S1[i] * S2[i] + S2[i] * S3[i] + S3[i] * S1[i];
                I3 = S1[i] * S2[i] * S3[i];//
                denominatorOfF = I1 * I2 - 9 * I3;
                if (denominatorOfF < 1E-20) denominatorOfF = 1E-20;
                F = Math.Sqrt((I1 * I2 - I3) / denominatorOfF);
                qc = 2 * I1 / (3 * F - 1);//   q in SMP 
                q[i] = 2 * I1 / (3 * F - 1);//q in transformational stress space ,q=qc 
                p[i] = I1 / 3;//p in transformational stress space
                qz[i] = 1 / Math.Sqrt(2) * Math.Sqrt(Math.Pow((S1[i] - S2[i]), 2) + Math.Pow((S2[i] - S3[i]), 2) + Math.Pow((S1[i] - S3[i]), 2));//真实应力空间广义剪应力
                pz[i] = (S1[i] + S2[i] + S3[i]) / 3;//

                S11[i] = pz[i] + qc * (S1[i] - pz[i]) / qz[i];//S11变换应力空间第一主应力，老师的经典公式
                S22[i] = pz[i] + qc * (S2[i] - pz[i]) / qz[i];
                S33[i] = pz[i] + qc * (S3[i] - pz[i]) / qz[i];
                n[i] = q[i] / p[i];
                nz[i] = qz[i] / pz[i];
                kesi[i] = Zb - landa * Math.Log((p[i] + ps) / (1 + ps)) - (landa - kapa) * Math.Log(((1 + (1 + x) * n[i] * n[i] / (M * M - x * n[i] * n[i])) * p[i] + ps) / (p[i] + ps)) - eVoidRatio[i];//这些变量都是在变换应力空间中求

                Mf[i] = 6 * Math.Pow((Math.Sqrt(12 * (3 - M) / (M * M) * Math.Exp(-kesi[i] / (landa - kapa)) + 1) + 1), (-1));
                Mc[i] = M * Math.Exp(-m * kesi[i]);

                dqdI1 = 2 / (3 * F - 1) + 24 * I1 * I2 * I3 / (F * (3 * F - 1) * (3 * F - 1) * (denominatorOfF * denominatorOfF));//%变换应力q 为求dqdS1做准备，这样就落到了真实应力空间中
                dqdI2 = 24 * I1 * I1 * I3 / (F * (3 * F - 1) * (3 * F - 1) * (denominatorOfF * denominatorOfF));
                dqdI3 = -24 * I1 * I1 * I2 / (F * (3 * F - 1) * (3 * F - 1) * (denominatorOfF * denominatorOfF));

                dI1dS1 = 1.0; dI1dS2 = 1.0; dI1dS3 = 1.0;
                dI2dS1 = S2[i] + S3[i]; dI2dS2 = S1[i] + S3[i]; dI2dS3 = S1[i] + S2[i];
                dI3dS1 = S2[i] * S3[i]; dI3dS2 = S1[i] * S3[i]; dI3dS3 = S1[i] * S2[i];
                dfdp = (Math.Pow(M, 4) - (1 + 3 * x) * M * M * Math.Pow(n[i], 2) - x * Math.Pow(n[i], 4)) / (p[i] * (Math.Pow(M, 2) - x * Math.Pow(n[i], 2)) * (Math.Pow(M, 2) + Math.Pow(n[i], 2) + (Math.Pow(M, 2) - x * Math.Pow(n[i], 2)) * ps / p[i]));//%论文里推好的公式，这个计算特别慢
                dfdq = 2 * Math.Pow(M, 2) * (1 + x) * n[i] / (p[i] * (Math.Pow(M, 2) - x * Math.Pow(n[i], 2)) * (M * M + Math.Pow(n[i], 2) + (M * M - x * Math.Pow(n[i], 2)) * ps / p[i]));

                dqdS1 = dqdI1 * dI1dS1 + dqdI2 * dI2dS1 + dqdI3 * dI3dS1;//变换应力空间中变换应力q对对真实最大主应力求导 为dfdS1做准备
                dqdS2 = dqdI1 * dI1dS2 + dqdI2 * dI2dS2 + dqdI3 * dI3dS2;
                dqdS3 = dqdI1 * dI1dS3 + dqdI2 * dI2dS3 + dqdI3 * dI3dS3;

                dpdS1 = 1 / 3.0; dpdS2 = 1 / 3.0; dpdS3 = 1 / 3.0;

                dfdS1 = dfdp * dpdS1 + dfdq * dqdS1;
                dfdS2 = dfdp * dpdS2 + dfdq * dqdS2;
                dfdS3 = dfdp * dpdS3 + dfdq * dqdS3;

                dgdp = (Math.Pow(Mc[i], 2) - Math.Pow(n[i], 2)) / (p[i] * (Math.Pow(Mc[i], 2) + Math.Pow(n[i], 2)));//%g是变换应力空间中？？？
                dgdq = 2 * n[i] / (p[i] * (Math.Pow(Mc[i], 2) + Math.Pow(n[i], 2)));

                dpdS11 = 1 / 3.0; dpdS22 = 1 / 3.0; dpdS33 = 1 / 3.0;
                dqdS11 = (2 * S11[i] - S22[i] - S33[i]) / (2 * q[i]);//这个q是变换应力，简单的推导
                dqdS22 = (2 * S22[i] - S11[i] - S33[i]) / (2 * q[i]);
                dqdS33 = (2 * S33[i] - S11[i] - S22[i]) / (2 * q[i]);


                dgdS11 = dgdp * dpdS11 + dgdq * dqdS11;
                dgdS22 = dgdp * dpdS22 + dgdq * dqdS22;
                dgdS33 = dgdp * dpdS33 + dgdq * dqdS33;
                E = 3 * (1 - 2 * v) * (p[i] + ps) / ck;
                C11 = 1 / E;
                C12 = -v / E;
                C13 = -v / E;
                C21 = -v / E;
                C22 = 1 / E;
                C23 = -v / E;
                C31 = -v / E;
                C32 = -v / E;
                C33 = 1 / E;
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

                if (dfdS1 * dS1 + dfdS2 * dS2 + dfdS3 * dS3 > 0)
                {
                    A = cp * (Math.Pow(Mc[i], 4) - Math.Pow(n[i], 4)) / (Math.Pow(Mf[i], 4) - Math.Pow(n[i], 4));
                    C11 = 1 / E + A * dfdS1 * dgdS11 / dgdp;
                    C12 = -v / E + A * dfdS2 * dgdS11 / dgdp;
                    C13 = -v / E + A * dfdS3 * dgdS11 / dgdp;
                    C21 = -v / E + A * dfdS1 * dgdS22 / dgdp;
                    C22 = 1 / E + A * dfdS2 * dgdS22 / dgdp;
                    C23 = -v / E + A * dfdS3 * dgdS22 / dgdp;
                    C31 = -v / E + A * dfdS1 * dgdS33 / dgdp;
                    C32 = -v / E + A * dfdS2 * dgdS33 / dgdp;
                    C33 = 1 / E + A * dfdS3 * dgdS33 / dgdp;
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
                                dS3 = 0;
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
    }
}
