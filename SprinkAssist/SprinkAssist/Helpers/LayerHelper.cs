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
using Autodesk.AutoCAD.Colors;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.PlottingServices;
using Ironwill.Commands;

namespace Ironwill
{
	[Flags]
	public enum ELayerStatus
	{
		None = 0,
		Visible = 1,
		Frozen = 2,
		ViewportVisibilityDefault = 4,
		Locked = 8,
		Plots = 16
	}

	public class LayerHelper
	{
		// ========================================================================================
		
        public static void SetLocked(Transaction transaction, bool lockedState, params string[] layerNames)
        {
            ExecuteOnLayers(
                transaction,
                (layerTableRecord) =>
                {
                    layerTableRecord.IsLocked = lockedState;
                },
                layerNames);
        }
        
		public static void ToggleLocked(Transaction transaction, string layerName)
		{
			ExecuteOnLayers(
				transaction,
                (layerTableRecord) =>
                {
                    layerTableRecord.IsLocked = !layerTableRecord.IsLocked;
                },
				layerName);
		}

		// ========================================================================================

		public static void SetVisible(Transaction transaction, bool visibleState, params string[] layerNames)
        {
            ExecuteOnLayers(
                transaction,
                (layerTableRecord) =>
                {
                    layerTableRecord.IsOff = !visibleState;
                },
                layerNames);
		}

        public static void ToggleVisible(Transaction transaction, string layerName)
        {
            ExecuteOnLayers(
                transaction,
                (layerTableRecord) =>
                {
                    layerTableRecord.IsOff = !layerTableRecord.IsOff;
                },
                layerName);
        }
        
		// ========================================================================================

		public static void SetFrozen(Transaction transaction, bool frozenState, params string[] layerNames)
        {
            ExecuteOnLayers(
                transaction,
                (layerTableRecord) =>
                {
                    if (layerTableRecord.ObjectId == Session.GetDatabase().Clayer)
                    {
                        SetCurrentLayer(transaction, "0");
                    }

					Session.GetEditor().Command("-layer", frozenState ? "f" : "t", $"{layerTableRecord.Name}", "");

					//layerTableRecord.IsFrozen = frozenState; // This is the .net way, but it requires manually regenerating the whole drawing, much slower!
				},
                layerNames);
		}

		public static void ToggleFrozen(Transaction transaction, string layerName)
		{
			ExecuteOnLayers(
				transaction,
				(layerTableRecord) =>
				{
					if (layerTableRecord.ObjectId == Session.GetDatabase().Clayer)
					{
						SetCurrentLayer(transaction, "0");
					}

					bool frozenState = !layerTableRecord.IsFrozen;

					Session.GetEditor().Command("-layer", frozenState ? "f" : "t", $"{layerTableRecord.Name}", "");

					//layerTableRecord.IsFrozen = !layerTableRecord.IsFrozen; // This is the .net way, but it requires manually regenerating the whole drawing, much slower!
				},
				layerName);
		}

		// ========================================================================================

		public static float GetTransparency(Transaction transaction, string layerName)
		{
			LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForRead);

			if (layerTableRecord == null)
			{
				Session.Log($"Warning: could not find layer {layerName}");
				return 0.0f;
			}

			Transparency t = layerTableRecord.Transparency;

			if (t.IsByAlpha)
			{
				return (256 - t.Alpha) / 256;
			}
			else
			{
				return 0f;
			}
		}

		public static void SetTransparency(Transaction transaction, double transparency, params string[] layerNames)
		{
			transparency = Math.Min(1.0f, transparency);
			transparency = Math.Max(0.0f, transparency);

			ExecuteOnLayers(
				transaction,
				(layerTableRecord) =>
				{
					byte b = (byte)((1.0f - transparency) * (256f));
					layerTableRecord.Transparency = new Transparency(b);
					layerTableRecord.IsOff = layerTableRecord.IsOff;
				},
				layerNames);
		}

		// ========================================================================================

		public static bool GetLayerState(Transaction transaction, string layerName, ref ELayerStatus layerStatus)
		{
			LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForWrite);

			if (layerTableRecord == null)
			{
				return false;
			}

			if (!layerTableRecord.IsHidden)
			{
				layerStatus |= ELayerStatus.Visible;
			}

			if (layerTableRecord.IsFrozen)
			{
				layerStatus |= ELayerStatus.Frozen;
			}

			if (layerTableRecord.ViewportVisibilityDefault)
			{
				layerStatus |= ELayerStatus.ViewportVisibilityDefault;
			}

			if (layerTableRecord.IsLocked)
			{
				layerStatus |= ELayerStatus.Locked;
			}

			if (layerTableRecord.IsPlottable)
			{
				layerStatus |= ELayerStatus.Plots;
			}

