using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

[assembly: CommandClass(typeof(Ironwill.Testing.Commands))]

namespace Ironwill.Testing
{
	public class Commands
	{
		[CommandMethod("GEBR")]
		public void GetEveryBlockReference()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;
			Database db = doc.Database;

			string blockname = "_TitleblockDwgNo";

			Dictionary<string, int> layoutIndices = GetLayoutIndices();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);

				foreach (ObjectId blockID in blockTable)
				{
					BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockID, OpenMode.ForRead);

					if (!blockTableRecord.IsLayout && !blockTableRecord.IsAnonymous)
					{
						if (blockTableRecord.Name.ToUpper() == blockname.ToUpper())
						{
							ObjectIdCollection allBlocks = blockTableRecord.GetBlockReferenceIds(true, false);

							foreach (ObjectId blockReferenceId in allBlocks)
							{
								BlockReference blockReference = (BlockReference)transaction.GetObject(blockReferenceId, OpenMode.ForRead);

								BlockTableRecord layoutBTR = (BlockTableRecord)transaction.GetObject(blockReference.OwnerId, OpenMode.ForRead);

								if (!layoutBTR.IsLayout)
								{
									continue;
								}

								Layout layout = (Layout)transaction.GetObject(layoutBTR.LayoutId, OpenMode.ForRead);

								Session.Log($"Found {blockname} on layout {layout.LayoutName}");

								int index;

								if (layoutIndices.TryGetValue(layout.LayoutName, out index))
								{
									UpdateAttributesInOneBlock(transaction, blockReference, "X", index.ToString());
									UpdateAttributesInOneBlock(transaction, blockReference, "Y", layoutIndices.Count.ToString());
								}
							}
						}
					}
				}

				transaction.Commit();

				return;
			}
		}

		/*
		[CommandMethod("GEBR")]
		public void GetEveryBlockReference()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;
			Database db = doc.Database;

			Dictionary<string, int> layoutIndices = GetLayoutIndices();

			string blockName = "TEST";

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				var BR = SelectedEnt.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
				var BTR = BR.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;

				var AllReferences = BTR.GetBlockReferenceIds(true, false);

				foreach (ObjectId n in AllReferences)
				{
					var blockReference = n.GetObject(OpenMode.ForRead) as BlockReference;

					if (blockReference.BlockName != blockName)
					{
						continue;
					}

					var LayoutBTR = blockReference.OwnerId.GetObject(OpenMode.ForRead) as BlockTableRecord;
					if (!LayoutBTR.IsLayout) continue;
					var Layout = LayoutBTR.LayoutId.GetObject(OpenMode.ForRead) as Layout;

					if (layoutIndices.ContainsKey(Layout.LayoutName))
					{
						BlockOps.SetBlockAttribute(tr, blockReference, "X", "OMAOMSMFOSM");
					}
				}

				tr.Commit();
			}
		}
		*/

		public Dictionary<string, int> GetLayoutIndices()
		{
			Dictionary<string, int> layoutIndices = new Dictionary<string, int>();

			Database db = Session.GetDatabase();

			int index = 1;

			using (Transaction transaction = Session.StartTransaction())
			{
				DBDictionary dbDictionary = (DBDictionary)transaction.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
				
				foreach (DBDictionaryEntry layoutDictionary in dbDictionary)
				{
					Layout layout = (Layout)layoutDictionary.Value.GetObject(OpenMode.ForRead);

					if (layout.LayoutName.StartsWith("FP"))
					{
						layoutIndices.Add(layout.LayoutName, index++);
					}

					/*
					Database layoutDB = layout.Database;

					Session.Log(layout.LayoutName);

					UpdateAttributesInDatabase(layoutDB, "TEST", "ONE", "4242");
					*/
				}
				
				transaction.Commit();
			}

			return layoutIndices;
		}

		[CommandMethod("UA")]
		public void UpdateAttribute()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			// Have the user choose the block and attribute names, and the new attribute value
			PromptResult pr = ed.GetString("\nEnter name of block to search for: ");

			if (pr.Status != PromptStatus.OK)
			{
				return;
			}

			string blockName = pr.StringResult.ToUpper();

			pr = ed.GetString("\nEnter tag of attribute to update: ");

			if (pr.Status != PromptStatus.OK)
			{
				return;
			}

			string attbName = pr.StringResult.ToUpper();

			pr = ed.GetString("\nEnter new value for attribute: ");

			if (pr.Status != PromptStatus.OK)
			{
				return;
			}

			string attbValue = pr.StringResult;

			UpdateAttributesInDatabase(db, blockName, attbName, attbValue);
		}

		private void UpdateAttributesInDatabase(Database db, string blockName, string attbName, string attbValue)
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;

			// Get the IDs of the spaces we want to process and simply call a function to process each

			ObjectId msId, psId;

			Transaction tr = db.TransactionManager.StartTransaction();

			using (tr)
			{
				BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

				msId = bt[BlockTableRecord.ModelSpace];

				psId = bt[BlockTableRecord.PaperSpace];

				tr.Commit();
			}

			//int msCount = UpdateAttributesInBlock(msId, blockName, attbName, attbValue);

			int psCount = UpdateAttributesInBlock(psId, blockName, attbName, attbValue);

			ed.Regen();

			// Display the results
			ed.WriteMessage("\nProcessing file: " + db.Filename);

			ed.WriteMessage
			(
				"\nUpdated {0} instance{1} of " +
				"attribute {2} in the default paperspace.",
				psCount,
				psCount == 1 ? "" : "s",
				attbName
			);
		}

		private int UpdateAttributesInBlock(ObjectId btrId, string blockName, string attbName, string attbValue)
		{
			// Will return the number of attributes modified
			int changedCount = 0;

			Document doc = Application.DocumentManager.MdiActiveDocument;

			Database db = doc.Database;

			Editor ed = doc.Editor;

			Transaction tr = doc.TransactionManager.StartTransaction();

			using (tr)
			{
				BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);

				// Test each entity in the container...
				foreach (ObjectId entId in btr)
				{
					Entity ent = tr.GetObject(entId, OpenMode.ForRead) as Entity;

					if (ent != null)
					{
						BlockReference br = ent as BlockReference;
						
						if (br == null)
						{
							continue;
						}
						
						if (!btr.IsLayout)
						{
							continue;
						}

						Layout layout = tr.GetObject(btr.LayoutId, OpenMode.ForRead) as Layout;

						if (layout == null)
						{
							continue;
						}

						Dictionary<string, int> layoutIndices = GetLayoutIndices();

						int layoutIndex;

						if (!layoutIndices.TryGetValue(layout.LayoutName, out layoutIndex))
						{
							continue;
						}

						BlockTableRecord bd = (BlockTableRecord)tr.GetObject(br.BlockTableRecord, OpenMode.ForRead);

						// ... to see whether it's a block with the name we're after
						if (bd.Name.ToUpper() == blockName)
						{
							UpdateAttributesInOneBlock(tr, br, attbName, layoutIndex.ToString());
							changedCount++;
						}
						
						// Recurse for nested blocks
						changedCount += UpdateAttributesInBlock(br.BlockTableRecord, blockName, attbName, attbValue);
					}
				}

				tr.Commit();
			}

			return changedCount;
		}

		private void UpdateAttributesInOneBlock(Transaction tr, BlockReference br, string attbName, string attbValue)
		{
			// Check each of the attributes...
			foreach (ObjectId arId in br.AttributeCollection)
			{
				DBObject obj = tr.GetObject(arId, OpenMode.ForRead);

				AttributeReference ar = obj as AttributeReference;

				if (ar != null)
				{
					// ... to see whether it has the tag we're after
					if (ar.Tag.ToUpper() == attbName)
					{
						// If so, update the value and increment the counter
						ar.UpgradeOpen();
						ar.TextString = attbValue;
						ar.DowngradeOpen();
					}
				}
			}
		}
	}
}