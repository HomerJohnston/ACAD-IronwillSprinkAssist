using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	internal class BoundingVolumeHierarchyNodeAdapter_BlockReference : BoundingVolumeHierarchyNodeAdapter<BlockReference>
	{
		public override Point3d GetEntityPos(BlockReference blockReference)
		{
			return blockReference.Position;
		}
	}
}
