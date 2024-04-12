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
		public static bool Extents3dContainsPoint3d(Extents3d extents, Point3d point)
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
	}
}
