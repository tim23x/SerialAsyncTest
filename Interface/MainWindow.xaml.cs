using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            pltLeft.DataContext = plotLeft;
            plotLeft.AddDataPointToSeries(0, new DataPoint(0, 1));
            pltRight.DataContext = plotRight;
            xTime = new Stopwatch();
            xTime.Start();
            //leftPort=SerialConnect("COM45");
            leftPort = new SerialPort();
            leftPort.PortName = "COM45";
            leftPort.BaudRate = 115200;
            leftPort.Parity = Parity.None;
            leftPort.StopBits = StopBits.One;
            leftPort.DataBits = 8;
            leftPort.Handshake = Handshake.None;
            leftPort.RtsEnable = true;
            leftPort.Open();
            if(leftPort.BytesToRead > 0)
            {
                string received = leftPort.ReadExisting();
            }
            leftPort.DataReceived += new SerialDataReceivedEventHandler(LeftDataReceivedHandler);
            
            rightPort = new SerialPort();
            rightPort.PortName = "COM47";
            rightPort.BaudRate = 115200;
            rightPort.Parity = Parity.None;
            rightPort.StopBits = StopBits.One;
            rightPort.DataBits = 8;
            rightPort.Handshake = Handshake.None;
            rightPort.RtsEnable = true;
            rightPort.Open();
            if (rightPort.BytesToRead > 0)
            {
                string received = rightPort.ReadExisting();
            }
            rightPort.DataReceived += new SerialDataReceivedEventHandler(RightDataReceivedHandler);
        }

        private void LeftDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            String[] data = received.Split('#');
            foreach(string point in data)
            {
                try
                {
                    DataPoint dp = new DataPoint(xTime.ElapsedMilliseconds, Convert.ToDouble(point));
                    plotLeft.AddDataPointToSeries(0, dp);
                }
                catch(Exception ex)
                { }
            }
        }

        private void RightDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var s = sender as SerialPort;
            string received = s.ReadExisting();
            String[] data = received.Split('#');
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

        private SerialPort SerialConnect(String name)
        {
            SerialPort port = new SerialPort();
            port.PortName = name;
            port.BaudRate = 115200;
            port.Open();
            return port;
        }
    }
}
