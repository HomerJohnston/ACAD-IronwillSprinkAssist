﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill
{
	public class BlockOps
	{
		struct TextInfo
		{
			public Point3d Position { get; private set; }
			public Point3d Alignment { get; private set; }
			public bool IsAligned { get; private set; }
			public double Rotation { get; private set; }
			public TextInfo(Point3d position, Point3d alignment, bool aligned, double rotation)
			{
				Position = position;
				Alignment = alignment;
				IsAligned = aligned;
				Rotation = rotation;
			}
		}

		// I need to pass in a transaction for this because an Entity var becomes invalid for use outside of the transaction used to create the entity.
		public static BlockReference InsertBlock(Transaction transaction, string blockName)
		{
			return InsertBlock(transaction, "", blockName);
		}

		public static BlockReference InsertBlock(Transaction transaction, string blockPath, string blockName)
		{
			var database = Session.GetDatabase();
			
			var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
			ObjectId btrId = blockTable.Has(blockName) ? blockTable[blockName] : ImportBlock(database, blockName, blockPath);
			if (btrId.IsNull)
			{
				Session.Log(Environment.NewLine + $"Block '{blockName}' not found.");
				transaction.Abort();
				return null;
			}

			var cSpace = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);
			var br = new BlockReference(Point3d.Origin, btrId);
			cSpace.AppendEntity(br);
			transaction.AddNewlyCreatedDBObject(br, true);

			// add attribute references to the block reference
			var btr = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForWrite);
			var attInfos = new Dictionary<string, TextInfo>();
			if (btr.HasAttributeDefinitions)
			{
				foreach (ObjectId id in btr)
				{
					if (id.ObjectClass.DxfName == "ATTDEF")
					{
						var attDef = (AttributeDefinition)transaction.GetObject(id, OpenMode.ForWrite);
						attInfos[attDef.Tag] = new TextInfo(
							attDef.Position,
							attDef.AlignmentPoint,
							attDef.Justify != AttachmentPoint.BaseLeft,
							attDef.Rotation);
						var attRef = new AttributeReference();
						attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
						attRef.TextString = attDef.TextString;

						br.AttributeCollection.AppendAttribute(attRef);
						transaction.AddNewlyCreatedDBObject(attRef, true);
					}
				}
			}

			return br;
		}

		private static ObjectId ImportBlock(Database destDb, string blockName, string sourceFileName)
		{
			if (System.IO.File.Exists(sourceFileName))
			{
				using (var sourceDb = new Database(false, true))
				{
					try
					{
						// Read the DWG into a side database
						sourceDb.ReadDwgFile(sourceFileName, FileOpenMode.OpenForReadAndAllShare, true, "");

						// Create a variable to store the block identifier
						var id = ObjectId.Null;
						using (var tr = new OpenCloseTransaction())
						{
							// Open the block table
							var bt = (BlockTable)tr.GetObject(sourceDb.BlockTableId, OpenMode.ForRead, false);

							// if the block table contains 'blockName', store it into the variable
							if (bt.Has(blockName))
								id = bt[blockName];
						}
						// if the variable is not null (i.e. the block was found)
						if (!id.IsNull)
						{
							// Copy the block definition from source to destination database
							var blockIds = new ObjectIdCollection();
							blockIds.Add(id);
							var mapping = new IdMapping();
							sourceDb.WblockCloneObjects(blockIds, destDb.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
							// if the copy succeeded, return the ObjectId of the clone
							if (mapping[id].IsCloned)
								return mapping[id].Value;
						}
					}
					catch (Autodesk.AutoCAD.Runtime.Exception ex)
					{
						Session.Log("\nError during copy: " + ex.Message + "\n" + ex.StackTrace);
					}
				}
			}

			return ObjectId.Null;
		}

		public static void SetBlockAttributes(Transaction transaction, BlockReference blockReference, Dictionary<string, string> attributeText)
		{
			if (blockReference == null)
			{
				return;
			}

			AttributeCollection attributeCollection = blockReference.AttributeCollection;

			foreach (ObjectId attObjectId in attributeCollection)
			{
				AttributeReference attributeReference = transaction.GetObject(attObjectId, OpenMode.ForWrite, false) as AttributeReference;

				string text;

				if (attributeText.TryGetValue(attributeReference.Tag, out text))
				{
					attributeReference.UpgradeOpen();
					attributeReference.TextString = text;
					attributeReference.TransformBy(blockReference.BlockTransform); // TODO: Does this force the block to update? Do I need this?
				}
			}
		}

		public static void SetBlockAttribute(Transaction transaction, BlockReference blockReference, string attribute, string text)
		{
			if (blockReference == null)
			{
				return;
			}

			AttributeCollection attributeCollection = blockReference.AttributeCollection;

			foreach (ObjectId attObjectId in attributeCollection)
			{
				AttributeReference attributeReference = transaction.GetObject(attObjectId, OpenMode.ForWrite, false) as AttributeReference;

				if (attribute == attributeReference.Tag)
				{
					attributeReference.UpgradeOpen();
					attributeReference.TextString = text;
					//attributeReference.TransformBy(blockReference.BlockTransform); // TODO: why did i have this??? Does this force the block to update? Do I need this?
					attributeReference.DowngradeOpen();
				}
			}
		}

		// TODO add layer as a parameter, rename to PickBlock
		// TODO see if there is a simpler existing picker in Editor I can use?
		public static BlockReference PickSprinkler(Transaction transaction, string prompt)
		{
			if (!prompt.StartsWith(Environment.NewLine))
			{
				prompt = Environment.NewLine + prompt;
			}

			BlockReference selectedBlock = null;

			while (selectedBlock == null)
			{
				PromptEntityOptions promptEntityOptions = new PromptEntityOptions(prompt);

				PromptEntityResult promptEntityResult = Session.GetEditor().GetEntity(promptEntityOptions);

				if (promptEntityResult.Status != PromptStatus.OK)
				{
					return null;
				}

				ObjectId objectId = promptEntityResult.ObjectId;

				selectedBlock = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

				if (selectedBlock.Layer != Layer.SystemHead.Get())
				{
					Session.Log("You must pick a block on the sprinkler system heads layer!");
					selectedBlock = null;
				}
			}

			return selectedBlock;
		}

		public static void CopyBlock(Transaction transaction, ObjectId sourceBlockId, Point3d newPosition)
		{
			var watch = System.Diagnostics.Stopwatch.StartNew();
			BlockReference sourceBlock = transaction.GetObject(sourceBlockId, OpenMode.ForWrite) as BlockReference;

			string newBlockName = BlockOps.GetDynamicBlockName(sourceBlock);

			BlockReference newBlock = BlockOps.InsertBlock(transaction, newBlockName);

			if (newBlock == null)
			{
				return;
			}

			// Must set scale etc. of new head BEFORE applying dynamic properties or size will be different
			newBlock.Position = newPosition;
			newBlock.Rotation = sourceBlock.Rotation;
			newBlock.ScaleFactors = sourceBlock.ScaleFactors;
			newBlock.Layer = sourceBlock.Layer;

			CopyDynamicBlockProperties(sourceBlock, newBlock);

			watch.Stop(); var elapsedMs = watch.ElapsedMilliseconds; Session.Log("CopyBlock took " + elapsedMs.ToString());
		}

		public static void RecreateBlock(Transaction transaction, string blockName, ObjectId blockIDToRecreate, bool copyDynamicProperties = true)
		{
			//Session.Log("Recreating block " + blockName);

			BlockReference newBlock = InsertBlock(transaction, blockName);

			if (newBlock == null)
			{
				Session.Log("An error occurred - failed to create new block!");
				transaction.Abort();
				return;
			}

			BlockReference oldBlock = transaction.GetObject(blockIDToRecreate, OpenMode.ForWrite, false, true) as BlockReference;

			//Session.Log("Found old block " + oldBlock.Name);

			// Must set scale etc. of new head BEFORE applying dynamic properties or size will be different
			newBlock.Position = oldBlock.Position;
			newBlock.Rotation = oldBlock.Rotation;
			newBlock.ScaleFactors = oldBlock.ScaleFactors;
			newBlock.Layer = oldBlock.Layer;

			if (copyDynamicProperties)
			{
				CopyDynamicBlockProperties(oldBlock, newBlock);
			}

			oldBlock.Erase();
		}

		public static string GetDynamicBlockName(BlockReference block)
		{
			BlockTableRecord originalBlock = block.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
			return originalBlock.Name;
		}

		public static void CopyCommonProperties(BlockReference sourceBlock, BlockReference targetBlock)
		{
			targetBlock.UpgradeOpen();

			targetBlock.Rotation = sourceBlock.Rotation;

			targetBlock.ScaleFactors = sourceBlock.ScaleFactors;

			targetBlock.Layer = sourceBlock.Layer;

			targetBlock.DowngradeOpen();
		}

		public static void CopyDynamicBlockProperties(BlockReference sourceBlock, BlockReference targetBlock)
		{
			DynamicBlockReferencePropertyCollection dynBlockProperties = sourceBlock.DynamicBlockReferencePropertyCollection;
			DynamicBlockReferencePropertyCollection newBlockProperties = targetBlock.DynamicBlockReferencePropertyCollection;

			foreach (DynamicBlockReferenceProperty oldProp in dynBlockProperties)
			{
				foreach (DynamicBlockReferenceProperty newProp in newBlockProperties)
				{
					if (oldProp.PropertyName == newProp.PropertyName)
					{
						if (newProp.ReadOnly)
						{
							continue;
						}

						//Session.Log("Copying " + oldProp.PropertyName + " to " + newProp.PropertyName);

						try
						{
							newProp.Value = oldProp.Value;
						}
						catch
						{
							Session.Log("Warning: failed to copy dynamic block property " + newProp.PropertyName);
						}
					}
				}
			}
		}

		public void GetEveryBlockReference(string blockName)
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;
			Database db = doc.Database;

			var SelectedEntOpt = new PromptEntityOptions("\nSlect a BlockReference.");
			SelectedEntOpt.SetRejectMessage("\nMust be a BlockReference");
			SelectedEntOpt.AddAllowedClass(typeof(BlockReference), true);

			var SelectedEnt = ed.GetEntity(SelectedEntOpt);
			if (SelectedEnt.Status != PromptStatus.OK) return;

			var ListofLayoutAndBlockReference = new List<string[]>();

			string BLockName = "";

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				var BR = SelectedEnt.ObjectId.GetObject(OpenMode.ForRead) as BlockReference;
				var BTR = BR.BlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;

				BLockName = BTR.Name;

				var AllReferences = BTR.GetBlockReferenceIds(true, false);

				foreach (ObjectId n in AllReferences)
				{
					var BlockReference = n.GetObject(OpenMode.ForRead) as BlockReference;
					var LayoutBTR = BlockReference.OwnerId.GetObject(OpenMode.ForRead) as BlockTableRecord;
					if (!LayoutBTR.IsLayout) continue;
					var Layout = LayoutBTR.LayoutId.GetObject(OpenMode.ForRead) as Layout;



					ListofLayoutAndBlockReference.Add(new string[2] { Layout.LayoutName, BlockReference.ObjectId.ToString() });
				}

				tr.Commit();
			}
			ed.WriteMessage("\nFound " + ListofLayoutAndBlockReference.Count + " references of selected Block (" + BLockName + ").");
			foreach (var n in ListofLayoutAndBlockReference)
			{
				ed.WriteMessage("\nLayout named " + n[0] + " contanins " + n[1] + ".");
			}
		}
	}
}
