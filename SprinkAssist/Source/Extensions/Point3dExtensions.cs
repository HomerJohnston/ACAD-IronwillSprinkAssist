using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public static class PointExtensions
	{
		public static bool IsOnLine(this Point3d point, Line line, double tolerance = 0.01)
		{
			Point3d closePoint = line.GetClosestPointTo(point, false);

			return closePoint.DistanceTo(point) <= tolerance;
		}
	}
}
