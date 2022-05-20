using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public static class LineExtensions
	{
		public static void ExtendBy(this Line line, double startExtend, double endExtend)
		{
			Point3d p1 = line.StartPoint;
			Point3d p2 = line.EndPoint;

			Vector3d dir = (p2 - p1).GetNormal();

			if (Math.Abs(startExtend) > 0.0)
			{
				p1 = p1 - startExtend * dir;
				line.StartPoint = p1;
			}

			if (Math.Abs(endExtend) > 0.0)
			{
				p2 = p2 + endExtend * dir;
				line.EndPoint = p2;
			}
		}
	}
}
