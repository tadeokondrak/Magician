using System;
using Magician.Library;

namespace Magician.Geo
{
    public static class Ref
    {
        // Reference to the Origin of the current Spell (see Spellcaster.Load)
        public static Multi Origin
        {
            get; set;
        }
        static Ref()
        {
            Origin = new Multi().Tagged("Reference Origin");
        }
    }
    public static class Create
    {
        // Create a point
        public static Multi Point(Multi? parent, double x, double y, Color col)
        {
            return new Multi(parent, x, y, col).DrawFlags(DrawMode.POINT);
        }
        public static Multi Point(double x, double y, Color col)
        {
            return Point(Ref.Origin, x, y, col).DrawFlags(DrawMode.POINT);
        }
        public static Multi Point(double x, double y)
        {
            return Point(Ref.Origin, x, y, Data.Col.UIDefault.FG);
        }

        // Create a line
        public static Multi Line(Multi p1, Multi p2, Color col)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double x2 = p2.X;
            double y2 = p2.Y;

            Multi line = new Multi(0, 0, col, DrawMode.PLOT,
            Point(x1, y1, col),
            Point(x2, y2, col));
            // Make sure the parents are set correctly
            line[0].Parented(line);
            line[1].Parented(line);
            return line;
        }
        public static Multi Line(Multi p1, Multi p2)
        {
            return Line(p1, p2, Data.Col.UIDefault.FG);
        }

        // TODO: make Rect easier to implement with a Flatten method
        // Rect from 2 points
        public static Multi Rect(Multi p0, Multi p1)
        {
            return Rect(p0.X, p0.Y, p1.X-p0.X, p1.Y-p0.Y);
        }
        public static Multi Rect(double x, double y, double width, double height)
        {
            return new Multi(
                Point(width, -height),
                Point(width, 0),
                Point(0, 0),
                Point(0, -height)
            ).Positioned(x, y);
        }

        // Create a regular polygon with a position, number of sides, color, and magnitude
        public static Multi RegularPolygon(double xOffset, double yOffset, Color col, int sides, double magnitude)
        {
            //List<Multi> ps = new List<Multi>();
            Multi ps = new Multi().Parented(Ref.Origin);
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double x = magnitude * Math.Cos(angle * i / 180 * Math.PI);
                double y = magnitude * Math.Sin(angle * i / 180 * Math.PI);
                ps.Add(Point(ps, x, y, Data.Col.UIDefault.FG));
            }

            //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
            return ps.Positioned(xOffset, yOffset).Colored(col).DrawFlags(DrawMode.INNER);

        }
        public static Multi RegularPolygon(double xOffset, double yOffset, int sides, double magnitude)
        {
            return RegularPolygon(xOffset, yOffset, Data.Col.UIDefault.FG, sides, magnitude);
        }
        public static Multi RegularPolygon(int sides, double magnitude)
        {
            return RegularPolygon(0, 0, sides, magnitude);
        }

        // Create a star with an inner and outer radius
        public static Multi Star(double xOffset, double yOffset, Color col, int sides, double innerRadius, double outerRadius)
        {
            Multi ps = new Multi().Parented(Ref.Origin);
            double angle = 360d / (double)sides;
            for (int i = 0; i < sides; i++)
            {
                double innerX = innerRadius * Math.Cos(angle * i / 180 * Math.PI);
                double innerY = innerRadius * Math.Sin(angle * i / 180 * Math.PI);
                double outerX = outerRadius * Math.Cos((angle * i + angle / 2) / 180 * Math.PI);
                double outerY = outerRadius * Math.Sin((angle * i + angle / 2) / 180 * Math.PI);
                ps.Add(Point(innerX, innerY, col));
                ps.Add(Point(outerX, outerY, col));
            }

            return ps.Positioned(xOffset, yOffset).Colored(col).DrawFlags(DrawMode.INNER);
            //return new Multi(xOffset, yOffset, col, DrawMode.FULL, ps.ToArray());
        }
        public static Multi Star(double xOffset, double yOffset, int sides, double innerRadius, double outerRadius)
        {
            return Star(xOffset, yOffset, Data.Col.UIDefault.FG, sides, innerRadius, outerRadius);
        }
        public static Multi Star(int sides, double innerRadius, double outerRadius)
        {
            return Star(0, 0, sides, innerRadius, outerRadius);
        }
    }

    public static class Check
    {
        // TODO: implement this by making the triangulated triangles global and linking each Multi to its set of vertices
        public static bool PointInPolygon(double x, double y, Multi polygon)
        {
            // Special case for rectangles
            if (IsRectangle(polygon))
            {
                double minX = Math.Min(polygon[0].X, polygon[2].X);
                double xRange = Math.Max(
                   Math.Abs(polygon[0].X - polygon[1].X),
                   Math.Abs(polygon[0].X - polygon[3].X)
                );
                double minY = Math.Min(polygon[0].Y, polygon[2].Y);
                double yRange = Math.Max(
                   Math.Abs(polygon[0].Y - polygon[1].Y),
                   Math.Abs(polygon[0].Y - polygon[3].Y)
                );
                return (x >= minX) && (x < minX + xRange) && (y >= minY) && (y < minY + yRange);
            }
            
            // For other shapes, grab the triangles from Siedel's algo and check each
            List<int[]> triangles = Seidel.Triangulator.Triangulate(polygon);
            foreach (int[] vertexIdx in triangles)
            {
                if (vertexIdx.Length != 3) {Scribe.Issue("Renderer gave bad triangle :(");}
                double x0, y0, x1, y1, x2, y2;
                int idx0 = vertexIdx[0];
                int idx1 = vertexIdx[1];
                int idx2 = vertexIdx[2];
                
                // If all vertex indices are zero, we're done
                if (idx0 + idx1 + idx2 == 0)
                {
                    break;
                }

                // Calculate absolute coordinates of triangle
                x0 = polygon[idx0-1].X;
                y0 = polygon[idx0-1].Y;
                x1 = polygon[idx1-1].X;
                y1 = polygon[idx1-1].Y;
                x2 = polygon[idx2-1].X;
                y2 = polygon[idx2-1].Y;

                // These two vectors add up to the position of the mouse
                double v0 = (x0*(y2-y0)+(y-y0)*(x2-x0)-x*(y2-y0)) / ((y1-y0)*(x2-x0)-(x1-x0)*(y2-y0));
                double v1 = (y - y0 - v0*(y1-y0)) / (y2-y0);
                
                // Point is NOT in triangle
                if (v0 < 0 || v1 < 0 || Math.Abs(v0) > 1 || Math.Abs(v1) > 1 || v0+v1 > 1 || double.IsNaN(v0) || double.IsNaN(v1))
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        public static bool IsRectangle(Multi m, double tolerance = Data.Globals.defaultTol)
        {
            if (m.Count != 4)
            {
                return false;
            }
            Multi v0 = m[0];

            // Either the x or the y of the first must match the x or y of the neighbour, within a tolerance
            return (Math.Abs(m[0].X - m[1].X) <= tolerance) ||
                    (Math.Abs(m[0].Y - m[1].Y) <= tolerance);
        }
    }

    public static class Find
    {
        /* Find the Euclidian distance */
        public static double Distance(double x0, double x1, double y0, double y1)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static double Distance(Multi m0, Multi m1)
        {
            return Distance(m0.X, m1.X, m0.Y, m1.Y);
        }
        public static double Distance(Multi m)
        {
            if (m.Count != 2)
            {
                throw new NotImplementedException("Given Multi was not a Line!");
            }
            return Distance(m[0], m[1]);
        }
    }
}
