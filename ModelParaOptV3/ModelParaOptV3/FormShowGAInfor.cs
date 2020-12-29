using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
namespace ModelParaOptExcelAddIn
{
    public partial class FormShowGAInfor : Form
    {
        public FormShowGAInfor()
        {
            InitializeComponent();
        }
        public  Thread addTH;
        public  GA.Chromosome currentChrom;
        private void FormShowGAInfor_Load(object sender, EventArgs e)
        {
            addTH = new Thread(new ThreadStart(addData));
            addTH.Start();
        }

        #region 添加数据
        public  void addData()
        {
            while (true)
            {

                this.BeginInvoke(new Action(() =>
                {
                    if (GA.diedaiList.HistBestChromOfGaq.Count!=0)
                    {
                        currentChrom = GA.diedaiList.HistBestChromOfGaq.Dequeue();
                        int daishu = currentChrom.CurrentIteration;
                        double fit = currentChrom.Fit;
                        double[] paraList = currentChrom.chromosome;
                        richTextBox1.AppendText($"iteration: \t{daishu} \t,bestFit: {fit} \t,parameters: \t");

                        foreach (double para in paraList)
                        {
                            richTextBox1.AppendText($"{para} \t");
                        }
                        richTextBox1.AppendText($"\r\n");
                        richTextBox1.Update();
                        chart1.Series[0].Points.AddXY(daishu,fit);
                        chart1.Update();

                    }
                    //if (GA.diedaiList.HistBestChromOfGa.Count!=0)
                    //{
                    //    //int COUNT = GA.diedaiList.HistBestChromOfGa.Count - 1;
                    //    //GA.Chromosome current = GA.diedaiList.HistBestChromOfGa[COUNT];
                    //    //int daishu = current.CurrentIteration;
                    //    //double fit = current.Fit;
                    //    //double[] paraList = current.chromosome;
                    //    richTextBox1.AppendText($"iteration:{daishu},bestFit:{fit},parameters:");

                    //    foreach (double para in paraList)
                    //    {
                    //        Ribbon1.formShowGAInfor.richTextBox1.AppendText($"{para} \t");
                    //    }
                    //    Ribbon1.formShowGAInfor.richTextBox1.AppendText($"\r\n");
                    //    Ribbon1.formShowGAInfor.richTextBox1.Update();
                    //}

                }));

                Thread.Sleep(500);
            }
        }
        #endregion
        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // SAVE PICTURE
        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            DialogResult dr = saveFileDialog1.ShowDialog();
            saveFileDialog1.Filter = "Tag Image File Format (*.tif)|*.tiff|All files (*.*)|*.*";
            saveFileDialog1.DefaultExt = "tiff";
            string filename = saveFileDialog1.FileName + ".tiff";
            if (dr == DialogResult.OK && !string.IsNullOrEmpty(filename))
            {
                chart1.SaveImage(filename, System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Tiff);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            DialogResult dr = saveFileDialog1.ShowDialog();
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            saveFileDialog1.DefaultExt = "txt";
            string filename = saveFileDialog1.FileName + ".txt";
            if (dr == DialogResult.OK && !string.IsNullOrEmpty(filename))
            {
                richTextBox1.SaveFile(filename, RichTextBoxStreamType.PlainText);
            }
        }

        private void FormShowGAInfor_FormClosing(object sender, FormClosingEventArgs e)
        {
            addTH.Abort();
            ////https://blog.csdn.net/shizhibuyi1234/article/details/78202647
            //while (addTH.ThreadState != ThreadState.Aborted)
            //{
            //    Thread.Sleep(100);
            //}

        }
    }
}
