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
        SerialPort leftPort, rightPort;
        Stopwatch xTime;
        ConcurrentQueue<String> lData, rData;
        CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            pltLeft.DataContext = plotLeft;
            plotLeft.AddDataPointToSeries(0, new DataPoint(0, 1));
            pltRight.DataContext = plotRight;
            lData = new ConcurrentQueue<string>();
            rData = new ConcurrentQueue<string>();
            cts = new CancellationTokenSource();
            xTime = new Stopwatch();
            xTime.Start();
            leftPort=SerialConnect("COM45");
            var sw = new Stopwatch();
            sw.Start();
            if(leftPort.BytesToRead > 0)
            {
                Debug.WriteLine("leftPort.ReadExisting Elapsed: {0}", sw.Elapsed);
                string received = leftPort.ReadExisting();
                Debug.WriteLine("leftPort Elapsed: {0}, received: {1}", sw.Elapsed, received);
            }
            Task.Factory.StartNew(() => AddLeftData(lData, cts.Token));
            leftPort.DataReceived += new SerialDataReceivedEventHandler(LeftDataReceivedHandler); 
            rightPort = SerialConnect("COM47");
            if (rightPort.BytesToRead > 0)
            {
                Debug.WriteLine("rightPort.ReadExisting Elapsed: {0}", sw.Elapsed);
                string received = rightPort.ReadExisting();
                Debug.WriteLine("rightPort Elapsed: {0}, received: {1}", sw.Elapsed, received);
            }
            Task.Factory.StartNew(() => AddRightData(rData, cts.Token));
            rightPort.DataReceived += new SerialDataReceivedEventHandler(RightDataReceivedHandler);
        }

        private void LeftDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            lData.Enqueue(received);
        }

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

        private void RightDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            rData.Enqueue(received);
        }

        public void AddRightData(ConcurrentQueue<string> qData, CancellationToken token)
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
