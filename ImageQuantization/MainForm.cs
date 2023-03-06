using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            sw.Start();
            string txt;
            txt = textBox1.Text;
            int k;
            k = Convert.ToInt32(txt);
            List<List<RGBPixel>> l = new List<List<RGBPixel>>();
            List<graph> mst = new List<graph>();
            List<RGBPixel> distinct_colors;
            distinct_colors = ImageOperations.find_diff_colors(ImageMatrix);
            textBox2.Text = distinct_colors.Count.ToString();
            mst = ImageOperations.MST(ImageMatrix, distinct_colors);
            double sum;
            sum=ImageOperations.MST_SUM(ImageOperations.Mst_sum);
            textBox3.Text = sum.ToString();
            ImageOperations.Mst_sum = 0;
            l = ImageOperations.Clustering(ImageMatrix, k, distinct_colors,mst);
            int cnt = l.Count;
            RGBPixel[,,] Colors_Pallete = new RGBPixel[256,256,256];
            Colors_Pallete= ImageOperations.Colors_Pallete(l);
            ImageOperations.Quantization(ref ImageMatrix, Colors_Pallete);
            sw.Stop();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            TimeSpan t = sw.Elapsed;
            textBox4.Text = t.ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e) 
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}