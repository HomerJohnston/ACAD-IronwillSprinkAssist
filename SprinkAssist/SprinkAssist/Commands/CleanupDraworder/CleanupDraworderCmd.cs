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
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.CleanupDraworder.CleanupDraworderCmd))]

namespace Ironwill.Commands.CleanupDraworder
{
	public class CleanupDraworderCmd
	{
		struct SortData
		{
			public string layerName;
			public TypedValue? objectType;
			public string objectName;

			public SortData(string inLayerName, TypedValue? inType, string inSpecificName = "")
			{
				objectType = inType;
				layerName = inLayerName;
				objectName = inSpecificName;
			}
		}

		static TypedValue lines = new TypedValue((int)DxfCode.Start, "LINE");
		static TypedValue blocks = new TypedValue((int)DxfCode.Start, "INSERT");
		static TypedValue? anyType = null;
		static TypedValue modelSpace = new TypedValue((int)DxfCode.ViewportVisibility, 0);
		static string anyLayer = string.Empty;

		[CommandDescription("Sets up common draw order of sprinkler elements.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "CleanupDrawOrder", CommandFlags.Modal | CommandFlags.NoBlockEditor)]
		public void Main()
		{
			List<SortData> allSortData = new List<SortData>()
			{
				// Bottom to Top
				new SortData(Layer.Extinguisher.Get(),				blocks),

				new SortData(Layer.SystemPipe_AuxDrain.Get(),		lines),
				new SortData(Layer.SystemPipe_Armover.Get(),		lines),
				new SortData(Layer.SystemPipe_Branchline.Get(),		lines),
				new SortData(Layer.SystemPipe_Main.Get(),			lines),

				new SortData(anyLayer,                              blocks, Blocks.Fitting_Cap.Get()),
				new SortData(Layer.SystemFitting.Get(),				blocks),

				new SortData(Layer.SystemPipe_Main.Get(),			blocks),
				new SortData(Layer.SystemPipe_Branchline.Get(),		blocks),
				new SortData(Layer.SystemPipe_Armover.Get(),		blocks),
				new SortData(Layer.SystemPipe_AuxDrain.Get(),		blocks),


				new SortData(Layer.PipeLabel.Get(),					blocks),

				new SortData(Layer.SystemDevice.Get(),				blocks),
				new SortData(Layer.SystemHead.Get(),				blocks),

				new SortData(Layer.Dimension.Get(),					anyType),
				new SortData(Layer.Note.Get(),						anyType),
			};

			HashSet<ObjectId> sortedObjects = new HashSet<ObjectId>();

			using (Transaction tr = Session.StartTransaction())
			{
				foreach (SortData sortEntryData in allSortData)
				{
					List<TypedValue> filterList = new List<TypedValue>();

					filterList.Add(modelSpace);

					if (sortEntryData.layerName != anyLayer)
					{
						filterList.Add(new TypedValue((int)DxfCode.LayerName, sortEntryData.layerName));
					}

					if (sortEntryData.objectType != null)
					{
						filterList.Add(sortEntryData.objectType.Value);
					}

					if (sortEntryData.objectName != string.Empty)
					{
						filterList.Add(new TypedValue((int)DxfCode.BlockName, sortEntryData.objectName));
					}

					SelectionFilter selectionFilter = new SelectionFilter(filterList.ToArray());

					PromptSelectionResult promptSelectionResult = Session.GetEditor().SelectAll(selectionFilter);

					if (promptSelectionResult.Status != PromptStatus.OK)
					{
						continue;
					}

					ObjectIdCollection objectIdCollection = new ObjectIdCollection();
					
					SelectionSet selectionSet = promptSelectionResult.Value;

					foreach (ObjectId objectId in selectionSet.GetObjectIds())
					{
						if (sortedObjects.Contains(objectId))
						{
							continue;
						}

						sortedObjects.Add(objectId);
						objectIdCollection.Add(objectId);
					}

					ObjectId[] objectIds = new ObjectId[objectIdCollection.Count];
					objectIdCollection.CopyTo(objectIds, 0);

					//Session.GetEditor().SetImpliedSelection(promptSelectionResult.Value);
					Session.GetEditor().SetImpliedSelection(objectIds);
					//Autodesk.AutoCAD.Internal.Utils.SelectObjects(objectIds);
					
					var CMDECHO = AcApplication.GetSystemVariable("CMDECHO");
					AcApplication.SetSystemVariable("CMDECHO", 0);
					Session.Command("draworder", "F");
					AcApplication.SetSystemVariable("CMDECHO", CMDECHO);
				}

				tr.Commit();
			}
		}

		/*
		// For some reason this method is EXTREMELY slow!
		void MoveAllToTopAlt(Transaction tr, string layer, TypedValue objectType)
		{
			TypedValue[] filterList = new TypedValue[2];
			filterList[0] = new TypedValue((int)DxfCode.LayerName, layer);
			filterList[1] = objectType;

			SelectionFilter selectionFilter = new SelectionFilter(filterList);

			PromptSelectionResult promptSelectionResult = Session.GetEditor().SelectAll(selectionFilter);

			if (promptSelectionResult.Status != PromptStatus.OK)
			{
				return;
			}

			SelectionSet selectionSet = promptSelectionResult.Value;

			ObjectIdCollection objectIdCollection = new ObjectIdCollection();

			foreach (ObjectId objectId in selectionSet.GetObjectIds())
			{
				Entity entity = tr.GetObject(objectId, OpenMode.ForRead) as Entity;

				if (entity == null)
				{
					continue;
				}

				BlockTableRecord blockTableRecord = tr.GetObject(entity.BlockId, OpenMode.ForRead) as BlockTableRecord;

				if (blockTableRecord == null)
				{
					continue;
				}

				DrawOrderTable drawOrderTable = tr.GetObject(blockTableRecord.DrawOrderTableId, OpenMode.ForWrite) as DrawOrderTable;

				if (drawOrderTable == null)
				{
					return;
				}

				objectIdCollection.Add(objectId);

				//drawOrderTable.MoveToTop(objectIdCollection);
			}
		}
		*/
	}
}
