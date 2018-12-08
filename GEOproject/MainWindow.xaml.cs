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

namespace GEOproject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> points = new List<Point>();
        List<Polygon> triangles = new List<Polygon>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawLine(Point p1, Point p2)
        {
            Line line = new Line();
            line.StrokeThickness = 4;
            line.Stroke = Brushes.Blue;
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
            drawSurface.Children.Add(line);
        }

        private bool IsClockwise()
        {
            double sum = 0;
            for (int i = 0; i < points.Count(); i++)
            {
                Point p1 = points[i];
                Point p2 = points[(i + 1) % points.Count()];
                sum += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
            return sum >= 0;
        }

        private bool ValidTriangle(Polygon triangle, Point p1, Point p2, Point p3)
        {
            foreach (Point p in points)
            {
                if (p != p1 && p != p2 && p != p3 && triangle.Points.IndexOf(p) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void TriangulatePolygon()
        {
            bool clockwise = IsClockwise();
            int index = 0;

            while (points.Count() > 2)
            {

                Point p1 = points[(index + 0) % points.Count()];
                Point p2 = points[(index + 1) % points.Count()];
                Point p3 = points[(index + 2) % points.Count()];
                
                Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y);
                Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y);
                double cross = Vector.CrossProduct(v1, v2);

                Polygon triangle = new Polygon();
                PointCollection trianglePointCollection = new PointCollection();
                trianglePointCollection.Add(p1);
                trianglePointCollection.Add(p2);
                trianglePointCollection.Add(p3);
                triangle.Points = trianglePointCollection;
                triangle.Stroke = Brushes.Red;
                triangle.StrokeThickness = 2;

                if (!clockwise && cross >= 0 && ValidTriangle(triangle, p1, p2, p3))
                {
                    points.Remove(p2);
                    triangles.Add(triangle);
                    drawSurface.Children.Add(triangle);
                }
                else if (clockwise && cross <= 0 && ValidTriangle(triangle, p1, p2, p3))
                {
                    points.Remove(p2);
                    triangles.Add(triangle);
                    drawSurface.Children.Add(triangle);
                }
                else
                {
                    index++;
                }
            }

            if (points.Count() < 3)
            {
                points.Clear();
            }
        }

        private void Canvas_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            // If this is the first point
            if (points.Count() == 0)
            {
                points.Add(e.GetPosition(this));
            }
            else
            {
                Point lastPoint = points[points.Count - 1];
                Point currPoint = e.GetPosition(this);
                points.Add(currPoint);
                DrawLine(lastPoint, currPoint);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Point firstPoint = points[0];
            Point lastPoint = points[points.Count - 1];
            DrawLine(lastPoint, firstPoint);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            points.RemoveAt(points.Count - 1);
            drawSurface.Children.RemoveAt(drawSurface.Children.Count - 1);
        }

        private void Compute_Click(object sender, RoutedEventArgs e)
        {
            TriangulatePolygon();
        }
    }
}
