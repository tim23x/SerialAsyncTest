using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using OxyPlot;

namespace Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PlotViewModel plotLeft = new PlotViewModel("Left");
        public PlotViewModel plotRight = new PlotViewModel("Right");
        Stopwatch xTime;
        ConcurrentQueue<String> lData, rData;
        CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            pltLeft.DataContext = plotLeft;
            pltRight.DataContext = plotRight;
            lData = new ConcurrentQueue<string>();
            rData = new ConcurrentQueue<string>();
            cts = new CancellationTokenSource();
            xTime = new Stopwatch();
            xTime.Start();
            Task.Factory.StartNew(() => GetData(lData, cts.Token, "COM28"));
            Task.Factory.StartNew(() => AddData(lData, cts.Token, plotLeft));
            Task.Factory.StartNew(() => GetData(rData, cts.Token, "COM9"));
            Task.Factory.StartNew(() => AddData(rData, cts.Token, plotRight));
        }

        public void GetData(ConcurrentQueue<string> qData, CancellationToken token, String portName)
        {
            SerialPort port = SerialConnect(portName);
            //leftPort.DataReceived += new SerialDataReceivedEventHandler(LeftDataReceivedHandler); 
            while (true)
            {
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                    break;
                if (port.BytesToRead > 0)
                {
                    //string received = port.ReadExisting();
                    string received = port.ReadTo("#");
                    qData.Enqueue(received);
                }
            }
        }

        public void AddData(ConcurrentQueue<string> qData, CancellationToken token, PlotViewModel plot)
        {
            string local;
            while (true)
            {
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                    break;
                while (qData.TryDequeue(out local))
                {
                    String[] data = local.Split('#');
                    foreach (string point in data)
                    {
                        try
                        {
                            DataPoint dp = new DataPoint(xTime.ElapsedMilliseconds, Convert.ToDouble(point));
                            plot.AddDataPointToSeries(0, dp);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                Thread.Sleep(1);
            }
        }
        /*
        public void AddLeftData(ConcurrentQueue<string> qData, CancellationToken token)
        {
            string local;
            while (true)
            {
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                    break;
                while (qData.TryDequeue(out local))
                {
                    String[] data = local.Split('#');
                    foreach (string point in data)
                    {
                        try
                        {
                            DataPoint dp = new DataPoint(xTime.ElapsedMilliseconds, Convert.ToDouble(point));
                            plotLeft.AddDataPointToSeries(0, dp);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                Thread.Sleep(1);
            }
        }


        public void GetRightData(ConcurrentQueue<string> qData, CancellationToken token, SerialPort rightPort)
        {
            rightPort = SerialConnect("COM9");
            //rightPort.DataReceived += new SerialDataReceivedEventHandler(RightDataReceivedHandler);
            if (rightPort.BytesToRead > 0)
            {
                string received = rightPort.ReadExisting();

                qData.Enqueue(received);
            }
        }

        private void AddRightData(ConcurrentQueue<string> qData, CancellationToken token)
        {
            string local;
            while (true)
            {
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                    break;
                while (qData.TryDequeue(out local))
                {
                    String[] data = local.Split('#');
                    foreach (string point in data)
                    {
                        try
                        {
                            DataPoint dp = new DataPoint(xTime.ElapsedMilliseconds, Convert.ToDouble(point));
                            plotRight.AddDataPointToSeries(0, dp);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
                Thread.Sleep(1);
            }
        }

        public void GetLeftData(ConcurrentQueue<string> qData, CancellationToken token, String portName)
        {
            SerialPort port = SerialConnect(portName);
            //leftPort.DataReceived += new SerialDataReceivedEventHandler(LeftDataReceivedHandler); 
            while (true)
            {
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                    break;

                if (port.BytesToRead > 0)
                {
                    string received = port.ReadExisting();
                    qData.Enqueue(received);
                }
            }
        }

        private void LeftDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            lData.Enqueue(received);
        }

        private void RightDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            rData.Enqueue(received);
        }


    */
        private SerialPort SerialConnect(String name)
        {
            SerialPort port = new SerialPort();
            port.PortName = name;
            port.BaudRate = 115200;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.DataBits = 8;
            port.Handshake = Handshake.None;
            port.RtsEnable = true;
            port.Open();
            return port;
        }
    }
}
