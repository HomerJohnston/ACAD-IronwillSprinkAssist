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

[assembly: CommandClass(typeof(Ironwill.ReplaceHeads))]

namespace Ironwill
{
	public class ReplaceHeads
	{
		[CommandMethod("SpkAssist_ReplaceHeads")]
		public void ReplaceHeadsCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				BlockReference headToReplace = PickHead(transaction, "Pick the type of head you want to replace");

				if (headToReplace == null)
				{
					Session.WriteMessage("No head selected, aborting");
					return;
				}

				string replacedHeadName = GetHeadBlockName(headToReplace);

				List<string> dynamicNames = GetAllBlockNames(transaction, replacedHeadName);

				SelectionFilter selectionFilter = new SelectionFilter(CreateFilterListForBlocks(dynamicNames));
				SelectionSet headsToReplace = PickHeadsToReplace(selectionFilter);

				if (headsToReplace == null || headsToReplace.Count == 0)
				{
					Session.WriteMessage("No objects selected, aborting");
					return;
				}

				BlockReference replacementHeadBlock = PickHead(transaction, "Pick the new head to replace selected heads with");

				if (replacementHeadBlock == null)
				{
					Session.WriteMessage("No head selected, aborting");
					return;
				}

				string newHeadName = GetHeadBlockName(replacementHeadBlock);

				for (int i = 0; i < headsToReplace.Count; i++)
				{
					BlockReference newHead = BlockDictionary.InsertBlock(newHeadName);

					if (newHead == null)
					{
						return;
					}

					ObjectId objectId = headsToReplace[i].ObjectId;

					BlockReference oldHead = transaction.GetObject(objectId, OpenMode.ForWrite) as BlockReference;

					// Must set scale etc. of new head BEFORE applying dynamic properties or size will be different
					newHead.Position = oldHead.Position;
					newHead.Rotation = oldHead.Rotation;
					newHead.ScaleFactors = oldHead.ScaleFactors;
					newHead.Layer = oldHead.Layer;

					DynamicBlockReferencePropertyCollection dynBlockProperties = oldHead.DynamicBlockReferencePropertyCollection;
					DynamicBlockReferencePropertyCollection newBlockProperties = newHead.DynamicBlockReferencePropertyCollection;

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

								newProp.Value = oldProp.Value;
							}
						}
					}


					oldHead.Erase();
				}

				transaction.Commit();
			}
		}

		private BlockReference PickHead(Transaction transaction, string prompt)
		{
			TypedValue[] filter =
			{
				new TypedValue((int)DxfCode.Operator, "<or"),
				new TypedValue((int)DxfCode.LayerName, "SpkSystem_Head"),
				new TypedValue((int)DxfCode.Operator, "or>"),
			};

			PromptEntityOptions promptEntityOptions = new PromptEntityOptions(prompt);
			PromptEntityResult promptEntityResult = Session.GetEditor().GetEntity(promptEntityOptions);

			if (promptEntityResult.Status != PromptStatus.OK)
			{
				return null;
			}

			ObjectId objectId = promptEntityResult.ObjectId;
			return transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;
		}

		private SelectionSet PickHeadsToReplace(SelectionFilter selectionFilter)
		{
			PromptSelectionOptions promptSelectionOptions = new PromptSelectionOptions();
			promptSelectionOptions.MessageForAdding = "Select all of the heads you want to replace";
			PromptSelectionResult headsToReplaceSelection = Session.GetEditor().GetSelection(promptSelectionOptions, selectionFilter);

			if (headsToReplaceSelection.Status != PromptStatus.OK)
			{
				return null;
			}

			return headsToReplaceSelection.Value;
		}

		private string GetHeadBlockName(BlockReference headBlock)
		{
			BlockTableRecord originalHeadBlock = headBlock.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;
			return originalHeadBlock.Name;
		}

		private List<string> GetAllBlockNames(Transaction transaction, string blockName)
		{
			List<string> allNames = new List<string>();

			allNames.Add(blockName);

			BlockTable drawingBlockTable = transaction.GetObject(Session.GetDatabase().BlockTableId, OpenMode.ForRead) as BlockTable;

			BlockTableRecord blockTableRecord = transaction.GetObject(drawingBlockTable[blockName], OpenMode.ForRead) as BlockTableRecord;

			if (blockTableRecord == null)
			{
				return null;
			}

			Handle blockHandle = blockTableRecord.Handle;

			// Iterate all blocks in the drawing and find all dynamic instances of this block
			foreach (ObjectId objectId in drawingBlockTable)
			{
				BlockTableRecord candidateBlockTableRecord = transaction.GetObject(objectId, OpenMode.ForRead) as BlockTableRecord;
				
				if (candidateBlockTableRecord.Name == blockName)
				{
					continue;
				}

				ResultBuffer XData = candidateBlockTableRecord.XData;
				
				if (XData == null)
				{
					continue;
				}
				
				TypedValue[] typedValues = XData.AsArray();

				for(int i = 0; i < typedValues.Length; i++)
				{
					TypedValue typedValue = typedValues[i];

					if (typedValue.TypeCode != (int)DxfCode.ExtendedDataRegAppName)
					{
						continue;
					}

					if ((string)typedValue.Value != "AcDbBlockRepBTag")
					{
						continue;
					}
					
					for (int j = i + 1; j < typedValues.Length; j++)
					{
						typedValue = typedValues[j];

						if (typedValue.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
						{
							i = j - 1;
							break;
						}

						if (typedValue.TypeCode == (int)DxfCode.ExtendedDataHandle)
						{
							if ((string)typedValue.Value == blockHandle.ToString())
							{
								allNames.Add(candidateBlockTableRecord.Name);
								i = typedValues.Length - 1;
								break;
							}
						}
					}
				}
			}

			return allNames;
		}

		private TypedValue[] CreateFilterListForBlocks(List<string> blockNames)
		{
			if (blockNames.Count == 0)
				return null;

			if (blockNames.Count == 1)
			{
				return new TypedValue[]
					{
						new TypedValue((int)DxfCode.BlockName, blockNames[0])
					};
			}

			List<TypedValue> typedValues = new List<TypedValue>(blockNames.Count + 2);

			typedValues.Add(new TypedValue((int)DxfCode.Operator, "<or"));

			foreach (string blockName in blockNames)
			{
				typedValues.Add(new TypedValue((int)DxfCode.BlockName, (blockName.StartsWith("*") ? "`" + blockName : blockName)));
			}

			typedValues.Add(new TypedValue((int)DxfCode.Operator, "or>"));

			return typedValues.ToArray();
		}
	}
}
