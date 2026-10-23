using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Ironwill.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands
{
	internal class StretchingLine
	{
		Line draggedLine;
		Line draggedLineJig;

		Line line;
		Line attachedLineJig;

		Vector3d lineDir;

		double dragPoint;

		List<PipeConnection> connections = new List<PipeConnection>();

		public StretchingLine(Line inAttachedLine, Line inDraggedLine, Line inDraggedLineJig)
		{
			draggedLine = inDraggedLine;
			draggedLineJig = inDraggedLineJig;

			line = inAttachedLine;

			attachedLineJig = BuildJiggedLine();

			lineDir = (line.EndPoint - line.StartPoint).GetNormal();

			BuildConnectionsList();
		}

		private Line BuildJiggedLine()
		{
			Line line = new Line();

			return line;
		}

		private void BuildConnectionsList()
		{
			List<Entity> nearbyEntities = BoundingVolumeHierarchyManager.Get().FindEntities(line.GeometricExtents);

			if (!line.StartPoint.IsOnLine(draggedLine))
			{
				connections.Add(new PipeConnection(line, null, null, 0));
			}

			foreach (Entity nearbyEntity in nearbyEntities)
			{
				if (nearbyEntity == draggedLine || nearbyEntity == line)
				{
					continue;
				}

				switch (nearbyEntity)
				{
					case BlockReference nearbyBlockReference:
					{
						GetAnchorPoints(nearbyBlockReference);
						break;
					}
					case Line nearbyLine:
					{
						GetAnchorPoints(nearbyLine);
						break;
					}
				}
			}

			if (!line.EndPoint.IsOnLine(draggedLine))
			{
				connections.Add(new PipeConnection(line, null, null, 0));
			}

			connections.Sort();

			if (connections.Count == 0)
			{
				Session.Log("No anchor points!");
			}
		}

		private void GetAnchorPoints(BlockReference nearbyBlockReference)
		{
			if (nearbyBlockReference.Layer == Layer.SystemHead)
			{
				if (!nearbyBlockReference.IsOnLine(line, BlockReferenceExtensions.LineTest.Anywhere))
				{
					AddAnchorPoint(nearbyBlockReference);
				}
			}
			else if (nearbyBlockReference.Layer == Layer.SystemFitting || Layer.PipeLayers.Contains(nearbyBlockReference.Layer))
			{
				if (nearbyBlockReference.IsOnLine(line, BlockReferenceExtensions.LineTest.Anywhere) && !nearbyBlockReference.IsOnLine(draggedLine, BlockReferenceExtensions.LineTest.Anywhere))
				{
					AddAnchorPoint(nearbyBlockReference);
				}
			}
		}

		void AddAnchorPoint(BlockReference blockReference)
		{
			double dot = (blockReference.Position - line.StartPoint).DotProduct(lineDir);

			connections.Add(new PipeConnection(line, null, blockReference, dot));
			
			Session.LogDebug("Adding anchor point (1) " + dot);
		}

		private void GetAnchorPoints(Line nearbyLine)
		{
			if (nearbyLine.Layer == Layer.SystemFitting || Layer.PipeLayers.Contains(nearbyLine.Layer))
			{
				if (nearbyLine.StartPoint.IsOnLine(line))
				{
					double dot = (nearbyLine.StartPoint - line.StartPoint).DotProduct(lineDir);

					connections.Add(new PipeConnection(line, null, null, dot));

					Session.LogDebug("Adding anchor point (2) " + dot);
				}
				else if (nearbyLine.EndPoint.IsOnLine(line))
				{
					double dot = (nearbyLine.EndPoint - line.StartPoint).DotProduct(lineDir);

					connections.Add(new PipeConnection(line, null, null, dot));

					Session.LogDebug("Adding anchor point (3) " + dot);
				}
			}
		}
		
		public void Update(WorldDraw drawer)
		{
			if (connections.Count == 0)
			{
				return;
			}

			Point3dCollection intersections = new Point3dCollection();

			draggedLineJig.IntersectWith(line, Intersect.ExtendBoth, intersections, new IntPtr(0), new IntPtr(0));

			if (intersections.Count != 1)
			{
				Session.Log("ERROR: no intersections found. Lines are parallel or on different Z planes?");
			}

			dragPoint = (intersections[0] - line.StartPoint).DotProduct(lineDir);

			double min = Math.Min(connections.First().anchorPoint, dragPoint);
			double max = Math.Max(connections.Last().anchorPoint, dragPoint);

			if (min > max)
			{
				double temp = min;
				min = max;
				max = temp;
			}

			Point3d newStartPoint = line.StartPoint + min * lineDir;
			Point3d newEndPoint = line.StartPoint + max * lineDir;

			attachedLineJig.StartPoint = newStartPoint;
			attachedLineJig.EndPoint = newEndPoint;

			attachedLineJig.Draw(drawer);
		}

		public void Apply()
		{
			double min = Math.Min(connections.First().anchorPoint, dragPoint);
			double max = Math.Max(connections.Last().anchorPoint, dragPoint);

			if (min > max)
			{
				double temp = min;
				min = max;
				max = temp;
			}

			if (Math.Abs(max - min) <= 0.01 * Session.UnitsScaleFactor())
			{
				line.UpgradeOpen();

				line.Erase();
			}
			else
			{
				Point3d newStartPoint = line.StartPoint + min * lineDir;
				Point3d newEndPoint = line.StartPoint + max * lineDir;

				line.UpgradeOpen();

				line.StartPoint = newStartPoint;
				line.EndPoint = newEndPoint;
			}

			foreach (PipeConnection connection in connections)
			{
				if (connection.anchorPoint < min || connection.anchorPoint > max)
				{
					if (connection.fitting != null)
					{
						connection.fitting.UpgradeOpen();

						connection.fitting.Erase();
					}

					continue;
				}
			}
		}

		public void Destroy()
		{
			attachedLineJig.Dispose();
		}
	}
}
