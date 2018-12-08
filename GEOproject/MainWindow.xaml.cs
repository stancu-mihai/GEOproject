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
        List<double> triangleAreas = new List<double>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // Draw a line on the canvas
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

        // get distance between two points
        private double GetLength(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        // Compute the area of a triangle
        private double ComputeArea(Polygon triangle)
        {
            Point p1 = triangle.Points[0];
            Point p2 = triangle.Points[1];
            Point p3 = triangle.Points[2];

            double len12 = GetLength(p1, p2);
            double len23 = GetLength(p2, p3);
            double len13 = GetLength(p1, p3);

            double p = (len12 + len23 + len13)/ 2;

            return Math.Sqrt( p * (p-len12) * (p-len23) * (p-len13));
        }

        // Find out if the polygon was drawn in CW or CCW
        // https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
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

        // Determine if a point is in a polygon by looping through the sides in the same direction,
        // while checking if the number of nodes is odd (meaning the point is inside)
        // http://dominoc925.blogspot.com/2012/02/c-code-snippet-to-determine-if-point-is.html
        private bool IsPointInTriangle(Polygon pg, Point p)
        {
            bool isInside = false;
            int j = pg.Points.Count - 1; // starts from the last point
            for (int i = 0; i < pg.Points.Count; i++) // loops through each side of the polygon
            {
                Point pi = pg.Points[i];
                Point pj = pg.Points[j];
                if (((pi.Y > p.Y) != (pj.Y > p.Y)) && // vertically speaking, pi is on the other side of p as pj
                (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (pj.Y - pi.Y) + pi.X)) //p.X < dist(pi, p).X + pi.X
                {
                    isInside = !isInside;
                }
                j = i;
            }
            return isInside;
        }

        // Checks if the triangle is valid
        private bool ValidTriangle(Polygon triangle, Point p1, Point p2, Point p3)
        {
            foreach (Point p in points)
            {
                if (p != p1 && p != p2 && p != p3 && IsPointInTriangle(triangle, p))
                {
                    return false;
                }
            }
            return true;
        }

        // Triangulation by ear clipping algorithm
        // https://github.com/leonardo-ono/Java2DTriangulationAlgorithmTest1/blob/master/src/View.java
        private void TriangulatePolygon()
        {
            bool clockwise = IsClockwise();
            int index = 0;

            while (points.Count() > 2)
            {
                int len = points.Count();
                // Takes 3 adjacent points
                Point p1 = points[(index + 0) % len];
                Point p2 = points[(index + 1) % len];
                Point p3 = points[(index + 2) % len];
                
                // Makes vectors and cross product
                Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y);
                Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y);
                double cross = Vector.CrossProduct(v1, v2);

                // Creates a triangle
                Polygon triangle = new Polygon();
                PointCollection trianglePointCollection = new PointCollection();
                trianglePointCollection.Add(p1);
                trianglePointCollection.Add(p2);
                trianglePointCollection.Add(p3);
                triangle.Points = trianglePointCollection;
                triangle.Stroke = Brushes.Red;
                triangle.StrokeThickness = 2;

                // If the polygon is CCW and the cross product positive and the triangle is valid then we found an ear
                if (!clockwise && cross >= 0 && ValidTriangle(triangle, p1, p2, p3))
                {
                    points.Remove(p2);
                    triangles.Add(triangle);
                    triangleAreas.Add(ComputeArea(triangle));
                    drawSurface.Children.Add(triangle);
                }
                // If the polygon is CW and the cross product negative and the triangle is valid then we found an ear
                else if (clockwise && cross <= 0 && ValidTriangle(triangle, p1, p2, p3))
                {
                    points.Remove(p2);
                    triangles.Add(triangle);
                    triangleAreas.Add(ComputeArea(triangle));
                    drawSurface.Children.Add(triangle);
                }
                else
                {
                    index++;
                }
            }

            // If there are only 2 points left then they already have the triangles drawn and computed
            if (points.Count() < 3)
            {
                points.Clear();
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            coords.Content = e.GetPosition(this).X + "," + e.GetPosition(this).Y;
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
            double summedArea = 0;
            foreach (double area in triangleAreas)
            {
                summedArea += area;
            }
            MessageBox.Show("Total area is: " + summedArea);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            int len1 = drawSurface.Children.Count;
            for (int i = len1 - 1; i >= 0; i--)
            {
                drawSurface.Children.RemoveAt(i);
            }

            int len2 = points.Count;
            for (int i = len2 - 1; i >= 0; i--)
            {
                points.RemoveAt(i);
            }

            int len3 = triangleAreas.Count;
            for (int i = len3 - 1; i >= 0; i--)
            {
                triangleAreas.RemoveAt(i);
            }
        }
    }
}
