﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace look.sender.wpf.controls._3rdParty.AnimatingTilePanel
{
    using System.Diagnostics;
    using System.Windows;

    public static class GeoHelper
    {
        public static bool IsValid(this double value)
        {
            return !double.IsInfinity(value) && !double.IsNaN(value);
        }

        public static bool IsValid(this Point value)
        {
            return value.X.IsValid() && value.Y.IsValid();
        }

        public static bool IsValid(this Size value)
        {
            return value.Width.IsValid() && value.Height.IsValid();
        }

        public static bool IsValid(this Vector value)
        {
            return value.X.IsValid() && value.Y.IsValid();
        }

        /// <summary>
        ///     Returns the scale factor by which an object of size <paramref name="source"/>
        ///     should be scaled to fit within an object of size <param name="target"/>.
        /// </summary>
        /// <param name="target">The target size.</param>
        /// <param name="size2">The source size.</param>
        public static double ScaleToFit(Size target, Size source)
        {
            double targetHWR = target.Height / target.Width;
            double sourceHWR = source.Height / source.Width;

            if (targetHWR > sourceHWR)
            {
                return target.Width / source.Width;
            }
            else
            {
                return target.Height / source.Height;
            }
        }

        public static bool Animate(
            double currentValue, double currentVelocity, double targetValue,
            double attractionFator, double dampening,
            double terminalVelocity, double minValueDelta, double minVelocityDelta,
            out double newValue, out double newVelocity)
        {
            Debug.Assert(currentValue.IsValid());
            Debug.Assert(currentVelocity.IsValid());
            Debug.Assert(targetValue.IsValid());

            Debug.Assert(dampening.IsValid());
            Debug.Assert(dampening > 0 && dampening < 1);

            Debug.Assert(attractionFator.IsValid());
            Debug.Assert(attractionFator > 0);

            Debug.Assert(terminalVelocity > 0);

            Debug.Assert(minValueDelta > 0);
            Debug.Assert(minVelocityDelta > 0);

            double diff = targetValue - currentValue;

            if (diff.Abs() > minValueDelta || currentVelocity.Abs() > minVelocityDelta)
            {
                newVelocity = currentVelocity * (1 - dampening);
                newVelocity += diff * attractionFator;
                if (currentVelocity.Abs() > terminalVelocity)
                {
                    newVelocity *= terminalVelocity / currentVelocity.Abs();
                }

                newValue = currentValue + newVelocity;

                return true;
            }
            else
            {
                newValue = targetValue;
                newVelocity = 0;
                return false;
            }
        }

        public static bool Animate(
        Point currentValue, Vector currentVelocity, Point targetValue,
        double attractionFator, double dampening,
        double terminalVelocity, double minValueDelta, double minVelocityDelta,
        out Point newValue, out Vector newVelocity)
        {
            Debug.Assert(currentValue.IsValid());
            Debug.Assert(currentVelocity.IsValid());
            Debug.Assert(targetValue.IsValid());

            Debug.Assert(dampening.IsValid());
            Debug.Assert(dampening > 0 && dampening < 1);

            Debug.Assert(attractionFator.IsValid());
            Debug.Assert(attractionFator > 0);

            Debug.Assert(terminalVelocity > 0);

            Debug.Assert(minValueDelta > 0);
            Debug.Assert(minVelocityDelta > 0);

            Vector diff = targetValue.Subtract(currentValue);

            if (diff.Length > minValueDelta || currentVelocity.Length > minVelocityDelta)
            {
                newVelocity = currentVelocity * (1 - dampening);
                newVelocity += diff * attractionFator;
                if (currentVelocity.Length > terminalVelocity)
                {
                    newVelocity *= terminalVelocity / currentVelocity.Length;
                }

                newValue = currentValue + newVelocity;

                return true;
            }
            else
            {
                newValue = targetValue;
                newVelocity = new Vector();
                return false;
            }
        }

        public static Vector Subtract(this Point point, Point other)
        {
            return new Vector(point.X - other.X, point.Y - other.Y);
        }

        public static Vector Subtract(this Size size, Size other)
        {
            return new Vector(size.Width - other.Width, size.Height - other.Height);
        }

        public static double Abs(this double value)
        {
            return Math.Abs(value);
        }

        public static Point GetCenter(this Rect value)
        {
            return new Point(value.X + value.Width / 2, value.Y + value.Height / 2);
        }

        public static Rect Expand(this Rect target, double amount)
        {
            var value = new Rect(target.X - amount, target.Y - amount, target.Width + 2 * amount, target.Height + 2 * amount);
            return value;
        }

        public static Point TopLeft(this Rect rect)
        {
            return new Point(rect.Left, rect.Top);
        }

        public static Point BottomRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Bottom);
        }

        public static Point BottomLeft(this Rect rect)
        {
            return new Point(rect.Left, rect.Bottom);
        }

        public static Point TopRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Top);
        }

        public static Size Size(this Rect rect)
        {
            return new Size(rect.Width, rect.Height);
        }

        public static Point ToPoint(this Vector vector)
        {
            return (Point)vector;
        }

        public static Vector CenterVector(this Size size)
        {
            return ((Vector)size) * .5;
        }

        public static double AngleRad(Point point1, Point point2, Point point3)
        {
            Debug.Assert(point1.IsValid());
            Debug.Assert(point2.IsValid());
            Debug.Assert(point3.IsValid());

            double rad = AngleRad(point2.Subtract(point1), point2.Subtract(point3));

            double rad2 = AngleRad(point2.Subtract(point1), (point2.Subtract(point3)).RightAngle());

            if (rad2 < (Math.PI / 2))
            {
                return rad;
            }
            else
            {
                return (Math.PI * 2) - rad;
            }
        }

        public static Vector RightAngle(this Vector vector)
        {
            return new Vector(-vector.Y, vector.X);
        }

        public static double Dot(Vector v1, Vector v2)
        {
            Debug.Assert(v1.IsValid());
            Debug.Assert(v2.IsValid());

            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static double AngleRad(Vector v1, Vector v2)
        {
            Debug.Assert(v1.IsValid());
            Debug.Assert(v2.IsValid());

            double dot = Dot(v1, v2);
            double dotNormalize = dot / (v1.Length * v2.Length);
            double acos = Math.Acos(dotNormalize);

            return acos;
        }

        public static Vector GetVectorFromAngle(double angleRadians, double length)
        {
            double x = Math.Cos(angleRadians) * length;
            double y = -Math.Sin(angleRadians) * length;

            return new Vector(x, y);
        }

        public static readonly Size SizeInfinite = new Size(double.PositiveInfinity, double.PositiveInfinity);
    }
}
