using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Interface
{
    public class PlotViewModel
    {
        public string Title { get; set; }
        public IList<DataPoint> Points { get; set; }
        /// <summary>
        /// Set maxDataPoints to zero to display all points.
        /// Default = 200
        /// </summary>
        public long maxDataPoints { get; set; }
        private bool disposed;

        public PlotModel plotModel { get; private set; }

        public PlotViewModel(string title)
        {
            plotModel = new PlotModel { Title = title };
            maxDataPoints = 200;
            var linearAxisCurrent = new LinearAxis { Position = AxisPosition.Left };
            linearAxisCurrent.Key = "Current";
            linearAxisCurrent.Unit = "Current (mA)";
            plotModel.Axes.Add(linearAxisCurrent);
            var seriesCurrent = new LineSeries { Title = "Current (mA)", MarkerType = MarkerType.Cross };
            seriesCurrent.Color = OxyColor.FromRgb(255, 0, 0);
            seriesCurrent.YAxisKey = "Current";
            plotModel.Series.Add(seriesCurrent);
            plotModel.LegendPlacement = LegendPlacement.Inside;
            plotModel.LegendPosition = LegendPosition.TopRight;
            //plotModel.LegendPosition = LegendPosition.
        }

        public void AddDataPointToSeries(int index, DataPoint p)
        {
            var s = (LineSeries)plotModel.Series[index];
            s.Points.Add(p);
            if (maxDataPoints > 0)
            {
                if (s.Points.Count >= maxDataPoints)
                    for (int i = 0; i < s.Points.Count - maxDataPoints; i++)
                        s.Points.RemoveAt(0);
            }
            this.plotModel.InvalidatePlot(true);
        }

        public void ClearPlot()
        {
            foreach (LineSeries s in plotModel.Series)
                s.Points.Clear();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //this.timer.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
