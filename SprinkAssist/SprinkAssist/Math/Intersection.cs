using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;

namespace Ironwill.IFEMath
{
	internal class Intersection
	{
		internal static bool Extents3dContainsPoint3d(Extents3d extents, Point3d point)
		{
			return
			(
				point.X > extents.MinPoint.X &&
				point.X < extents.MaxPoint.X &&
				point.Y > extents.MinPoint.Y &&
				point.Y < extents.MaxPoint.Y &&
				point.Z > extents.MinPoint.Z &&
				point.Z < extents.MaxPoint.Z
			);
		}

		internal static bool Extents3dCollidesWithExtents3d(Extents3d box1, Extents3d box2)
		{
			return	IntervalOverlaps(box1.MinPoint.X, box1.MaxPoint.X, box2.MinPoint.X, box2.MaxPoint.X)
					&&
					IntervalOverlaps(box1.MinPoint.Y, box1.MaxPoint.Y, box2.MinPoint.Y, box2.MaxPoint.Y)
					&&
					IntervalOverlaps(box1.MinPoint.Z, box1.MaxPoint.Z, box2.MinPoint.Z, box2.MaxPoint.Z);
		}

		internal static bool IntervalOverlaps(double i1a, double i1b, double i2a, double i2b)
		{
			if (i1b < i1a)
			{
				double temp = i1a;
				i1a = i1b;
				i1b = temp;
			}

			if (i2b < i2a)
			{
				double temp = i2a;
				i2a = i2b;
				i2b = temp;
			}

			return (i1a <= i2b && i1b >= i2a);
		}
	}
}
