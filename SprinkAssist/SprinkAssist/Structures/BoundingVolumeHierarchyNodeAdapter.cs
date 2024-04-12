using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	internal class BoundingVolumeHierarchyNodeAdapter
	{
		Dictionary<Entity, BoundingVolumeHierarchyNode> entityToLeafMap = new Dictionary<Entity, BoundingVolumeHierarchyNode>();



		public BoundingVolumeHierarchy bvh;

		public BoundingVolumeHierarchyNodeAdapter(BoundingVolumeHierarchy inBvh)
		{
			bvh = inBvh;
		}

        public Point3d GetEntityPos(Entity entity)
		{
			switch (entity)
			{
				case Line line:
				{
					Point3d p1 = line.StartPoint;
					Point3d p2 = line.EndPoint;

					return new Point3d(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z) * 0.5;
				}
				case Circle circle:
				{
					return circle.Center;
				}
			}

			return new Point3d(0, 0, 0);
		}

		public Extents3d GetExtents(Entity entity)
		{
			Matrix3d scaleMatrix = Matrix3d.Scaling(1.1, GetEntityPos(entity));
			
			Extents3d extents = entity.GeometricExtents;
			
			//extents.TransformBy(scaleMatrix);
			extents.ExpandBy(new Vector3d(+500.0 * Session.AutoScaleFactor(), +500.0 * Session.AutoScaleFactor(), +500.0 * Session.AutoScaleFactor()));
			extents.ExpandBy(new Vector3d(-500.0 * Session.AutoScaleFactor(), -500.0 * Session.AutoScaleFactor(), -500.0 * Session.AutoScaleFactor()));

			return extents;
		}

        public double GetSize(Entity entity)
		{
			return entity.GeometricExtents.MaxPoint.DistanceTo(entity.GeometricExtents.MinPoint);
		}

        public void MapObjectToNode(Entity entity, BoundingVolumeHierarchyNode node)
		{
			entityToLeafMap[entity] = node;
		}

        public void UnmapObject(Entity entity)
		{
			entityToLeafMap.Remove(entity);
        }

        public void CheckMap(Entity entity) 
		{
            if (!entityToLeafMap.ContainsKey(entity))
			{
				throw new Exception("Missing map for child");
			}
        }

        public BoundingVolumeHierarchyNode GetLeaf(Entity entity) 
		{
			return entityToLeafMap[entity];
        }
    }
}
