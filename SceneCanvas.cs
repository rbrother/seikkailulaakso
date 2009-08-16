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
        private bool drawing = false;
        private List<Point> objectPoints;
        private List<Line> tempLines;

        private const double POLYGON_SIDE = 25.0;

        public SceneCanvas()
        {
            this.Background = new SolidColorBrush(Colors.Yellow);
        }

        internal void StartDrawing(MouseButtonEventArgs e)
        {
            drawing = true;
            this.objectPoints = new List<Point>();
            this.tempLines = new List<Line>();
            this.objectPoints.Add(e.GetPosition(this));
        }

        internal void ContinueDrawing(MouseEventArgs e)
        {
            if (drawing)
            {
                Point newPos = e.GetPosition(this);
                var distanceFromLast = Distance(newPos, objectPoints.Last());
                if (distanceFromLast > POLYGON_SIDE)
                {
                    var line = new Line { X1 = objectPoints.Last().X, Y1 = objectPoints.Last().Y, X2 = newPos.X, Y2 = newPos.Y };
                    line.Stroke = new SolidColorBrush(Colors.Black);
                    line.StrokeThickness = 2;
                    objectPoints.Add(newPos);
                    this.Children.Add(line);
                    this.tempLines.Add(line);
                }
            }
        }

        internal void EndDrawing(MouseButtonEventArgs e)
        {
            if (drawing)
            {
                MakeObject();
                this.drawing = false;
                this.objectPoints = null;
            }
        }

        private void MakeObject()
        {
            RemoveTempLines();
            double minX = objectPoints.Min(p => p.X);
            double minY = objectPoints.Min(p => p.Y);
            var poly = CreateDefaultPolygon();
            poly.Points = new PointCollection(this.objectPoints.Select(p => new Point(p.X - minX, p.Y - minY))) ;            
            SetTop(poly, minY);
            SetLeft(poly, minX);
            this.Children.Add(poly);
        }

        public void AddRectPolygon(double top, double left, double width, double height)
        {
            var poly = CreateDefaultPolygon();
            poly.Points.Add(new Point(0, 0));
            poly.Points.Add(new Point(width, 0));
            poly.Points.Add(new Point(width, height));
            poly.Points.Add(new Point(0, height));
            SetTop(poly, top);
            SetLeft(poly, left);
            this.Children.Add(poly);
        }

        private void RemoveTempLines() {
            foreach (var line in this.tempLines) {
                this.Children.Remove(line);
            }
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
