using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	delegate bool NodeTest(Extents3d extents);

	internal class BoundingVolumeHierarchy<T> where T : Entity
	{
		BoundingVolumeHierarchyNode<T> rootNode;

		public BoundingVolumeHierarchyNodeAdapter<T> nodeAdapter;

		public int leafObjectMax = 1;

		public int nodeCount = 0;

		public int maxDepth = 0;

		public BoundingVolumeHierarchy(BoundingVolumeHierarchyNodeAdapter<T> inNodeAdapter, List<T> entities = null, int leafObjectMax = 1)
		{
			this.leafObjectMax = leafObjectMax;

			nodeAdapter = inNodeAdapter;
			nodeAdapter.AssignBVH(this);

			if (entities ==  null || entities.Count == 0)
			{
				rootNode = new BoundingVolumeHierarchyNode<T>(this);
				rootNode.entities = new List<T>();
			}
			else
			{
				rootNode = new BoundingVolumeHierarchyNode<T>(this, entities, null, 0);
			}
		}

		public List<Polyline3d> GeneratePolylines()
		{
			List<Polyline3d> polylines = new List<Polyline3d>();

			rootNode.GeneratePolylines(ref polylines, maxDepth);

			return polylines;
		}

		private void TraverseInternal(BoundingVolumeHierarchyNode<T> currentNode, NodeTest hitTest, ref List<BoundingVolumeHierarchyNode<T>> hitList)
		{
			if (currentNode == null)
			{
				return; 
			}

			if (hitTest(currentNode.extents))
			{
				hitList.Add(currentNode);

				TraverseInternal(currentNode.leftChild, hitTest, ref hitList);
				TraverseInternal(currentNode.rightChild, hitTest, ref hitList);
			}
		}

		private List<BoundingVolumeHierarchyNode<T>> Traverse(NodeTest hitTest)
		{
			List<BoundingVolumeHierarchyNode<T>> hits = new List<BoundingVolumeHierarchyNode<T>>();

			TraverseInternal(rootNode, hitTest, ref hits);

			List<BoundingVolumeHierarchyNode<T>> populatedHits = new List<BoundingVolumeHierarchyNode<T>>();

			foreach (BoundingVolumeHierarchyNode<T> node in hits)
			{
				if (node.entities != null && node.entities.Count > 0)
				{
					populatedHits.Add(node);
				}
			}

			return populatedHits;
		}

		public List<T> FindEntities(Point3d point)
		{
			List<BoundingVolumeHierarchyNode<T>> nodes = Traverse(nodeExtents => IFEMath.Intersection.Extents3dContainsPoint3d(nodeExtents, point));
			return RetrieveEntities(nodes);
		}

		public List<T> FindEntities(Extents3d box)
		{
			List<BoundingVolumeHierarchyNode<T>> nodes = Traverse(nodeExtents => IFEMath.Intersection.Extents3dCollidesWithExtents3d(nodeExtents, box));
			return RetrieveEntities(nodes);
		}

		private List<T> RetrieveEntities(List<BoundingVolumeHierarchyNode<T>> nodes)
		{
			List<T> foundEntities = new List<T>();

			foreach (BoundingVolumeHierarchyNode<T> node in nodes)
			{
				node.GetEntities(ref foundEntities);
			}

			return foundEntities;
		}
	}
}
