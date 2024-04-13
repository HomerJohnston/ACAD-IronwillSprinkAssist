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

	internal class BoundingVolumeHierarchy
	{
		BoundingVolumeHierarchyNode rootNode;

		public BoundingVolumeHierarchyNodeAdapter nodeAdapter;

		public int leafObjectMax = 1;

		public int nodeCount = 0;

		public BoundingVolumeHierarchy(List<Entity> entities = null, int leafObjectMax = 1)
		{
			this.leafObjectMax = leafObjectMax;

			nodeAdapter = new BoundingVolumeHierarchyNodeAdapter(this);

			if (entities ==  null || entities.Count == 0)
			{
				rootNode = new BoundingVolumeHierarchyNode(this);
				rootNode.entities = new List<Entity>();
			}
			else
			{
				rootNode = new BoundingVolumeHierarchyNode(this, entities, null, 0);
			}
		}

		public List<Polyline3d> GeneratePolylines()
		{
			List<Polyline3d> polylines = new List<Polyline3d>();

			rootNode.GeneratePolylines(ref polylines);

			return polylines;
		}

		private void TraverseInternal(BoundingVolumeHierarchyNode currentNode, NodeTest hitTest, ref List<BoundingVolumeHierarchyNode> hitList)
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

		public List<BoundingVolumeHierarchyNode> Traverse(NodeTest hitTest)
		{
			List<BoundingVolumeHierarchyNode> hits = new List<BoundingVolumeHierarchyNode>();

			TraverseInternal(rootNode, hitTest, ref hits);

			List<BoundingVolumeHierarchyNode> populatedHits = new List<BoundingVolumeHierarchyNode>();

			foreach (BoundingVolumeHierarchyNode node in hits)
			{
				if (node.entities != null && node.entities.Count > 0)
				{
					populatedHits.Add(node);
				}
			}

			return populatedHits;
		}

		public List<BoundingVolumeHierarchyNode> Traverse(Point3d point)
		{
			return Traverse(box => IFEMath.Intersection.Extents3dContainsPoint3d(box, point));
		}

		public List<Entity> FindEntities(Point3d point)
		{
			List<BoundingVolumeHierarchyNode> nodes = Traverse(point);

			List<Entity> foundEntities = new List<Entity>();

			foreach (BoundingVolumeHierarchyNode node in nodes)
			{
				node.GetEntities(ref foundEntities);
			}

			return foundEntities;
		}
	}
}
