using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Tools.Excel;

namespace ModelParaOptExcelAddIn
{
    public class InputData
    {
        public Excel.Application ExcelApp;
        public Excel.Workbook wbk;
        Excel.Worksheet wst;
        Excel.Range range;

        public static int GroupColumn = 7;//each group test data have 7 column:ε1(%)、q(kPa)、p(kPa)、εv(%)、u(kPa)、η=q/p、e(Void Ratio)
        public static int SheetHeaderRow = 1;//the rows Note occupied
        public static int SheetDataHeaderRow = 4;//experiment data's header row number
        public static int ModelStep = 3000;//The initial stiffness method is used to calculate the iterative steps in the evolution process of the constitutive model
        public static double excessE1 = 0.01;//the theoretical e1 value is greater than the experimental value e1, %
        public static int GroupNum;// The number of groups of test data, a group data means the same p0(initial confining pressure) and e0(initial Void ratio)

        public static EnumClass.ConstitutiveModelType CurrentUsingModel;
        public static EnumClass.PaticipateInCompute IsAllPaticipateCompute;
        public static ExpModelData[] ExpModelDataClassArr;//all test experiment and theory model data array


        public static List<int> SheetsHaveDataColumnList = new List<int>();//Maximum number of columns of test data in different sheets
        public static List<int> SheetsGroupNumList = new List<int>();//group number of different sheets 
        public static List<List<int>> SheetsDataRowList = new List<List<int>>();

        public static int isOutputToExcel = 0;//0:not output, 1:output to excel


        #region write constitive model data to excel(if necessary, need to write test experiment void ratio data to excel)
        public void WriteModelDataToExcel(List<string> wstNameList)
        {
            ExcelApp = Globals.ThisAddIn.Application;//Declare an Excel object
            wbk = ExcelApp.ActiveWorkbook;
            int serialExpSheet = 0;//serial number of Experiment contains in sheet
            int serialDataGroup = 0;//serial number of Experiment Data group
            //Write model data to excel
            for (int i = 2; i < wstNameList.Count; i = i + 2)
            {
                string wstName = wstNameList[i];//Iterate over the names of the worksheets stored in the List
                wst = wbk.Worksheets[wstName];//Gets the worksheet object in the workbook based on the worksheet name

                for (int j = 0; j < SheetsGroupNumList[serialExpSheet]; j++)
                {
                    // use Cells to set cell value
                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 1] as Excel.Range;
                    range.Value2 = ExpModelDataClassArr[serialDataGroup].SerialNumber;

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 2] as Excel.Range;
                    range.Value2 = ExpModelDataClassArr[serialDataGroup].IsParticipateCompute;

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 3] as Excel.Range;
                    range.Value2 = ExpModelDataClassArr[serialDataGroup].ISOIniConfiningPressure;

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 4] as Excel.Range;
                    range.Value2 = ExpModelDataClassArr[serialDataGroup].ISOIniVoidRatio;

