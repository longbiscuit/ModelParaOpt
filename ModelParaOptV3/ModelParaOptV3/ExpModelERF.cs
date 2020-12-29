using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelParaOptExcelAddIn
{
    public delegate double ModelComputeDelegateEventHandler(double[] para);
    /// <summary>
    /// Error function of experiment and model, using least square method.
    /// </summary>
    public class ExpModelERF
    {
        public static event ModelComputeDelegateEventHandler ModelComputeEvent;
        public static double ComputeAllError(double[] para)
        {
            double errorCost = 0.0;
            //compute constitive model error

                int currentIteration = GA.diedaiList.CurrentBestChrom.CurrentIteration;
                int maxIteration = GA.diedaiList.CurrentBestChrom.MaxIteration;// Ribbon1.maxIteration;
                if ((currentIteration  >0.3* maxIteration || GA.diedaiList.CurrentBestChrom.standardDeviationOfPop < 0.05) &&
                     GA.diedaiList.CurrentBestChrom.chromosome!=null && IsParaEqual(para, GA.diedaiList.CurrentBestChrom.chromosome))
                {
                    errorCost = GA.diedaiList.CurrentBestChrom.Fit;
                }
                else
                {
                    errorCost = (double)(ModelComputeEvent?.Invoke(para));
                }
            
            return errorCost;
        }



        public static double CSUHComputeAllErr(double[] para)
        {
            double errorCost;
            //double M = para[0]; double v = para[1]; double kapa = para[2]; double landa = para[3]; double N = para[4];
            //double Zc = para[5]; double Ze = Zc; double x = para[6]; double m = para[7];
            //if ( kappa>lambda     ||      N<Z        ||  m>(1-x)/((lambda-kappa)*(1+x))  ) is not allowed
            if (para[2] > para[3] || para[4] < para[5] || para[7] > (1 - para[6]) / ((para[3] - para[2]) * (1 + para[6])))
            {
                errorCost = 1e5;
            }
            else
            {
                CSUHComputeModelData(para);
                errorCost = LeastSquare();

                // uncommon lambda<2.0*kappa  || lambda>20.0*kappa
                if (para[3] < 2.0 * para[2] || para[3] > 20.0 * para[2])
                {
                    errorCost = 1.2 * errorCost;
                }
            }

            return errorCost;
        }

        public static bool IsParaEqual(double[] para, double[] currentBestChromPara)
        {
            int numPara = para.Length;
            bool isAllEqual = true;
            for (int i = 0; i < numPara; i++)
            {
                if (para[i] != currentBestChromPara[i])
                {
                    isAllEqual = false;
                    break;
                }
            }
            return isAllEqual;
        }
        public static double UHComputeAllErr(double[] para)
        {
            double errorCost;
            UHComputeModelData(para);
            errorCost = LeastSquare();
            // uncommon lambda<2.0*kappa  || lambda>20.0*kappa
            if (para[3] < 2.0 * para[2] || para[3] > 20.0 * para[2])
            {
                errorCost = 1.2 * errorCost;
            }
            return errorCost;
        }

        public static double MCCComputeAllErr(double[] para)
        {
            double errorCost;
            MCCComputeModelData(para);
            errorCost = LeastSquare();
            // uncommon lambda<2.0*kappa  || lambda>20.0*kappa
            if (para[3] < 2.0 * para[2] || para[3] > 20.0 * para[2])
            {
                errorCost = 1.2 * errorCost;
            }
            return errorCost;
        }

        public static double DuncanEBComputeAllErr(double[] para)
        {
            double errorCost;
            DuncanEBComputeModelData(para);
            errorCost = LeastSquare();
            return errorCost;
        }

        /// <summary>
        ///1. using initial conditions and constitive model compue the theory data
        /// </summary>
        /// <param name="para">parameters of CSUH model</param>
        public static void CSUHComputeModelData(double[] para)
        {

            ConstitiveModel.CSUH constitiveModel = new ConstitiveModel.CSUH();
            //1. compute all theory model data
            double tempD1, tempD2;
            double iso_p, iso_e, shear_p, shear_e;
            int tempI1GroupLength, tempI2, tempI3;
            tempI1GroupLength = InputData.ExpModelDataClassArr.Length;
            for (int i = 0; i < tempI1GroupLength; i++)
            {
                iso_p = InputData.ExpModelDataClassArr[i].ISOIniConfiningPressure;
                iso_e = InputData.ExpModelDataClassArr[i].ISOIniVoidRatio;
                shear_p = InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                shear_e = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio;
                //1.1 can only use the test data after row 5
                if ((iso_p == 0.0 && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == shear_e && shear_p > 0.0 && shear_e > 0))
                {
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;//VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }

                // 1.2 Normal consolidation but shear initial porosity is not known
                else if (iso_p > 0.0 && iso_p < shear_p && iso_e > 0 && shear_e == 0)//carefull
                {
                    // 1.2.1 first stage: use isotropic stress path compute the shear stage initial void ratio
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, EnumClass.TestType.IsoCom);
                    constitiveModel.ModelCompute(para, iso_p, iso_e, InputData.ExpModelDataClassArr[i].b);
                    tempI2 = constitiveModel.ModelPz.Length - 1;
                    for (int j = 0; j < tempI2; j++)
                    {
                        tempD1 = constitiveModel.ModelPz[j] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        tempD2 = constitiveModel.ModelPz[j + 1] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        if (tempD1 <= 0.0 && tempD2 >= 0.0)//Squeeze Theorem
                        {
                            if (Math.Abs(tempD1) < Math.Abs(tempD1))
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j];
                                shear_e = constitiveModel.ModelVoidRatio[j];
                            }
                            else
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j + 1];
                                shear_e = constitiveModel.ModelVoidRatio[j + 1];
                            }
                            break;//break the for loop
                        }
                    }

                    //if the model data output to excel ，compute the missing void ratio data 
                    if (InputData.IsAllPaticipateCompute == EnumClass.PaticipateInCompute.All)
                    {
                        //according to the isotropic stage  p0 and e0  and shear stage p0, compute the shear stage experiment void ratio
                        tempI3 = InputData.ExpModelDataClassArr[i].ExpEv.Length;
                        for (int m = 1; m < tempI3; m++)
                        {
                            //ei=e0-ev(1+e0)
                            InputData.ExpModelDataClassArr[i].ExpVoidRatio[m] = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio -
                                InputData.ExpModelDataClassArr[i].ExpEv[m] * (1 + InputData.ExpModelDataClassArr[i].ShearIniVoidRatio);
                        }
                    }

                    //1.2.2 second stage: Calculate according to the given shear stress path
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1) / 100.0, InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);//elas-plastic
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }
            }//for
        }


        /// <summary>
        ///2. using initial conditions and constitive model compue the theory data
        /// </summary>
        /// <param name="para">parameters of CSUH model</param>
        public static void UHComputeModelData(double[] para)
        {

            ConstitiveModel.UH constitiveModel = new ConstitiveModel.UH();
            //1. compute all theory model data
            double tempD1, tempD2;
            double iso_p, iso_e, shear_p, shear_e;
            int tempI1GroupLength, tempI2, tempI3;
            tempI1GroupLength = InputData.ExpModelDataClassArr.Length;
            for (int i = 0; i < tempI1GroupLength; i++)
            {
                iso_p = InputData.ExpModelDataClassArr[i].ISOIniConfiningPressure;
                iso_e = InputData.ExpModelDataClassArr[i].ISOIniVoidRatio;
                shear_p = InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                shear_e = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio;
                //1.1 can only use the test data after row 5
                if ((iso_p == 0.0 && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == shear_e && shear_p > 0.0 && shear_e > 0))
                {
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }



                // 1.2 Normal consolidation but shear initial porosity is not known
                else if (iso_p > 0.0 && iso_p < shear_p && iso_e > 0 && shear_e == 0)
                {
                    // 1.2.1 first stage: use isotropic stress path compute the shear stage initial void ratio
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, EnumClass.TestType.IsoCom);
                    constitiveModel.ModelCompute(para, iso_p, iso_e, InputData.ExpModelDataClassArr[i].b);
                    tempI2 = constitiveModel.ModelPz.Length - 1;
                    for (int j = 0; j < tempI2; j++)
                    {
                        tempD1 = constitiveModel.ModelPz[j] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        tempD2 = constitiveModel.ModelPz[j + 1] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        if (tempD1 <= 0.0 && tempD2 >= 0.0)//Squeeze Theorem
                        {
                            if (Math.Abs(tempD1) < Math.Abs(tempD1))
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j];
                                shear_e = constitiveModel.ModelVoidRatio[j];
                            }
                            else
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j + 1];
                                shear_e = constitiveModel.ModelVoidRatio[j + 1];
                            }
                            break;
                        }
                    }


                    //if (InputData.isOutputToExcel == 1)
                    if (InputData.IsAllPaticipateCompute == EnumClass.PaticipateInCompute.All)
                    {
                        //according to the isotropic stage  p0 and e0  and shear stage p0, compute the shear stage experiment void ratio
                        tempI3 = InputData.ExpModelDataClassArr[i].ExpEv.Length;
                        for (int m = 1; m < tempI3; m++)
                        {
                            //ei=e0-ev(1+e0)
                            InputData.ExpModelDataClassArr[i].ExpVoidRatio[m] = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio -
                                InputData.ExpModelDataClassArr[i].ExpEv[m] * (1 + InputData.ExpModelDataClassArr[i].ShearIniVoidRatio);
                        }
                    }

                    //1.2.2 second stage: Calculate according to the given shear stress path
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1) / 100.0, InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);//elas-plastic
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }
            }//for
        }


        /// <summary>
        /// using initial conditions and constitive model compue the theory data
        /// </summary>
        /// <param name="para">parameters of CSUH model</param>
        public static void MCCComputeModelData(double[] para)
        {

            ConstitiveModel.MCC constitiveModel = new ConstitiveModel.MCC();
            //1. compute all theory model data
            double tempD1, tempD2;
            double iso_p, iso_e, shear_p, shear_e;
            int tempI1GroupLength, tempI2, tempI3;
            tempI1GroupLength = InputData.ExpModelDataClassArr.Length;
            for (int i = 0; i < tempI1GroupLength; i++)
            {
                iso_p = InputData.ExpModelDataClassArr[i].ISOIniConfiningPressure;
                iso_e = InputData.ExpModelDataClassArr[i].ISOIniVoidRatio;
                shear_p = InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                shear_e = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio;
                //1.1 can only use the test data after row 5
                if ((iso_p == 0.0 && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == shear_e && shear_p > 0.0 && shear_e > 0))
                {
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b, 0.0);
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }
                // 1.2 Normal consolidation but shear initial porosity is not known
                else if (iso_p > 0.0 && iso_p < shear_p && iso_e > 0 && shear_e == 0)
                {
                    // 1.2.1 first stage: use isotropic stress path compute the shear stage initial void ratio
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, EnumClass.TestType.IsoCom);
                    constitiveModel.ModelCompute(para, iso_p, iso_e, InputData.ExpModelDataClassArr[i].b, 0.0);
                    tempI2 = constitiveModel.ModelPz.Length - 1;
                    for (int j = 0; j < tempI2; j++)
                    {
                        tempD1 = constitiveModel.ModelPz[j] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        tempD2 = constitiveModel.ModelPz[j + 1] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        if (tempD1 <= 0.0 && tempD2 >= 0.0)//Squeeze Theorem
                        {
                            if (Math.Abs(tempD1) < Math.Abs(tempD1))
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j];
                                shear_e = constitiveModel.ModelVoidRatio[j];
                            }
                            else
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j + 1];
                                shear_e = constitiveModel.ModelVoidRatio[j + 1];
                            }
                            break;
                        }
                    }


                    //if (InputData.isOutputToExcel == 1)
                    if (InputData.IsAllPaticipateCompute == EnumClass.PaticipateInCompute.All)
                    {
                        //according to the isotropic stage  p0 and e0  and shear stage p0, compute the shear stage experiment void ratio
                        tempI3 = InputData.ExpModelDataClassArr[i].ExpEv.Length;
                        for (int m = 1; m < tempI3; m++)
                        {
                            //ei=e0-ev(1+e0)
                            InputData.ExpModelDataClassArr[i].ExpVoidRatio[m] = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio -
                                InputData.ExpModelDataClassArr[i].ExpEv[m] * (1 + InputData.ExpModelDataClassArr[i].ShearIniVoidRatio);
                        }
                    }

                    //1.2.2 second stage: Calculate according to the given shear stress path

                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b, 0.0);//elas-plastic
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }
                // 1.3 overconsolidation
                else if (shear_p > 0.0 && iso_p > shear_p && iso_e >= 0 && shear_e > 0 && shear_e >= iso_e)
                {
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);

                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b, iso_p);
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }

            }//for
        }



        /// <summary>
        /// using initial conditions and constitive model compue the theory data
        /// </summary>
        /// <param name="para">parameters of CSUH model</param>
        public static void DuncanEBComputeModelData(double[] para)
        {

            ConstitiveModel.DuncanEB constitiveModel = new ConstitiveModel.DuncanEB();
            //1. compute all theory model data
            double tempD1, tempD2;
            double iso_p, iso_e, shear_p, shear_e;
            int tempI1GroupLength, tempI2, tempI3;
            tempI1GroupLength = InputData.ExpModelDataClassArr.Length;
            for (int i = 0; i < tempI1GroupLength; i++)
            {
                iso_p = InputData.ExpModelDataClassArr[i].ISOIniConfiningPressure;
                iso_e = InputData.ExpModelDataClassArr[i].ISOIniVoidRatio;
                shear_p = InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                shear_e = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio;
                //1.1 can only use the test data after row 5
                if ((iso_p == 0.0 && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == 0.0 && shear_p > 0.0 && shear_e > 0) ||
                    (iso_p == shear_p && iso_e == shear_e && shear_p > 0.0 && shear_e > 0))
                {
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }

                // 1.2 Normal consolidation but shear initial porosity is not known
                else if (iso_p > 0.0 && iso_p < shear_p && iso_e > 0 && shear_e == 0)
                {
                    // 1.2.1 first stage: use isotropic stress path compute the shear stage initial void ratio
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, EnumClass.TestType.IsoCom);
                    constitiveModel.ModelCompute(para, iso_p, iso_e, InputData.ExpModelDataClassArr[i].b);
                    tempI2 = constitiveModel.ModelPz.Length - 1;
                    for (int j = 0; j < tempI2; j++)
                    {
                        tempD1 = constitiveModel.ModelPz[j] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        tempD2 = constitiveModel.ModelPz[j + 1] - InputData.ExpModelDataClassArr[i].ShearIniConfiningPressure;
                        if (tempD1 <= 0.0 && tempD2 >= 0.0)//Squeeze Theorem
                        {
                            if (Math.Abs(tempD1) < Math.Abs(tempD1))
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j];
                                shear_e = constitiveModel.ModelVoidRatio[j];
                            }
                            else
                            {
                                InputData.ExpModelDataClassArr[i].ShearIniVoidRatio = constitiveModel.ModelVoidRatio[j + 1];
                                shear_e = constitiveModel.ModelVoidRatio[j + 1];
                            }
                            break;//break the for loop
                        }
                    }


                    //if (InputData.isOutputToExcel == 1)
                    if (InputData.IsAllPaticipateCompute == EnumClass.PaticipateInCompute.All)
                    {
                        //according to the isotropic stage  p0 and e0  and shear stage p0, compute the shear stage experiment void ratio
                        tempI3 = InputData.ExpModelDataClassArr[i].ExpEv.Length;
                        for (int m = 1; m < tempI3; m++)
                        {
                            //ei=e0-ev(1+e0)
                            InputData.ExpModelDataClassArr[i].ExpVoidRatio[m] = InputData.ExpModelDataClassArr[i].ShearIniVoidRatio -
                                InputData.ExpModelDataClassArr[i].ExpEv[m] * (1 + InputData.ExpModelDataClassArr[i].ShearIniVoidRatio);
                        }
                    }

                    //1.2.2 second stage: Calculate according to the given shear stress path
                    constitiveModel.ModelIni((InputData.ExpModelDataClassArr[i].MaxE1 + InputData.excessE1), InputData.ModelStep, InputData.ExpModelDataClassArr[i].CurrentTestType);
                    constitiveModel.ModelCompute(para, shear_p, shear_e, InputData.ExpModelDataClassArr[i].b);//elas-plastic
                    InputData.ExpModelDataClassArr[i].ModelE1 = constitiveModel.ModelE1;// VectorMultiValue(constitiveModel.ModelE1, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelQ = constitiveModel.ModelQz;
                    InputData.ExpModelDataClassArr[i].ModelP = constitiveModel.ModelPz;
                    InputData.ExpModelDataClassArr[i].ModelEv = constitiveModel.ModelEv;// VectorMultiValue(constitiveModel.ModelEv, 100.0);
                    InputData.ExpModelDataClassArr[i].ModelU = constitiveModel.ModelU;
                    InputData.ExpModelDataClassArr[i].ModelYita = constitiveModel.ModelYita;
                    InputData.ExpModelDataClassArr[i].ModelVoidRatio = constitiveModel.ModelVoidRatio;
                }
            }//for
        }




        // 2. least square method compute the errorCost
        public static double LeastSquare()
        {
            double errorCost;
            double errorQ = 0.0, errorP = 0.0, errorEv = 0.0, errorU = 0.0;
            int sameE1Index, totalRows = 0;
            int tempI1GroupLength = InputData.ExpModelDataClassArr.Length;
            for (int i = 0; i < tempI1GroupLength; i++)
            {
                if (InputData.ExpModelDataClassArr[i].IsParticipateCompute == 1 || InputData.IsAllPaticipateCompute == EnumClass.PaticipateInCompute.All)
                {
                    int rowsNum = InputData.ExpModelDataClassArr[i].ExpE1.Length;
                    switch (InputData.ExpModelDataClassArr[i].CurrentTestType)
                    {
                        case EnumClass.TestType.CU:
                            {
                                // Conventional triaxial undrained shear test
                                //常规三轴固结不排水剪切试验（先等向压缩固结排水到指定围压，再进行不排水剪切试验到破坏），常规三轴是指σ2=σ3
                                //q u
                                for (int m = 0; m < rowsNum; m++)
                                {
                                    sameE1Index = InputData.ExpModelDataClassArr[i].SameE1Index[m];
                                    if (InputData.ExpModelDataClassArr[i].MaxQ != 0.0)
                                    {
                                        errorQ = errorQ + Math.Pow((InputData.ExpModelDataClassArr[i].ExpQ[m] - InputData.ExpModelDataClassArr[i].ModelQ[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxQ, 2);
                                        totalRows++;
                                    }

                                    if (InputData.ExpModelDataClassArr[i].MaxU != 0.0)
                                    {
                                        errorU = errorU + Math.Pow((InputData.ExpModelDataClassArr[i].ExpU[m] - InputData.ExpModelDataClassArr[i].ModelU[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxU, 2);
                                        totalRows++;
                                    }

                                }
                            }
                            break;
                        case EnumClass.TestType.ConstP:
                            {
                                //Conventional triaxial consolidation drainage shear test with  Const mean principal pressure p
                                //常规三轴固结排水等p剪切试验
                                //q ev
                                for (int m = 0; m < rowsNum; m++)
                                {
                                    sameE1Index = InputData.ExpModelDataClassArr[i].SameE1Index[m];
                                    if (InputData.ExpModelDataClassArr[i].MaxQ != 0.0)
                                    {
                                        errorQ = errorQ + Math.Pow((InputData.ExpModelDataClassArr[i].ExpQ[m] - InputData.ExpModelDataClassArr[i].ModelQ[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxQ, 2);
                                        totalRows++;
                                    }

                                    if (InputData.ExpModelDataClassArr[i].MaxEv != 0.0)
                                    {
                                        errorEv = errorEv + Math.Pow((InputData.ExpModelDataClassArr[i].ExpEv[m] - InputData.ExpModelDataClassArr[i].ModelEv[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxEv, 2);
                                        totalRows++;
                                    }


                                }
                            }
                            break;
                        case EnumClass.TestType.CD:
                            {
                                //Conventional triaxial Consolidation Drainage shear test with  Const surrounding confine pressure σ3
                                //常规三轴固结排水围压不变的剪切试验
                                // q ev
                                for (int m = 0; m < rowsNum; m++)
                                {
                                    sameE1Index = InputData.ExpModelDataClassArr[i].SameE1Index[m];
                                    if (InputData.ExpModelDataClassArr[i].MaxQ != 0.0)
                                    {
                                        errorQ = errorQ + Math.Pow((InputData.ExpModelDataClassArr[i].ExpQ[m] - InputData.ExpModelDataClassArr[i].ModelQ[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxQ, 2);
                                        totalRows++;
                                    }

                                    if (InputData.ExpModelDataClassArr[i].MaxEv != 0.0)
                                    {
                                        errorEv = errorEv + Math.Pow((InputData.ExpModelDataClassArr[i].ExpEv[m] - InputData.ExpModelDataClassArr[i].ModelEv[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxEv, 2);
                                        totalRows++;
                                    }


                                }
                            }
                            break;
                        case EnumClass.TestType.K0:
                            {
                                //Lateral compression drainage test
                                //侧限压缩固结试验
                                //p ev
                                for (int m = 0; m < rowsNum; m++)
                                {
                                    sameE1Index = InputData.ExpModelDataClassArr[i].SameE1Index[m];
                                    if (InputData.ExpModelDataClassArr[i].MaxP != 0.0)
                                    {
                                        errorP = errorP + Math.Pow((InputData.ExpModelDataClassArr[i].ExpP[m] - InputData.ExpModelDataClassArr[i].ModelP[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxP, 2);
                                        totalRows++;
                                    }

                                    if (InputData.ExpModelDataClassArr[i].MaxEv != 0.0)
                                    {
                                        errorEv = errorEv + Math.Pow((InputData.ExpModelDataClassArr[i].ExpEv[m] - InputData.ExpModelDataClassArr[i].ModelEv[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxEv, 2);
                                        totalRows++;
                                    }


                                }

                            }
                            break;
                        case EnumClass.TestType.IsoCom:
                            {
                                //Isotropic compression drainage test, principal stress coefficient b must be 0.0;
                                //等向压缩固结试验
                                // p ev
                                for (int m = 0; m < rowsNum; m++)
                                {
                                    sameE1Index = InputData.ExpModelDataClassArr[i].SameE1Index[m];
                                    if (InputData.ExpModelDataClassArr[i].MaxP != 0.0)
                                    {
                                        errorP = errorP + Math.Pow((InputData.ExpModelDataClassArr[i].ExpP[m] - InputData.ExpModelDataClassArr[i].ModelP[sameE1Index]) /
                                            InputData.ExpModelDataClassArr[i].MaxP, 2);
                                        totalRows++;
                                    }

                                    //if (InputData.ExpModelDataClassArr[i].MaxEv != 0.0)
                                    //{
                                    //    errorEv = errorEv + Math.Pow((InputData.ExpModelDataClassArr[i].ExpEv[m] - InputData.ExpModelDataClassArr[i].ModelEv[sameE1Index]) /
                                    //        InputData.ExpModelDataClassArr[i].MaxEv, 2);
                                    //    totalRows++;
                                    //}
                                }
                            }
                            break;
                        default:
                            System.Windows.Forms.MessageBox.Show("The type of experiment was not given !");
                            break;
                    }

                }
            }

            errorCost = (errorQ + errorP + errorEv + errorU) / totalRows;
            return errorCost;
        }


        public static double[] VectorMultiValue(double[] vector, double value)
        {
            int vectorLen = vector.Length;
            double[] newVector = new double[vectorLen];
            for (int i = 0; i < vectorLen; i++)
            {
                newVector[i] = vector[i] * value;
            }
            return newVector;
        }








    }
}
