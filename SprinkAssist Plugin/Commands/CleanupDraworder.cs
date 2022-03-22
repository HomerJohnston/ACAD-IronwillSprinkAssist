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

[assembly: CommandClass(typeof(Ironwill.CleanupDraworder))]

namespace Ironwill
{
	public class CleanupDraworder
	{
		[CommandMethod("SpkAssist_CleanupDrawOrder")]
		public void CleanupDrawOrderCmd()
		{
			List<string> linesLayerOrder = new List<string>
			{
				// Bottom to top
				Layers.Extinguisher.Get(),
				Layers.SystemPipe_AuxDrain.Get(),
				Layers.SystemPipe_Armover.Get(),
				Layers.SystemPipe_Branchline.Get(),
				Layers.SystemPipe_Main.Get(),
				Layers.SystemFitting.Get(),
				Layers.PipeLabel.Get(),
				Layers.SystemDevice.Get(),
				Layers.SystemHead.Get(),
			};

			List<TypedValue> typeOrder = new List<TypedValue>
			{
				// Bottom to top
				new TypedValue((int)DxfCode.Start, "LINE"),
				new TypedValue((int)DxfCode.Start, "INSERT"),
			};

			using (Transaction tr = Session.StartTransaction())
			{
				foreach (TypedValue typedValue in typeOrder)
				{
					foreach (string layer in linesLayerOrder)
					{
						MoveAllToTop(tr, layer, typedValue);
					}
				}
				tr.Commit();
			}
		}

		void MoveAllToTop(Transaction tr, string layer, TypedValue objectType)
		{
			TypedValue[] filterList = new TypedValue[3];
			filterList[0] = new TypedValue((int)DxfCode.LayerName, layer);
			filterList[1] = objectType;
			filterList[2] = new TypedValue((int)DxfCode.ViewportVisibility, 0);

			SelectionFilter selectionFilter = new SelectionFilter(filterList);
			

			PromptSelectionResult promptSelectionResult = Session.GetEditor().SelectAll(selectionFilter);
			
			if (promptSelectionResult.Status != PromptStatus.OK)
			{
				return;
			}

			Session.GetEditor().SetImpliedSelection(promptSelectionResult.Value);
			
			var CMDECHO = AcApplication.GetSystemVariable("CMDECHO");
			AcApplication.SetSystemVariable("CMDECHO", 0);
			Session.Command("draworder", "F");
			AcApplication.SetSystemVariable("CMDECHO", CMDECHO);
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
