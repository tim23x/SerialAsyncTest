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
        SerialPort aliPort, rightPort;
        private char[] MFCLetters = new char[] { 'A', 'B', 'E' };
        private int MFCCounter = 0;

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
            //Task.Factory.StartNew(() => GetData(lData, cts.Token, "COM28"));
            aliPort = SerialConnect("COM43", 19200);
            //rightPort = SerialConnect("COM9", 115200);
            Task.Factory.StartNew(() => GetAlicatData(lData, cts.Token, aliPort));
            Task.Factory.StartNew(() => AlicatPoll(cts.Token));
            Task.Factory.StartNew(() => AddData(lData, cts.Token, plotLeft));
            //Task.Factory.StartNew(() => GetData(rData, cts.Token, "COM9"));
            //Task.Factory.StartNew(() => GetData(rData, cts.Token, rightPort));
            //Task.Factory.StartNew(() => AddData(rData, cts.Token, plotRight));
        }

        public void GetData(ConcurrentQueue<string> qData, CancellationToken token, SerialPort port)
        {
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

        public void GetAlicatData(ConcurrentQueue<string> qData, CancellationToken token, SerialPort port)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                if (port.BytesToRead > 0)
                {
                    string received = port.ReadTo("\r");
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
                    String[] data = local.Split(' ');
                    string point = data[1];
                    try
                    {
                        DataPoint dp = new DataPoint(xTime.ElapsedMilliseconds, Convert.ToDouble(point));
                        plot.AddDataPointToSeries(0, dp);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }

        public void AlicatPoll(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                if (MFCCounter > 2)
                    MFCCounter = 0;
                String aliCommand = MFCLetters[MFCCounter].ToString() + "\r";
                aliPort.Write(aliCommand);
                Thread.Sleep(50);
            }
        }


        private SerialPort SerialConnect(String name, Int32 baud)
        {
            SerialPort port = new SerialPort();
            port.PortName = name;
            port.BaudRate = baud;
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
