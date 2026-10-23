using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public static class BlockReferenceExtensions
	{
		public enum LineTest
		{
			Anywhere,
			EndsOnly,
			AnywhereExceptEnds
		}

		public static bool IsOnLine(this BlockReference blockReference, Line line, LineTest lineTest, double tolerance = 0.01)
		{
			if (line == null)
			{
				return false;
			}

			switch (lineTest)
			{
				case LineTest.Anywhere:
				{
					Point3d closePoint = line.GetClosestPointTo(blockReference.Position, false);
					return blockReference.IsOnPoint(closePoint, tolerance);
				}
				case LineTest.EndsOnly:
				{
					return blockReference.IsOnPoint(line.StartPoint, tolerance) || blockReference.IsOnPoint(line.EndPoint, tolerance);
				}
				case LineTest.AnywhereExceptEnds:
				{
					Point3d closePoint = line.GetClosestPointTo(blockReference.Position, false);

					return blockReference.IsOnPoint(closePoint, tolerance) && !blockReference.IsOnPoint(line.StartPoint, tolerance) && !blockReference.IsOnPoint(line.EndPoint, tolerance);
				}
			}

			return false;
		}

		public static bool IsOnPoint(this BlockReference blockReference, Point3d point, double tolerance = 0.01)
		{
			return point.DistanceTo(blockReference.Position) <= tolerance;
		}
	}
}