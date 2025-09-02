using System;
using System.Windows;

namespace wpfUI
{
    public static class WireIntersectionHelper
    {
        private const double INTERSECTION_TOLERANCE = 0.1;

        public static bool LinesIntersect(Point line1Start, Point line1End, Point line2Start, Point line2End, out Point intersection)
        {
            intersection = new Point();

            double a1 = line1End.Y - line1Start.Y;
            double b1 = line1Start.X - line1End.X;
            double c1 = a1 * line1Start.X + b1 * line1Start.Y;

            double a2 = line2End.Y - line2Start.Y;
            double b2 = line2Start.X - line2End.X;
            double c2 = a2 * line2Start.X + b2 * line2Start.Y;

            double determinant = a1 * b2 - a2 * b1;

            if (Math.Abs(determinant) < INTERSECTION_TOLERANCE)
                return false;

            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;
            intersection = new Point(x, y);

            // Check if intersection point lies within both line segments
            if (!IsPointOnLineSegment(line1Start, line1End, intersection) ||
                !IsPointOnLineSegment(line2Start, line2End, intersection))
                return false;

            return true;
        }

        public static bool IsPointOnLineSegment(Point lineStart, Point lineEnd, Point point)
        {
            double tolerance = 1.0; // Adjust this value based on your grid size and precision needs

            // Check if point is within the bounding box of the line segment
            if (point.X < Math.Min(lineStart.X, lineEnd.X) - tolerance ||
                point.X > Math.Max(lineStart.X, lineEnd.X) + tolerance ||
                point.Y < Math.Min(lineStart.Y, lineEnd.Y) - tolerance ||
                point.Y > Math.Max(lineStart.Y, lineEnd.Y) + tolerance)
                return false;

            // For vertical lines
            if (Math.Abs(lineEnd.X - lineStart.X) < tolerance)
                return Math.Abs(point.X - lineStart.X) < tolerance;

            // For horizontal lines
            if (Math.Abs(lineEnd.Y - lineStart.Y) < tolerance)
                return Math.Abs(point.Y - lineStart.Y) < tolerance;

            // For other lines, check if point lies on the line using distance
            double lineLength = (lineEnd - lineStart).Length;
            double d1 = (point - lineStart).Length;
            double d2 = (point - lineEnd).Length;

            return Math.Abs(d1 + d2 - lineLength) < tolerance;
        }
    }
}
