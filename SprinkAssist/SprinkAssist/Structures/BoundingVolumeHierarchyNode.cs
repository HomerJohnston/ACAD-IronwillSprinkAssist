using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using Color = Autodesk.AutoCAD.Colors.Color;
using SysColor = System.Drawing.Color;

namespace Ironwill.Structures
{
	internal class BoundingVolumeHierarchyNode
	{
		public Extents3d extents;

		public BoundingVolumeHierarchyNode parent = null;
		public BoundingVolumeHierarchyNode leftChild = null;
		public BoundingVolumeHierarchyNode rightChild = null;

		public int depth;
		public int nodeNumber; // for debugging only

		public List<Entity> entities; // will only contain anything in leaf nodes

		private float minEntitySize = float.MaxValue;
		private float maxEntitySize = float.MinValue;

		public BoundingVolumeHierarchyNode(BoundingVolumeHierarchy bvh)
		{
			nodeNumber = bvh.nodeCount++;
		}

		public BoundingVolumeHierarchyNode(BoundingVolumeHierarchy bvh, List<Entity> inEntities, BoundingVolumeHierarchyNode inParent, int currentDepth)
		{
			nodeNumber = bvh.nodeCount++;
			
			parent = inParent;
			depth = currentDepth;

			if (inEntities == null || inEntities.Count == 0)
			{
				throw new Exception("BVH Node constructed with null entities list or zero entities");
			}

			entities = inEntities;

			if (inEntities.Count <= bvh.leafObjectMax)
			{
				leftChild = null;
				rightChild = null;

				foreach(Entity entity in entities)
				{
					ComputeVolume(bvh.nodeAdapter);
					SplitIfNecessary(bvh.nodeAdapter);
				}
			}
			else
			{
				ComputeVolume(bvh.nodeAdapter);
				SplitNode(bvh.nodeAdapter);
				ChildRefit(bvh.nodeAdapter, false);
			}
		}

		void ComputeVolume(BoundingVolumeHierarchyNodeAdapter nodeAdapter)
		{
			AssignVolume(nodeAdapter.GetExtents(entities[0]));

			for (int i = 1; i < entities.Count; i++)
			{
				ExpandVolume(nodeAdapter, nodeAdapter.GetExtents(entities[i]));
			}
		}

		void AssignVolume(Extents3d volume)
		{
			extents = volume;
		}

		void ExpandVolume(BoundingVolumeHierarchyNodeAdapter nodeAdapter, Extents3d inExtents)
		{
			bool expanded = false;

			if (inExtents.MinPoint.X < extents.MinPoint.X || inExtents.MinPoint.Y < extents.MinPoint.Y || inExtents.MinPoint.Z < extents.MinPoint.Z
				||
				inExtents.MaxPoint.X > extents.MaxPoint.X || inExtents.MaxPoint.Y > extents.MaxPoint.Y || inExtents.MaxPoint.Z > extents.MaxPoint.Z)
			{
				expanded = true;
				extents.AddExtents(inExtents);
			}

			if (expanded && parent != null)
			{
				parent.ChildExpanded(nodeAdapter, this); // TODO should parent subscribe to child events instead, child just broadcasts? maybe that's overkill.
			}
		}

		void SplitIfNecessary(BoundingVolumeHierarchyNodeAdapter nodeAdapter)
		{
			if (entities.Count > nodeAdapter.bvh.leafObjectMax)
			{
				SplitNode(nodeAdapter);
			}
		}

		void SplitNode(BoundingVolumeHierarchyNodeAdapter nodeAdapter)
		{
			List<Entity> splitList = entities;

			foreach (Entity entity in splitList)
			{
				nodeAdapter.UnmapObject(entity);
			}

			int center;

			Axis splitAxis = PickSplitAxis();

			switch (splitAxis)
			{
				case Axis.X:
					{
						splitList.Sort(delegate (Entity ent1, Entity ent2) { return nodeAdapter.GetEntityPos(ent1).X.CompareTo(nodeAdapter.GetEntityPos(ent2).X); });
						break;
					}
				case Axis.Y:
					{
						splitList.Sort(delegate (Entity ent1, Entity ent2) { return nodeAdapter.GetEntityPos(ent1).Y.CompareTo(nodeAdapter.GetEntityPos(ent2).Y); });
						break;
					}
				case Axis.Z:
					{
						splitList.Sort(delegate (Entity ent1, Entity ent2) { return nodeAdapter.GetEntityPos(ent1).Z.CompareTo(nodeAdapter.GetEntityPos(ent2).Z); });
						break;
					}
			}

			center = splitList.Count / 2;
			
			leftChild = new BoundingVolumeHierarchyNode(nodeAdapter.bvh, splitList.GetRange(0, center), this, depth + 1);
			rightChild = new BoundingVolumeHierarchyNode(nodeAdapter.bvh, splitList.GetRange(center, splitList.Count - center), this, depth + 1);
			
			entities = null;
		}

