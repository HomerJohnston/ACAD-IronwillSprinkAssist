using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Ironwill.Structures;
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

		public static List<Point3d> FindAttachPoints(this Line line)
		{
			List<Point3d> points = new List<Point3d>();

			List<Entity> nearbyEntities = BoundingVolumeHierarchyManager.Get().FindEntities(line.GeometricExtents);

			foreach (Entity entity in nearbyEntities)
			{
				Line nearbyLine = entity as Line;

				if (nearbyLine != null && Layer.PipeLayers.Contains(nearbyLine.Layer))
				{
					Point3d closePoint1 = line.GetClosestPointTo(nearbyLine.StartPoint, false);
					Point3d closePoint2 = line.GetClosestPointTo(nearbyLine.EndPoint, false);

					if (closePoint1.DistanceTo(nearbyLine.StartPoint) <=  0.01 * Session.UnitsScaleFactor())
					{
						points.Add(closePoint1);
					}

					if (closePoint2.DistanceTo(nearbyLine.EndPoint) <= 0.01 * Session.UnitsScaleFactor())
					{
						points.Add(closePoint2);
					}

					continue;
				}

				BlockReference nearbyFitting = entity as BlockReference;

				if (nearbyFitting != null && nearbyFitting.Layer == Layer.SystemFitting)
				{
					Point3d closePoint = line.GetClosestPointTo(nearbyFitting.Position, false);

					if (closePoint.DistanceTo(nearbyFitting.Position) <= 0.01 * Session.UnitsScaleFactor())
					{
						points.Add(closePoint);
					}
				}
			}

			return points;
		}

		public static bool IsAttachedTo(this Line line, Line otherLine)
		{
			return line.StartPoint.IsOnLine(otherLine) || line.EndPoint.IsOnLine(otherLine);
		}
	}
}