                    range = wst.Range[wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 1],
                               wst.Cells[10000, j * (GroupColumn + 1) + 7]];
                    range.Clear();

                    range = wst.Range[wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 1],
                                      wst.Cells[ModelStep + SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 7]];

                    ExpModelDataClassArr[serialDataGroup].ModelData = CombineANDTransVector(ExpModelDataClassArr[serialDataGroup].ModelE1,
                        ExpModelDataClassArr[serialDataGroup].ModelQ, ExpModelDataClassArr[serialDataGroup].ModelP, ExpModelDataClassArr[serialDataGroup].ModelEv,
                        ExpModelDataClassArr[serialDataGroup].ModelU, ExpModelDataClassArr[serialDataGroup].ModelYita, ExpModelDataClassArr[serialDataGroup].ModelVoidRatio);
                    range.Value2 = ExpModelDataClassArr[serialDataGroup].ModelData;
                    serialDataGroup++;
                }
                serialExpSheet++;
            }


            // write test experiment void ratio data
            serialExpSheet = 0;//serial number of Experiment contains in sheet
            serialDataGroup = 0;//serial number of Experiment Data group
            for (int i = 1; i < wstNameList.Count; i = i + 2)
            {
                string wstName = wstNameList[i];//Iterate over the names of the worksheets stored in the List
                wst = wbk.Worksheets[wstName];//Gets the worksheet object in the workbook based on the worksheet name
                //wst.Activate();
                for (int j = 0; j < SheetsGroupNumList[serialExpSheet]; j++)
                {
                    range = wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 7] as Excel.Range;
                    //double VoidRatioCellR2 = range.Value2;
                    //if (range.Value2 == 0 || null== range.Value2)//is null
                    if (Convert.ToString(range.Text) == "")//cell is null
                    {
                        range = wst.Range[wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 7],
                      wst.Cells[SheetsDataRowList[serialExpSheet][j] + SheetDataHeaderRow, j * (GroupColumn + 1) + 7]];
                        range.Value2 = DoubleVectorTranspose1to2(InputData.ExpModelDataClassArr[serialDataGroup].ExpVoidRatio);
                        range.Interior.Color = System.Drawing.Color.FromArgb(255, 242, 204);
                    }
                    serialDataGroup++;
                }
                serialExpSheet++;
            }
        }

        public double[,] CombineANDTransVector(double[] E1, double[] Q, double[] P, double[] Ev, double[] U, double[] Yita, double[] VoidRatio)
        {
            int row = E1.Length;
            int column = GroupColumn;
            double[,] arr = new double[row - 1, column];
            for (int i = 0; i < row - 1; i++)
            {
                arr[i, 0] = E1[i]*100.0;
                arr[i, 1] = Q[i];
                arr[i, 2] = P[i];
                arr[i, 3] = Ev[i]*100.0;
                arr[i, 4] = U[i];
                arr[i, 5] = Yita[i];
                arr[i, 6] = VoidRatio[i];
            }
            return arr;
        }



        public double[,] DoubleVectorTranspose1to2(double[] Arr)
        {
            double[,] newArr = new double[Arr.GetLength(0), 1];
            for (int i = 0; i < Arr.GetLength(0); i++)
            {
                newArr[i, 0] = Arr[i];
            }
            return newArr;
        }
        #endregion





        #region read excel data to ExpModelDataClassArr array
        public void ReadTestDataToArr(List<string> wstNameList)
        {
            ExcelApp = Globals.ThisAddIn.Application;//Declare an Excel object
            wbk = ExcelApp.ActiveWorkbook;
            int numCol, numGroup, numRow, tempInt;
            SheetsHaveDataColumnList.Clear();
            SheetsGroupNumList.Clear();
            SheetsDataRowList.Clear();

            for (int i = 1; i < wstNameList.Count; i += 2)
            {
                string wstName = wstNameList[i];//Iterate over the names of the worksheets stored in the List
                wst = wbk.Worksheets[wstName];//Gets the worksheet object in the workbook based on the worksheet name
                numCol = wst.Range["XFD5"].End[Excel.XlDirection.xlToLeft].Column;//Find the number of columns in the right-most non-empty cell in row 5 of this worksheet
                if (numCol == 1) numCol = 0;
                SheetsHaveDataColumnList.Add(numCol);//Stores the maximum number of columns in each worksheet in the List
                numGroup = (int)Math.Ceiling((double)numCol / (GroupColumn + 1));//A group of data has 8 columns. Find out how many groups of data there are in this worksheet
                SheetsGroupNumList.Add(numGroup);//Store the number of groups of data in each worksheet into a List

                //find the row of test data in this group
                List<int> tempList = new List<int>();

                for (int j = 0; j < numGroup; j++)
                {
                    //numRow=GetRowsForColumn(wstName, j * (groupColumn + 1) + 1);
                    range = wst.Cells[60000, j * (GroupColumn + 1) + 1];  //1048576 
                    tempInt = Convert.ToInt32(range.End[Excel.XlDirection.xlUp].Row);
                    numRow = tempInt - SheetDataHeaderRow;//
                    if (numRow > 1)
                    {
                        tempList.Add(numRow);
                    }
                    GroupNum++;
                }
                SheetsDataRowList.Add(tempList);//
            }

            //read Experient data in AllTestExpDatas
            ExpModelDataClassArr = new ExpModelData[SheetsGroupNumList.Sum()];
            int serialExpSheet = 0;//serial number of Experiment contains in sheet
            int serialDataGroup = 0;//serial number of Experiment Data group
            double maxe1;
            for (int i = 1; i < wstNameList.Count; i = i + 2)
            {

                string wstName = wstNameList[i];//Iterate over the names of the worksheets stored in the List
                wst = wbk.Worksheets[wstName];//Gets the worksheet object in the workbook based on the worksheet name
                //wst.Activate();
                for (int j = 0; j < SheetsGroupNumList[serialExpSheet]; j++)
                {
                    ExpModelDataClassArr[serialDataGroup] = new ExpModelData();//
                    ExpModelDataClassArr[serialDataGroup].WstName = wstName;
                    ExpModelDataClassArr[serialDataGroup].TestType = Convert.ToInt32(wstName.Substring(0, 1));//int
                    ExpModelDataClassArr[serialDataGroup].CurrentTestType = (EnumClass.TestType)(Convert.ToInt32(wstName.Substring(0, 1)) - 1);//enum
                    ////https://blog.csdn.net/weixin_30417487/article/details/98015569?ops_request_misc=&request_id=&biz_id=102&utm_term=vsto%2520cells&utm_medium=distribute.pc_search_result.none-task-blog-2~all~sobaiduweb~default-9-98015569.first_rank_v2_pc_rank_v29
                    // use Cells to get cell value
                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 1] as Excel.Range;
                    if (Convert.ToString(range.Text)!="")//http://www.51testing.com/html/18/569418-831365.html
                    {
                        ExpModelDataClassArr[serialDataGroup].SerialNumber = Convert.ToInt32(range.Value2);
                    }
                    else
                    {
                        ExpModelDataClassArr[serialDataGroup].SerialNumber = serialDataGroup;
                    }

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 2] as Excel.Range;
                    if (Convert.ToString(range.Text) != "")
                    {
                        ExpModelDataClassArr[serialDataGroup].IsParticipateCompute = Convert.ToInt32(range.Value2);
                    }
                    else
                    {
                        ExpModelDataClassArr[serialDataGroup].IsParticipateCompute = 0;//Default does not participate in the calculation
                    }

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 3] as Excel.Range;
                    if (Convert.ToString(range.Text) != "") 
                    {
                        ExpModelDataClassArr[serialDataGroup].ISOIniConfiningPressure = Convert.ToDouble(range.Value2);
                    }
                    else
                    {
                        ExpModelDataClassArr[serialDataGroup].ISOIniConfiningPressure = 0.0;//The null value defaults to 0
                    }
                        

                    range = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 4] as Excel.Range;
                    if (Convert.ToString(range.Text) != "")
                    {
                        ExpModelDataClassArr[serialDataGroup].ISOIniVoidRatio = Convert.ToDouble(range.Value2);
                    }
                    else
                    {
                        ExpModelDataClassArr[serialDataGroup].ISOIniVoidRatio = 0.0;
                    }
                        
                    //range = wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 3] as Excel.Range;
                    //ExpModelDataClassArr[serialDataGroup].ShearIniConfiningPressure = Convert.ToDouble(range.Value2);
                    //range = wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 7] as Excel.Range;
                    //ExpModelDataClassArr[serialDataGroup].ShearIniVoidRatio = Convert.ToDouble(range.Value2);

                    range = wst.Range[wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 1],
                                      wst.Cells[SheetsDataRowList[serialExpSheet][j] + SheetDataHeaderRow, j * (GroupColumn + 1) + 7]];
                    //range.Select();

                    double[,] tempArr2 = TranslateRangeToArray(range.Value2);//cannot assign values to AllTestExpDatas[serialDataGroup].ExpData directly
                    tempArr2 = ExpModelData.ReadinDatad100(tempArr2);//e1%-->e1,ev%-->ev
                    ExpModelDataClassArr[serialDataGroup].ExpData = tempArr2;

                    //find the max column element of a group of test experiment data
                    //maxe1 = tempArr2[SheetsDataRowList[serialExpSheet][j] - 1, 0];//
                    maxe1 = ExpModelData.FindColumnMax(tempArr2,0);
                    ExpModelDataClassArr[serialDataGroup].MaxE1 = maxe1;
                    ExpModelDataClassArr[serialDataGroup].MaxQ = ExpModelData.FindColumnMax(tempArr2, 1);
                    ExpModelDataClassArr[serialDataGroup].MaxP = ExpModelData.FindColumnMax(tempArr2, 2);
                    ExpModelDataClassArr[serialDataGroup].MaxEv = ExpModelData.FindColumnMax(tempArr2, 3);
                    ExpModelDataClassArr[serialDataGroup].MaxU = ExpModelData.FindColumnMax(tempArr2, 4);
                    ExpModelDataClassArr[serialDataGroup].MaxYita = ExpModelData.FindColumnMax(tempArr2, 5);
                    ExpModelDataClassArr[serialDataGroup].MaxVoidRatio = ExpModelData.FindColumnMax(tempArr2, 6);

                    ExpModelDataClassArr[serialDataGroup].SameE1Index = ExpModelData.findSameE1Index(ModelStep, maxe1, excessE1, tempArr2);
                    ExpModelDataClassArr[serialDataGroup].ExpE1 = ExpModelData.ParseColumn(tempArr2, 0);
                    ExpModelDataClassArr[serialDataGroup].ExpQ = ExpModelData.ParseColumn(tempArr2, 1);
                    ExpModelDataClassArr[serialDataGroup].ExpP = ExpModelData.ParseColumn(tempArr2, 2);
                    ExpModelDataClassArr[serialDataGroup].ExpEv = ExpModelData.ParseColumn(tempArr2, 3);
                    ExpModelDataClassArr[serialDataGroup].ExpU = ExpModelData.ParseColumn(tempArr2, 4);
                    ExpModelDataClassArr[serialDataGroup].ExpYita = ExpModelData.ParseColumn(tempArr2, 5);
                    ExpModelDataClassArr[serialDataGroup].ExpVoidRatio = ExpModelData.ParseColumn(tempArr2, 6);

                    ExpModelDataClassArr[serialDataGroup].ShearIniConfiningPressure = ExpModelDataClassArr[serialDataGroup].ExpP[0];
                    ExpModelDataClassArr[serialDataGroup].ShearIniVoidRatio = ExpModelDataClassArr[serialDataGroup].ExpVoidRatio[0];



                    if (ExpModelDataClassArr[serialDataGroup].ISOIniConfiningPressure == 0 &&
                        ExpModelDataClassArr[serialDataGroup].ISOIniVoidRatio == 0 &&
                        ExpModelDataClassArr[serialDataGroup].ShearIniVoidRatio == 0)
                    {
                        Excel.Range range1 = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 3] as Excel.Range;
                        Excel.Range range2 = wst.Cells[SheetDataHeaderRow - 1, j * (GroupColumn + 1) + 4] as Excel.Range;
                        Excel.Range range3 = wst.Cells[SheetDataHeaderRow + 1, j * (GroupColumn + 1) + 7] as Excel.Range;
                        MessageBox.Show($"In {wstName} sheet {ExpModelDataClassArr[serialDataGroup].SerialNumber} group test data," +
                              $" the values of {range1.Address},{range2.Address},{range3.Address} can't equals 0 at the same time!) ");
                        break;
                    }


                    serialDataGroup++;
                }
                serialExpSheet++;
            }






        }



        /// <summary>
        /// Translate the excel range(two dimension) to array(two dimension)
        /// </summary>
        /// <param name="Arr">excel range</param>
        /// <returns>two dimension array</returns>
        public double[,] TranslateRangeToArray(object[,] Arr)
        {
            double[,] newArr = new double[Arr.GetLength(0), Arr.GetLength(1)];
            for (int i = 0; i < Arr.GetLength(0); i++)
            {
                for (int j = 0; j < Arr.GetLength(1); j++)
                {
                    //Notice that object index starts at 1
                    newArr[i, j] = Convert.ToDouble(Arr[i + 1, j + 1]);
                }
            }
            return newArr;
        }

        #endregion


        ////https://forums.asp.net/t/2159370.aspx?Get+Office+Interop+Excel+C+Specific+Column+last+cell+to+add+next+row+value
        //private int GetRowsForColumn(string sheetName, int columnNumber)
        //{
        //    int columnCount = 0;
        //    try
        //    {
        //        Excel.XlDirection goUp = Excel.XlDirection.xlUp;
        //        wst=wbk.Worksheets.get_Item(sheetName); ;
        //        //xlWorkSheets = wbk.Worksheets;
        //        //xlWorkSheet = xlWorkSheets.get_Item("Scale_Data");

        //        //columnCount = xlWorkSheet.Cells[xlWorkSheet.Rows.Count, columnNumber].End(goUp).Row;
        //        columnCount = wst.Cells[wst.Rows.Count, columnNumber].End(goUp).Row;
        //    }
        //    catch (Exception ex) { MessageBox.Show(ex.Message); }

        //    return columnCount;
        //}





    }
}
//public Excel.Application ExcelApp = Globals.ThisAddIn.Application;//Declare an Excel object
//public Excel.Workbook wbk = ExcelApp.ActiveWorkbook;
//Excel.Worksheet wst = wbk.Worksheets[wstName];