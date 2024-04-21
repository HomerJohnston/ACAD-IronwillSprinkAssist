using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	internal class BoundingVolumeHierarchyNodeAdapter_Line : BoundingVolumeHierarchyNodeAdapter<Line>
	{
		public override Point3d GetEntityPos(Line line)
		{
			Point3d p1 = line.StartPoint;
			Point3d p2 = line.EndPoint;

			return new Point3d(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z) * 0.5;
		}
    }
}
