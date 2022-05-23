using Autodesk.AutoCAD.DatabaseServices;
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

		public static int xxx = 0;
		/** Iterates all entities of a database */
		public static void IterateAllEntities(Database database, ERecurseFlags recurseFlags, Action<Entity> processEntity)
		{
			List<Line> foundLines = new List<Line>();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForRead) as BlockTableRecord;

				Session.LogDebug("Iterating model space BTR " + modelSpaceBTR.Id);

				xxx++;

				if (xxx > 10)
				{
					//xxx = 0;
					return;
				}

				IterateAllEntities(modelSpaceBTR, recurseFlags, processEntity);
				IterateEntitiesInXrefs(database, recurseFlags, processEntity);
			}
		}

		/** Given a block table record, iterate through and run the specified actions on each entity */
		public static void IterateAllEntities(BlockTableRecord blockTableRecord, ERecurseFlags recurseFlags, Action<Entity> processEntity)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				foreach (ObjectId objectId in blockTableRecord)
				{
					Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

					BlockReference blockReference = entity as BlockReference;
					
					if (blockReference != null && recurseFlags.HasFlag(ERecurseFlags.RecurseBlocks))
					{
						BlockTableRecord blockBtr = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

						IterateAllEntities(blockBtr, recurseFlags, processEntity);
					}
					else
					{
						processEntity(entity);
					}
				}
			}
		}

		/**  */
		public static void IterateEntitiesInXrefs(Database database, ERecurseFlags recurseFlags, Action<Entity> processEntity)
		{
			using (Transaction transaction = Session.StartTransaction())
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
					IterateAllEntities(xrefBlockTableRecord, recurseFlags, processEntity);
				}
			}
		}
	}
}
