using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	internal class ModelSpaceHelper
	{
		[Flags]
		public enum ERecurseFlags
		{
			None = 0,
			RecurseXrefs = 1,
			RecurseBlocks = 2,
		}

		/** Iterates all entities of a database and runs an action on them */
		public static void IterateAllEntities(Transaction transaction, Database database, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction)
		{
			BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForRead) as BlockTableRecord;

			Session.LogDebug("Iterating model space BTR " + modelSpaceBTR.Id);

			// Gather all entities
			IterateAllEntities(transaction, modelSpaceBTR, recurseFlags, processEntityAction, Matrix3d.Identity);
		}

		/** Iterates all entities of a block table record */
		public static void IterateAllEntities(Transaction transaction, BlockTableRecord blockTableRecord, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction, Matrix3d parentTransform)
		{
			foreach (ObjectId objectId in blockTableRecord)
			{
				Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

				BlockReference blockReference = entity as BlockReference;
					
				if (blockReference != null && recurseFlags.HasFlag(ERecurseFlags.RecurseBlocks))
				{
					Matrix3d blockTransform = blockReference.BlockTransform;
					Matrix3d blockToWorld = parentTransform * blockTransform;

					BlockTableRecord blockBtr = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

					if (blockBtr.IsFromExternalReference)
					{
						Session.Log("Found an XREF: {0}", blockBtr.Name);
					}

					IterateAllEntities(transaction, blockBtr, recurseFlags, processEntityAction, blockToWorld);
				}
				else
				{
					processEntityAction(entity, parentTransform);
				}
			}
		}



		/*
		public static void IterateEntitiesInXrefs(Transaction transaction, Database database, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction)
		{
			Session.LogDebug("Iterating xrefs in database: " + database.CurrentSpaceId);
			database.ResolveXrefs(true, false);

			XrefGraph xrefGraph = database.GetHostDwgXrefGraph(false);

			GraphNode rootGraphNode = xrefGraph.RootNode;

			List<Database> xrefs = new List<Database>();

			for (int i = 0; i < rootGraphNode.NumOut; ++i)
			{
				XrefGraphNode xrefGraphNode = rootGraphNode.Out(i) as XrefGraphNode;

				if (xrefGraphNode == null)
				{
					continue;
				}

				BlockTableRecord xrefBlockTableRecord = transaction.GetObject(xrefGraphNode.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
				
				Session.LogDebug("Iterating xref: " + xrefGraphNode.Name);
				IterateAllEntities(transaction, xrefBlockTableRecord, recurseFlags, processEntityAction);

					
			}
		}
		*/
	}
}
