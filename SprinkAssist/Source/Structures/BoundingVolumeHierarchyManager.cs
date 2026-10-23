using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Structures
{
	public class BoundingVolumeHierarchyManager
	{
		static BoundingVolumeHierarchy<Entity> boundingVolumeHierarchy;

		public BoundingVolumeHierarchyManager()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Rebuild(transaction);
			}
		}

		public static void Rebuild(Transaction transaction)
		{
			List<Entity> list = new List<Entity>();

			list = Drawing.GetAllSystemEntities(transaction);

			boundingVolumeHierarchy = new BoundingVolumeHierarchy<Entity>(new BoundingVolumeHierarchyNodeAdapter_Entity(), list);
		}

		public static BoundingVolumeHierarchy<Entity> Get()
		{
			return boundingVolumeHierarchy;
		}
	}
}
