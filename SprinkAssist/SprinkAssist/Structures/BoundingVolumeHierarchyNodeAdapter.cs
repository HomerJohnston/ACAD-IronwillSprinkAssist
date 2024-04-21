using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	internal abstract class BoundingVolumeHierarchyNodeAdapter<T> where T : Entity
	{
		Dictionary<T, BoundingVolumeHierarchyNode<T>> entityToLeafMap = new Dictionary<T, BoundingVolumeHierarchyNode<T>>();

		public BoundingVolumeHierarchy<T> bvh;

		public void AssignBVH(BoundingVolumeHierarchy<T> inBvh)
		{
			bvh = inBvh;
		}

		public abstract Point3d GetEntityPos(T entity);

		public Extents3d GetExtents(T entity)
		{
			Matrix3d scaleMatrix = Matrix3d.Scaling(1.1, GetEntityPos(entity));
			
			Extents3d extents = entity.GeometricExtents;

			// TODO make the 1000 expansion a setting
			double exp = Session.GlobalCloseToDistance();

			extents.ExpandBy(new Vector3d(+exp, +exp, +exp));
			extents.ExpandBy(new Vector3d(-exp, -exp, -exp));

			return extents;
		}

        public double GetSize(T entity)
		{
			return entity.GeometricExtents.MaxPoint.DistanceTo(entity.GeometricExtents.MinPoint);
		}

        public void MapObjectToNode(T entity, BoundingVolumeHierarchyNode<T> node)
		{
			entityToLeafMap[entity] = node;
		}

        public void UnmapObject(T entity)
		{
			entityToLeafMap.Remove(entity);
        }

        public void CheckMap(T entity) 
		{
            if (!entityToLeafMap.ContainsKey(entity))
			{
				throw new Exception("Missing map for child");
			}
        }

        public BoundingVolumeHierarchyNode<T> GetLeaf(T entity) 
		{
			return entityToLeafMap[entity];
        }
    }
}
