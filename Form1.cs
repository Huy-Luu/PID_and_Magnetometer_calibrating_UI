using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Numerics;

//graph
using ZedGraph;

//Serial
using System.IO;
using System.IO.Ports;
using System.Xml;

namespace PID_Tuning
{
    public partial class Form1 : Form
    {
        string SDatas = String.Empty; 
        string SRealTime = String.Empty; 
        int status = 0; 
        double realtime = 0; 
        double datas = 0;
		bool log_data = false;

        public Form1()
        {
            InitializeComponent();
            GraphPane graph = zedGraphControl1.GraphPane;
            

            string[] ports = SerialPort.GetPortNames();
            boxPort.Items.AddRange(ports);

            btnClose.Enabled = true;

            graph.Title.Text = "PID Tuning";
            graph.XAxis.Title.Text = "Time (s)";
            graph.YAxis.Title.Text = "Speed (km/h)";

            RollingPointPairList list = new RollingPointPairList(600000);
            LineItem curve = graph.AddCurve("Data", list, Color.Red, SymbolType.None);

            graph.XAxis.Scale.Min = 0;
            graph.XAxis.Scale.Max = 30;
            graph.XAxis.Scale.MinorStep = 1;
            graph.XAxis.Scale.MajorStep = 5;
            graph.YAxis.Scale.Min = -100;
            graph.YAxis.Scale.Max = 100;

            graph.AxisChange();

            GraphPane graph2 = zedGraphControl2.GraphPane;

            graph2.Title.Text = "Magnetomeer data calibration";
            graph2.XAxis.Title.Text = "x";
            graph2.YAxis.Title.Text = "y";

            RollingPointPairList coordinates_list_org = new RollingPointPairList(600000);
            RollingPointPairList coordinates_list_cal = new RollingPointPairList(600000);
            //LineItem coordinates_org = graph.AddCurve("Data Original", coordinates_list_org, Color.Red, SymbolType.Circle);
            //LineItem coordinates_cal = graph.AddCurve("Data", coordinates_list_org, Color.Red, SymbolType.Circle);

            graph2.XAxis.Scale.Min = -2000;
            graph2.XAxis.Scale.Max = 2000;
            graph2.XAxis.Scale.MinorStep = 100;
            graph2.XAxis.Scale.MajorStep = 100;
            graph2.YAxis.Scale.MinorStep = 100;
            graph2.YAxis.Scale.MajorStep = 100;
            graph2.YAxis.Scale.Min = -2000;
            graph2.YAxis.Scale.Max = 2000;

            zedGraphControl2.GraphPane.YAxis.Cross = 0.0;


            graph2.AxisChange();

            // drawSineCurve();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            btnClose.Enabled = true;

			serialPort.BaudRate = Convert.ToInt32(boxBaud.Text);

            try
            {
                serialPort.PortName = boxPort.Text;
                serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = true;
            btnClose.Enabled = false;
            try
            {
                serialPort.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Write(txtKp.Text + "," + txtKi.Text + "," + txtKd.Text + "," + txtSpeed.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //string indata = serialPort.ReadLine();
                //Console.WriteLine(indata);
                string[] arrList = serialPort.ReadLine().Split(','); 
                SRealTime = arrList[1]; 
                SDatas = arrList[0];
                //Console.WriteLine(SRealTime);
                //Console.WriteLine(SDatas);

                double.TryParse(SDatas, out datas); 
                double.TryParse(SRealTime, out realtime);
                realtime = realtime/1000; // Đối ms sang s
                //txtDebug.Text = datas + "and" + realtime + '\r';
                //Draw();
            }
            catch
            {
                return;
            }
        }

		private void btnClear_Click(object sender, EventArgs e)
		{
			zedGraphControl1.GraphPane.CurveList.Clear();
			zedGraphControl1.GraphPane.GraphObjList.Clear(); 

			zedGraphControl1.AxisChange();
			zedGraphControl1.Invalidate();

			GraphPane graph = zedGraphControl1.GraphPane;
			graph.Title.Text = "PID Tuning";
			graph.XAxis.Title.Text = "Time (s)";
			graph.YAxis.Title.Text = "Speed (m/s)";

			RollingPointPairList list = new RollingPointPairList(60000);
			LineItem curve = graph.AddCurve("Data", list, Color.Red, SymbolType.None);

			graph.XAxis.Scale.Min = 0;
			graph.XAxis.Scale.Max = 30;
			graph.XAxis.Scale.MinorStep = 1;
			graph.XAxis.Scale.MajorStep = 5;
			graph.YAxis.Scale.Min = -100;
			graph.YAxis.Scale.Max = 100;

			graph.AxisChange();
		}

		// Vẽ đồ thị
		private void Draw()
        {

            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;

            if (curve == null)
                return;

            IPointListEdit list = curve.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(realtime, datas); // Thêm điểm trên đồ thị

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (realtime > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = realtime + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (datas > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = datas + yScale.MajorStep;
            }
            else if (datas < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = datas - yScale.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                Draw();
            }
        }

		//Magnetometer
		private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if(log_data)
            {
				AppendToFile(serialPort2.ReadLine());
			}

		}

		private void AppendToFile(string toAppend)
		{
			string FilePath = @"E:\Mag.txt";
			File.AppendAllText(FilePath, toAppend);
		}

		private void btnStartLog_Click(object sender, EventArgs e)
		{
			string FilePath = @"E:\Mag.txt";
			if (serialPort2.IsOpen)
			{
				btnStartLog.Enabled = false;
				btnStopLog.Enabled = true;
			}
			File.WriteAllText(FilePath, String.Empty);
			log_data = true;
		}

		private void btnStopLog_Click(object sender, EventArgs e)
		{
			if (serialPort2.IsOpen)
			{
				btnStartLog.Enabled = true;
				btnStopLog.Enabled = false;
			}
			log_data = false;
			string FilePath = @"E:\Mag.txt";

		}

		private void btnConnect2_Click(object sender, EventArgs e)
		{
			btnConnect2.Enabled = false;
			btnClose2.Enabled = true;

			serialPort2.BaudRate = Convert.ToInt32(boxBaud2.Text);

			try
			{
				serialPort2.PortName = boxPorts2.Text;
				serialPort2.Open();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnClose2_Click(object sender, EventArgs e)
		{
			btnConnect2.Enabled = true;
			btnClose2.Enabled = false;
			try
			{
				serialPort2.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        private void resetGraph2()
        {
            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();

            GraphPane graph2 = zedGraphControl2.GraphPane;

            graph2.Title.Text = "Magnetomeer data calibration";
            graph2.XAxis.Title.Text = "x";
            graph2.YAxis.Title.Text = "y";

            RollingPointPairList coordinates_list_org = new RollingPointPairList(600000);
            RollingPointPairList coordinates_list_cal = new RollingPointPairList(600000);
            //LineItem coordinates_org = graph.AddCurve("Data Original", coordinates_list_org, Color.Red, SymbolType.Circle);
            //LineItem coordinates_cal = graph.AddCurve("Data", coordinates_list_org, Color.Red, SymbolType.Circle);

            graph2.XAxis.Scale.Min = -2000;
            graph2.XAxis.Scale.Max = 2000;
            graph2.XAxis.Scale.MinorStep = 100;
            graph2.XAxis.Scale.MajorStep = 100;
            graph2.YAxis.Scale.MinorStep = 100;
            graph2.YAxis.Scale.MajorStep = 100;
            graph2.YAxis.Scale.Min = -2000;
            graph2.YAxis.Scale.Max = 2000;

            zedGraphControl2.GraphPane.YAxis.Cross = 0.0;


            graph2.AxisChange();
        }


		private void btnCalibrate_Click(object sender, EventArgs e)
        {
			string filename = @"E:\Mag.txt";
			int i, n;
			double A, B, C, D, E, F, x0, y0, x01, y01, a, b, r = 0, scale;
            double[] theta = new double[360];
            

			// load measurement data
			Matrix<double> data = DelimitedReader.Read<double>(filename, false, " ", false);

			n = data.RowCount;

			// create "X1", "X2" with the size of (n,3)
			Matrix<double> X1 = Matrix<double>.Build.Dense(n,3);
            Matrix<double> X2 = Matrix<double>.Build.Dense(n, 3);

            // create "S" and "Eig" with the size of (6,6)
            Matrix<double> S1 = Matrix<double>.Build.Dense(3, 3);
            Matrix<double> S2 = Matrix<double>.Build.Dense(3, 3);
            Matrix<double> S3 = Matrix<double>.Build.Dense(3, 3);
            Matrix<double> Eig = Matrix<double>.Build.Dense(3, 3);
            Matrix<double> T = Matrix<double>.Build.Dense(3, 3);

            // create "a1" and "a2" with the size of (3,1)
            Matrix<double> a1 = Matrix<double>.Build.Dense(3, 1);
            Matrix<double> a2 = Matrix<double>.Build.Dense(3, 1);

            // create Constraint matrix with the size of (6, 6)
            Matrix<double> Cst1 = Matrix<double>.Build.Dense(3, 3, 0.0);
            Cst1[0, 2] = 2;
            Cst1[1, 1] = -1;
            Cst1[2, 0] = 2;

            /*
            for (i = 0; i < n; i++)
            {
                data[i, 0] = data[i, 0] / 3000 * 100000;
                data[i, 1] = data[i, 1] / 3000 * 100000;
                data[i, 2] = data[i, 2] / 3000 * 100000;
            }*/

            for (i = 0; i < n; i++)
            {
                X1[i, 0] = data[i, 0] * data[i, 0];  // x^2
                X1[i, 1] = data[i, 0] * data[i, 1];  // x*y
                X1[i, 2] = data[i, 1] * data[i, 1];  // y^2
            }

            for (i = 0; i < n; i++)
            {
                X2[i, 0] = data[i, 0];               // x
                X2[i, 1] = data[i, 1];				//y
                X2[i, 2] = 1;                        //1
            }

            S1 = X1.Transpose() * X1;
            S2 = X1.Transpose() * X2;
            S3 = X2.Transpose() * X2;


            

            T = -1 * S3.Inverse() * S2.Transpose();



            Eig = S1 + S2 * T;
            Eig = Cst1.Inverse() * Eig;
            Console.WriteLine(Eig.ToString());

            Evd<double> eigen = Eig.Evd();
            Vector<Complex> wr = eigen.EigenValues;
            Matrix<Double> vr = eigen.EigenVectors;

            int index = 0;
            double maxval = wr[0].Real;
            for (i = 1; i < 3; i++)
            {
                if (wr[i].Real > maxval)
                {
                    maxval = wr[i].Real;
                    index = i;
                }
            }

            Vector<Double> vector = vr.Column(index);


            for (i = 0; i < vector.Count(); i++)
            {
                a1[i, 0] = vector[i];
            }


            /*

            for (i=0; i< 3; i++)
            {
                Vector<Double> vector = vr.Column(i);
                Double condition = 4 * vector[0] * vector[2] - vector[1] * vector[1];
                if(condition > 0)
                {
                    if(vector[0]<0)
                    {
                        vector[0] = -1 * vector[0];
                        vector[1] = -1 * vector[1];
                        vector[2] = -1 * vector[2];
                    }
                    int j = 0;
                    for(j=0; j<vector.Count(); j++)
                    {
                        a1[j, 0] = vector[j];
                    }
                }

            }
            */

            a2 = T * a1;
            Console.WriteLine("{0}, {1}", a1, a2);

            A = a1[0,0];
            B = a1[1,0];
            C = a1[2,0];
            D = a2[0,0];
            E = a2[1,0];
            F = a2[2,0];

            B = B / 2;
            D = D / 2;
            E = E / 2;

            double den = Math.Pow(B, 2) - A * C;
            if(den > 0)
            {
                Console.WriteLine("No ellipse found!");
            }

            x0 = (C * D - B * E) / den;
            y0 = (A * E - B * D) / den;

            double num = 2 * (A * E * E + C * D * D + F * B * B - 2 * B * D * E - A * C * F);
            double fac = Math.Sqrt(Math.Pow(A-C,2) + 4 * B * B);

            a = Math.Sqrt(num / den / (fac - A - C));
            b = Math.Sqrt(num / den / (-fac - A - C));

            resetGraph2();
            

            //Create 2 Calibrating matrices "A" and "B".
            Matrix<double> Acal = Matrix<double>.Build.Dense(2, 2, 0.0);
            Matrix<double> Bcal = Matrix<double>.Build.Dense(1, 2, 0.0);

            Bcal[0, 0] = x0;
            Bcal[0, 1] = y0;

            if (a<b)
            {
                r = a;
                scale = a / b;
                Acal[0, 0] = 1;
                Acal[1, 1] = scale;
            }

            else if (b<a)
            {
                r = b;
                scale = b / a;
                Acal[0, 0] = scale;
                Acal[1, 1] = 1;
            }

            txtA00.Text = Acal[0, 0].ToString();
            txtA01.Text = Acal[0, 1].ToString();
            txtA10.Text = Acal[1, 0].ToString();
            txtA11.Text = Acal[1, 1].ToString();

            txtB00.Text = Bcal[0, 0].ToString();
            txtB01.Text = Bcal[0, 1].ToString();

            double[] xplot = new double[n];
            double[] yplot = new double[n];
            double[] xcalplot = new double[n];
            double[] ycalplot = new double[n];

            for (i = 0; i < n; i++)
            {
                xplot[i] = data[i, 0];
                yplot[i] = data[i, 1];

                xcalplot[i] = Acal[0, 0] * (xplot[i] - Bcal[0, 0]);
                ycalplot[i] = Acal[1, 1] * (yplot[i] - Bcal[0, 1]);
            }

            double[] ellipse_x = new double[360];
            double[] ellipse_y = new double[360];
            double[] circle_x = new double[360];
            double[] circle_y = new double[360];

            //plotting circle and ellipse
            double alpha = 0;
            for (i = 0; i<360; i++)
            {
                alpha = i * Math.PI / 180;
                ellipse_x[i] = x0 + a * Math.Cos(alpha);
                ellipse_y[i] = y0 + b * Math.Sin(alpha);

                circle_x[i] = r * Math.Cos(alpha);
                circle_y[i] = r * Math.Sin(alpha);  
            }

            

            var Original = zedGraphControl2.GraphPane.AddCurve("Original Data", xplot, yplot, Color.Red);
            Original.Line.IsVisible = false;

            var Ellipse = zedGraphControl2.GraphPane.AddCurve("Ellipse", ellipse_x, ellipse_y, Color.Red);
            Ellipse.Line.IsVisible = true;
            Ellipse.Symbol.IsVisible = false;

            var Calibrated = zedGraphControl2.GraphPane.AddCurve("Calibrated Data", xcalplot, ycalplot, Color.Blue);
            Calibrated.Line.IsVisible = false;

            var Circle = zedGraphControl2.GraphPane.AddCurve("Circle", circle_x, circle_y, Color.Blue);
            Circle.Line.IsVisible = true;
            Circle.Symbol.IsVisible = false;

            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();

            //zedGraphControl2.GraphPane.Chart.IsRectAuto = true;
            zedGraphControl2.GraphPane.AxisChange();
            
            
            
            zedGraphControl2.Refresh();

        }

        private void drawEllipse(double a, double b, double x, double y, double[] list_x, double[] list_y)
        {
            double count, temp;
            for(count = a+x; count > (x-a); count-=0.1)
            {
                temp = Math.Sqrt((1 - Math.Pow(count - x, 2) / Math.Pow(a, 2)) * Math.Pow(b, 2)) + y;
                list_x.Append(temp);
            }
            for (count = x-a; count < (x + a); count += 0.1)
            {

            }

        }






        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void txtDebug_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void boxBaud_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }
}