			return true;
		}

		public static void SetLinetype(Transaction transaction, LayerTableRecord ltr, string name, bool forceLoad = false)
		{
			LinetypeTable linetypeTable = transaction.GetObject(Session.GetDatabase().LinetypeTableId, OpenMode.ForRead) as LinetypeTable;

			if (linetypeTable == null)
			{
				Session.Log("Failed - could not find linetype table?");
				return;
			}

			if (!linetypeTable.Has(name))
			{
				List<string> linetypeFiles = new List<string>()
				{
					"acad.lin",
					"spkassist.lin",
				};

				foreach (string linetypeFile in linetypeFiles)
				{
					Session.GetDatabase().LoadLineTypeFile(name, linetypeFile);

					if (linetypeTable.Has(name))
					{
						break;
					}
				}
			}

			if (!linetypeTable.Has(name))
			{
				Session.Log("Failed - linetype " + name + " could not be found");
				return;
			}

			ltr.LinetypeObjectId = linetypeTable[name];
		}

		public static void SetAllObjectsLinetypeScale(Transaction transaction, LayerTableRecord ltr, double newScale)
		{
			TypedValue[] filterList = new TypedValue[1];
			filterList[0] = new TypedValue((int)DxfCode.LayerName, ltr.Name);

			SelectionFilter selectionFilter = new SelectionFilter(filterList);

			PromptSelectionResult promptSelectionResult = Session.GetEditor().SelectAll(selectionFilter);

			if (promptSelectionResult.Status == PromptStatus.OK)
			{
				SelectionSet selectionSet = promptSelectionResult.Value;

				foreach (ObjectId objectId in selectionSet.GetObjectIds())
				{
					Entity entity = transaction.GetObject(objectId, OpenMode.ForWrite) as Entity;

					if (entity == null)
					{
						continue;
					}

					entity.LinetypeScale = newScale;
				}
			}
		}

		public static void SetLayerColor(LayerTableRecord ltr, short ColorIndex)
		{
			ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, ColorIndex);
		}

		public static string DeleteLayer(Document document, Transaction transaction, string layerName)
		{
			Database database = document.Database;

			if (layerName == "0")
			{
				return "Layer '0' cannot be deleted.";
			}

			LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);

			if (!layerTable.Has(layerName))
			{
				return "Layer '" + layerName + "' not found.";
			}
			try
			{
				ObjectId layerId = layerTable[layerName];
					
				if (database.Clayer == layerId)
				{
					return "Current layer cannot be deleted.";
				}
				
				LayerTableRecord layer = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);
				layer.IsLocked = false;

				BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
				
				foreach (ObjectId btrId in blockTable)
				{
					BlockTableRecord block = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
					foreach (var entId in block)
					{
						Entity ent = (Entity)transaction.GetObject(entId, OpenMode.ForRead);
						if (ent.Layer == layerName)
						{
							ent.UpgradeOpen();
							ent.Erase();
						}
					}
				}

				layer.Erase();
				return "Layer '" + layerName + "' has been deleted.";
			}
			catch (System.Exception e)
			{
				return "Error: " + e.Message;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="containingString"></param>
		/// <returns></returns>
		public static List<string> CollectLayersWithString(string containingString)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Database database = Session.GetDatabase();
				List<string> ceilingLayers = new List<string>();

				SymbolTable symbolTable = transaction.GetObject(database.LayerTableId, OpenMode.ForWrite) as SymbolTable;

				if (symbolTable == null)
				{
					Session.Log("Error, symbol table not found!");
					return ceilingLayers;
				}

				foreach (ObjectId objectId in symbolTable)
				{
					LayerTableRecord layerTableRecord = transaction.GetObject(objectId, OpenMode.ForWrite) as LayerTableRecord;

					if (layerTableRecord == null)
					{
						continue;
					}

					string name = layerTableRecord.Name;

					if (name.Contains(containingString))
					{
						ceilingLayers.Add(name);
					}
				}

				return ceilingLayers;
			}
		}

		public static bool SetCurrentLayer(Transaction transaction, string layerName)
		{
			LayerTable layerTable = Session.GetLayerTable(transaction, OpenMode.ForWrite);
			Database database = Session.GetDatabase();

			if (layerTable.Has(layerName))
			{
				database.Clayer = layerTable[layerName];
				return true;
			}
			else
			{
				Session.Log($"Failed - layer {layerName} does not exist!");
				return false;
			}
		}

		private static LayerTableRecord FindLayer(Transaction transaction, string layerName, OpenMode openMode)
		{
			LayerTable layerTable = Session.GetLayerTable(transaction, openMode);

			foreach (ObjectId layerTableRecordID in layerTable)
			{
				LayerTableRecord layerTableRecord = transaction.GetObject(layerTableRecordID, OpenMode.ForWrite) as LayerTableRecord;

				if (layerTableRecord == null)
					continue;

				if (layerTableRecord.Name == layerName)
				{
					return layerTableRecord;
				}
			}

			Session.Log("Could not find layer: " + layerName);
			return null;
		}
        
        private static void ExecuteOnLayers(Transaction transaction, Action<LayerTableRecord> func, params string[] layerNames)
        {
            foreach (string layerName in layerNames)
            {
                LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForWrite);

                if (layerTableRecord == null)
                {
                    Session.Log($"Warning: could not find layer {layerName}");
                    continue;
                }

                func(layerTableRecord);
            }
        }
	}
}
