using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Net.Brotherus.SeikkailuLaakso
{
    public class SceneCanvas : Canvas
    {
        private const double TILE_SIZE = 60.0;

        public SceneCanvas()
        {
            this.Background = new SolidColorBrush(Colors.Yellow);
        }

        private static Point CoarsePosition(Point pos) {
            return new Point(Quantize(pos.X), Quantize(pos.Y));
        }

        private static int Quantize(double val) {
            return ( Convert.ToInt32(val) / 20) * 20;
        }

        #region STATIC

        private static Polygon CreateDefaultPolygon()
        {
            Polygon poly = new Polygon();
            poly.Stroke = null;
            poly.Fill = new SolidColorBrush(Colors.Red);
            return poly;
        }

        private static double Distance(Point a, Point b) {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion

    } // class

} // namespace
