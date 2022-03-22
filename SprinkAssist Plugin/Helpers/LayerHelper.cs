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

			Session.WriteMessage("Could not find layer: " + layerName);
			return null;
		}

		public static void ToggleVisibility(string layerName)
		{
			using (Transaction transaction = Session.GetTransactionManager().StartTransaction())
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

				transaction.Commit();
			}
		}

		public static void ToggleFrozen(string layerName)
		{

			using (Transaction transaction = Session.GetTransactionManager().StartTransaction())
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
						Session.WriteMessage("Setting current layer to " + defaultLayerName);
						database.Clayer = layerTable[defaultLayerName];
					}
					else
					{
						Session.WriteMessage("Failed - cannot freeze current layer");
					}
				}

				bool wasFrozen = layerTableRecord.IsFrozen;
				layerTableRecord.IsFrozen = !wasFrozen;

				Session.GetEditor().ApplyCurDwgLayerTableChanges();
				//Session.GetEditor().Regen();
				Session.GetEditor().Command("REGENALL");

				transaction.Commit();
			}
		}
	}
}
