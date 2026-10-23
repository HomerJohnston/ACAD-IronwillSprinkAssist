using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Ironwill.Utility.AutoCAD;

namespace Ironwill.Utility.BVH
{
	internal class BoundingVolumeHierarchyNodeAdapter_Entity : BoundingVolumeHierarchyNodeAdapter<Entity>
	{
		public override Point3d GetEntityPos(Entity entity)
		{
			switch (entity)
			{
				case Line line:
					{
						Point3d p1 = line.StartPoint;
						Point3d p2 = line.EndPoint;

						return new Point3d(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z) * 0.5;
					}
				case BlockReference blockReference:
					{
						return blockReference.Position;
					}
				default:
					{
						// lol fucking autocad is there an easier way to do this?
						Session.LogDebug("BoundingVolumeHierarchyNodeAdapter_Entity using default implementation, should this be occurring?");
						return Point3d.Origin.Add((0.5 * (entity.GeometricExtents.MinPoint.GetAsVector() + entity.GeometricExtents.MaxPoint.GetAsVector())));
					}
			}
		}
	}
}
