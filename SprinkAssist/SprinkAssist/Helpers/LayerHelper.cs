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
using Autodesk.AutoCAD.Colors;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill
{
	[Flags]
	public enum LayerStatus
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
		public static LayerTableRecord FindLayer(Transaction transaction, string layerName, OpenMode openMode)
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

		public static void ToggleVisibility(Transaction transaction, string layerName)
		{
			LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForWrite);

			if (layerTableRecord == null)
			{
				return;
			}

			bool wasOff = layerTableRecord.IsOff;
			layerTableRecord.IsOff = !wasOff;

			Session.GetEditor().ApplyCurDwgLayerTableChanges();
			//Session.GetEditor().Regen();
			Session.GetEditor().Command("REGENALL");
		}

		// TODO all of these helpers should take in a parent transaction
		public static void ToggleFrozen(Transaction transaction, string layerName)
		{
			LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForWrite);

			if (layerTableRecord == null)
			{
				return;
			}

			if (Session.GetDatabase().Clayer == layerTableRecord.ObjectId)
			{
				string defaultLayerName = "0";
				LayerTable layerTable = Session.GetLayerTable(transaction, OpenMode.ForWrite);
				Database database = Session.GetDatabase();

				if (layerTable.Has(defaultLayerName))
				{
					Session.Log("Setting current layer to " + defaultLayerName);
					database.Clayer = layerTable[defaultLayerName];
				}
				else
				{
					Session.Log("Failed - cannot freeze current layer");
				}
			}

			bool wasFrozen = layerTableRecord.IsFrozen;
			layerTableRecord.IsFrozen = !wasFrozen;

			Session.GetEditor().ApplyCurDwgLayerTableChanges();
			Session.GetEditor().Command("REGENALL");
		}

		public static bool GetLayerState(Transaction transaction, string layerName, ref LayerStatus layerStatus)
		{
			LayerTableRecord layerTableRecord = FindLayer(transaction, layerName, OpenMode.ForWrite);

			if (layerTableRecord == null)
			{
				return false;
			}

			if (!layerTableRecord.IsHidden)
			{
				layerStatus |= LayerStatus.Visible;
			}

			if (layerTableRecord.IsFrozen)
			{
				layerStatus |= LayerStatus.Frozen;
			}

			if (layerTableRecord.ViewportVisibilityDefault)
			{
				layerStatus |= LayerStatus.ViewportVisibilityDefault;
			}

			if (layerTableRecord.IsLocked)
			{
				layerStatus |= LayerStatus.Locked;
			}

			if (layerTableRecord.IsPlottable)
			{
				layerStatus |= LayerStatus.Plots;
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

		public static void SetColor(LayerTableRecord ltr, short ColorIndex)
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

		/** Given a string, find all of the layers in this drawing that contain the string */
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
	}
}
