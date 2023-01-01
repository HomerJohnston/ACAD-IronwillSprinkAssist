using System;
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

		public static BlockReference InsertBlock(string blockName)
		{
			return InsertBlock("", blockName);
		}

		public static BlockReference InsertBlock(string blockPath, string blockName)
		{
			var doc = AcApplication.DocumentManager.MdiActiveDocument;
			var db = doc.Database;
			
			using (var tr = db.TransactionManager.StartTransaction())
			{
				var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForWrite);
				ObjectId btrId = bt.Has(blockName) ? bt[blockName] : ImportBlock(db, blockName, blockPath);
				if (btrId.IsNull)
				{
					Session.Log(Environment.NewLine + $"Block '{blockName}' not found.");
					tr.Abort();
					return null;
				}

				var cSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
				var br = new BlockReference(Point3d.Origin, btrId);
				cSpace.AppendEntity(br);
				tr.AddNewlyCreatedDBObject(br, true);

				// add attribute references to the block reference
				var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForWrite);
				var attInfos = new Dictionary<string, TextInfo>();
				if (btr.HasAttributeDefinitions)
				{
					foreach (ObjectId id in btr)
					{
						if (id.ObjectClass.DxfName == "ATTDEF")
						{
							var attDef = (AttributeDefinition)tr.GetObject(id, OpenMode.ForWrite);
							attInfos[attDef.Tag] = new TextInfo(
								attDef.Position,
								attDef.AlignmentPoint,
								attDef.Justify != AttachmentPoint.BaseLeft,
								attDef.Rotation);
							var attRef = new AttributeReference();
							attRef.SetAttributeFromBlock(attDef, br.BlockTransform);
							attRef.TextString = attDef.TextString;

							br.AttributeCollection.AppendAttribute(attRef);
							tr.AddNewlyCreatedDBObject(attRef, true);
						}
					}
				}

				tr.Commit();

				return br;
			}
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
				return;

			AttributeCollection attributeCollection = blockReference.AttributeCollection;

			foreach (ObjectId attObjectId in attributeCollection)
			{
				AttributeReference attributeReference = transaction.GetObject(attObjectId, OpenMode.ForWrite, false) as AttributeReference;

				if (attributeText.ContainsKey(attributeReference.Tag))
				{
					attributeReference.UpgradeOpen();
					attributeReference.TextString = attributeText[attributeReference.Tag];
					attributeReference.TransformBy(blockReference.BlockTransform);
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

			BlockReference newBlock = BlockOps.InsertBlock(newBlockName);

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
			Session.Log("Recreating block " + blockName);

			BlockReference newBlock = InsertBlock(blockName);

			if (newBlock == null)
			{
				Session.Log("An error occurred - failed to create new block!");
				transaction.Abort();
				return;
			}

			BlockReference oldBlock = transaction.GetObject(blockIDToRecreate, OpenMode.ForWrite, false, true) as BlockReference;

			Session.Log("Found old block " + oldBlock.Name);

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
			targetBlock.Rotation = sourceBlock.Rotation;

			targetBlock.ScaleFactors = sourceBlock.ScaleFactors;

			targetBlock.Layer = sourceBlock.Layer;
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

						Session.Log("Copying " + oldProp.PropertyName + " to " + newProp.PropertyName);

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
	}
}
