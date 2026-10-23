using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ironwill.Utility.AutoCAD;

namespace Ironwill.Utility.BVH
{
	public class BoundingVolumeHierarchyManager
	{
		BoundingVolumeHierarchy<Entity> boundingVolumeHierarchy = default!;

		public BoundingVolumeHierarchyManager()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Rebuild(transaction);
			}
		}

		public void Rebuild(Transaction transaction)
		{
			List<Entity> list = new List<Entity>();

			list = Drawing.GetAllSystemEntities(transaction);

			boundingVolumeHierarchy = new BoundingVolumeHierarchy<Entity>(new BoundingVolumeHierarchyNodeAdapter_Entity(), list);
		}

		public BoundingVolumeHierarchy<Entity> Get()
		{
			return boundingVolumeHierarchy;
		}
	}
}
