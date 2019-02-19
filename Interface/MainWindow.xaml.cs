using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                if (port.BytesToRead > 0)
                {
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