		Axis PickSplitAxis()
		{
			double axisX = extents.MaxPoint.X - extents.MinPoint.X;
			double axisY = extents.MaxPoint.Y - extents.MinPoint.Y;
			double axisZ = extents.MaxPoint.Z - extents.MinPoint.Z;

			if (axisX > axisY && axisX > axisZ)
			{
				return Axis.X;
			}
			
			if (axisY > axisX && axisY > axisZ)
			{
				return Axis.Y;
			}

			return Axis.Z;
		}

		bool RefitVolume(BoundingVolumeHierarchyNodeAdapter nodeAdapter)
		{
			if (entities.Count == 0)
			{
				throw new NotImplementedException();
			}

			Extents3d oldExtents = extents;

			ComputeVolume(nodeAdapter);

			if (!extents.Equals(oldExtents))
			{
				if (parent != null)
				{
					parent.ChildRefit(nodeAdapter);
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		void ChildExpanded(BoundingVolumeHierarchyNodeAdapter nodeAdapter, BoundingVolumeHierarchyNode childNode)
		{
			bool expanded = false;

			Extents3d childExtents = childNode.extents;

			if (childExtents.MinPoint.X < extents.MinPoint.X || childExtents.MinPoint.Y < extents.MinPoint.Y || childExtents.MinPoint.Z < extents.MinPoint.Z
				||
				childExtents.MaxPoint.X > extents.MaxPoint.X || childExtents.MaxPoint.Y > extents.MaxPoint.Y || childExtents.MaxPoint.Z > extents.MaxPoint.Z)
			{
				expanded = true;
				extents.AddExtents(childExtents);
			}

			if (expanded && parent != null)
			{
				parent.ChildExpanded(nodeAdapter, this); // TODO should parent subscribe to child events instead, child just broadcasts? maybe that's overkill.
			}
		}

		void ChildRefit(BoundingVolumeHierarchyNodeAdapter nodeAdapter, bool recurse = true)
		{
			extents = leftChild.extents;
			extents.AddExtents(rightChild.extents);

			if (recurse && parent != null)
			{
				parent.ChildRefit(nodeAdapter); 
			}
		}

		public void GeneratePolylines(ref List<Polyline3d> polylines)
		{
			Point3d[] bottomPoints = new Point3d[4] 
			{
				new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MinPoint.Z), 
				new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, extents.MinPoint.Z), 
				new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MinPoint.Z),
				new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, extents.MinPoint.Z)
			};

			Point3d[] topPoints = new Point3d[4]
			{
				new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MaxPoint.Z),
				new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z),
				new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z),
				new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, extents.MaxPoint.Z)
			};

			Point3d[] corner1 = new Point3d[2]
			{
				new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MinPoint.Z),
				new Point3d(extents.MinPoint.X, extents.MinPoint.Y, extents.MaxPoint.Z),
			};

			Point3d[] corner2 = new Point3d[2]
			{
				new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, extents.MinPoint.Z),
				new Point3d(extents.MaxPoint.X, extents.MinPoint.Y, extents.MaxPoint.Z),
			};

			Point3d[] corner3 = new Point3d[2]
			{
				new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, extents.MinPoint.Z),
				new Point3d(extents.MinPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z),
			};

			Point3d[] corner4 = new Point3d[2]
			{
				new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MinPoint.Z),
				new Point3d(extents.MaxPoint.X, extents.MaxPoint.Y, extents.MaxPoint.Z),
			};

			List<Point3d[]> pointCollections = new List<Point3d[]>()
			{
				bottomPoints,
				topPoints,
				corner1, 
				corner2, 
				corner3, 
				corner4
			};

			int maxDepth = 20;
			double percentage = depth / maxDepth;

			foreach (Point3d[] pointCollection in pointCollections)
			{
				Point3dCollection point3dCollection = new Point3dCollection(pointCollection);
				Polyline3d polyline = new Polyline3d(Poly3dType.SimplePoly, point3dCollection, true);

				SysColor c1 = SysColor.Firebrick;
				SysColor c2 = SysColor.Teal;

				SysColor c3 = Blend(c1, c2, percentage);

				polyline.Color = Color.FromColor(c3);
				
				polylines.Add(polyline);
			}

			if (leftChild != null)
			{
				leftChild.GeneratePolylines(ref polylines);
			}

			if (rightChild != null)
			{
				rightChild.GeneratePolylines(ref polylines); 
			}
		}
		public static SysColor Blend(SysColor color, SysColor backColor, double amount)
		{
			byte r = (byte)(color.R * amount + backColor.R * (1 - amount));
			byte g = (byte)(color.G * amount + backColor.G * (1 - amount));
			byte b = (byte)(color.B * amount + backColor.B * (1 - amount));
			return SysColor.FromArgb(255, r, g, b);
		}

		public void GetEntities(ref List<Entity> foundEntities)
		{
			if (entities != null)
			{
				foundEntities.AddRange(entities);
			}

			if (leftChild != null)
			{
				leftChild.GetEntities(ref foundEntities);
			}

			if (rightChild != null)
			{
				rightChild.GetEntities(ref foundEntities);
			}
		}
	}
}
