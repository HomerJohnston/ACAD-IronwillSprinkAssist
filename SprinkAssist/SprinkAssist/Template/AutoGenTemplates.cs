using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.AutoCAD.Runtime;
using Ironwill.Commands;
using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;


using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(Ironwill.Template.AutoGenTemplates))]

namespace Ironwill.Template
{
/*
	void GenMetric(Document document, Transaction transaction)
	{
		// Delete imperial layouts

		// Cleanly rename all layout tabs
		CleanLayoutNames(document, transaction, "(Metric)");


		// Delete imperial model space objects // TODO: my dynamic extinguisher blocks' extents are huge, why?
		EraseObjectsWithin(document, transaction, "TEMPLATE_BASE");
		EraseObjectsWithin(document, transaction, "TEMPLATE_IMPERIAL");


		// Erase template layers
		LayerHelper.Delete(document, transaction, "TEMPLATE_IMPERIAL");
		LayerHelper.Delete(document, transaction, "TEMPLATE_METRIC");
		LayerHelper.Delete(document, transaction, "TEMPLATE_BASE");


		// Set current styles
		SetTextStyle(transaction, DefaultMetricTextStyle);
		SetDimStyle(document, transaction, DefaultMetricDimStyle);
		SetTableStyle(transaction, DefaultMetricTableStyle);
		SetMLeaderStyle(transaction, DefaultMetricMLeaderStyle);


		// Set system variables
		AcApplication.SetSystemVariable("LUNITS", 2);
		AcApplication.SetSystemVariable("LTSCALE", 100);
		AcApplication.SetSystemVariable("MEASUREMENT", 1);
		AcApplication.SetSystemVariable("PDSIZE", 50);

		// Reload linetypes 
		// ReloadLinetypes(document, transaction); // TODO do I actually need to do this?

		// Purge
		//Session.Command("-pu", "all", "*", "n");
		//Session.Command("-pu", "all", "*", "n");
		//Session.Command("-pu", "all", "*", "n");
	}*/

	internal class GenerationData
	{
		public string outputFile;

		public List<string> eraseLayoutsContaining = new List<string>();
		public List<string> cleanLayoutsContaining = new List<string>();

		public List<string> eraseEntitiesWithinLayers = new List<string>();
		public List<string> templateLayersToErase = new List<string>();

		public string defaultTextStyle;
		public string defaultDimStyle;
		public string defaultTableStyle;
		public string defaultMLeaderStyle;
		
		public int LUNITS;
		public int LTSCALE;
		public int MEASUREMENT;
		public int PDSIZE;

		public Vector3d modelSpaceDisplacement;
	}

	internal class AutoGenTemplates
	{
		List<string> MetricLayouts = new List<string>()
		{
			"NOTES",
			"ESFR",
			"DETAILS",
			"11x17",
			"24x36",
			"30x42",
			"36x48",
			"11x17 (H)",
			"24x36 (H)",
			"30x42 (H)",
			"36x48 (H)"
		};

		List<string> ImperialLayouts = new List<string>()
		{
			"NOTES (MET)",
			"ESFR (MET)",
			"DETAILS (MET)",
			"11x17 (MET)",
			"24x36 (MET)",
			"30x42 (MET)",
			"36x48 (MET)",
			"11x17 (H) (MET)",
			"24x36 (H) (MET)",
			"30x42 (H) (MET)",
			"36x48 (H) (MET)"
		};

		string MetricLayoutMarker = "(MET)";
		string ImperialLayoutMarker = "(IMP)";
		string TempLayoutMarker = "(TEMP)";

		string MetricTemplateLayer = "TEMPLATE_METRIC";
		string ImperialTemplateLayer = "TEMPLATE_IMPERIAL";
		string BaseTemplateLayer = "TEMPLATE_BASE";

		List<string> MetricDimStyles = new List<string>()
		{
			"Ironwill - Metric (Annotative)",
			"Ironwill - Metric 1-004",
			"Ironwill - Metric 1-010",
			"Ironwill - Metric 1-100",
		};

		List<string> ImperialDimStyles = new List<string>()
		{
			"Ironwill - Imperial (Annotative)",
			"Ironwill - Imperial 1-06",
			"Ironwill - Imperial 1-12",
			"Ironwill - Imperial 1-96",
		};

		List<string> MiscTextStyles = new List<string>()
		{
			"_IFE Linetype - Courier New",
			"_IFE Pipe Labels - isocp",
			"_IFE Pipe Labels - romans",
			"_IFE Template Notes - romans",
			"_IFE Titleblock - Arial",
			"_IFE Titleblock - OpenSans",
			"IFE Dims (Anno)",
			"IFE Unscaled",
			"IFE Unscaled Bold",
		};

		List<string> MetricTextStyles = new List<string>()
		{
			"IFE Metric Heading (1-1)",
			"IFE Metric Heading (1-100)",
			"IFE Metric Heading (Anno)",
			"IFE Metric Notes (1-1)",
			"IFE Metric Notes (1-100)",
			"IFE Metric Notes (Anno)",
		};

		List<string> TextStyles = new List<string>()
		{
			"IFE Imp Heading (1-1)",
			"IFE Imp Heading (1-96)",
			"IFE Imp Heading (Anno)",
			"IFE Imp Notes (1-1)",
			"IFE Imp Notes (1-96)",
			"IFE Imp Notes (Anno)",
		};

		List<string> MetricTableStyles = new List<string>()
		{
			"Ironwill Metric Device Legend",
			"Ironwill Metric Sprinkler Legend",
		};

		List<string> ImperialTableStyles = new List<string>()
		{
			"Ironwill Imperial Device Legend",
			"Ironwill Imperial Sprinkler Legend",
		};

		List<string> MetricMLeaderStyles = new List<string>()
		{
			"IFE - Metric (Annotative)",
			"IFE - Metric 1-100",
		};

		List<string> ImperialMLeaderStyles = new List<string>()
		{
			"IFE - Imperial (Anno)",
			"IFE - Imperial 1-96",
		};

		string DefaultMetricTextStyle = "IFE Unscaled";
		string DefaultImperialTextStyle = "IFE Unscaled";

		string DefaultMetricDimStyle = "Ironwill - Metric (Anno)";
		string DefaultImperialDimStyle = "Ironwill - Imperial (Anno)";

		string DefaultMetricTableStyle = "Standard";
		string DefaultImperialTableStyle = "Standard";
		
		string DefaultMetricMLeaderStyle = "IFE - Metric (Anno)";
		string DefaultImperialMLeaderStyle = "IFE - Imperial (Anno)";
		
		// Note: I need "session" context in order to close and open drawings. 
		// Without this, I found that fucking autocad layout tabs bug out whenever I erase/rename them from C# and undo.
		// Closing and freshly opening the drawing in between templates keeps a clean slate and works.
		[CommandMethod("GenTemplates", CommandFlags.Session)] // TODO add group name
		public void GenerateTemplates()
		{
			GenerateMetric();
			GenerateImperial();
		}

		void GenerateMetric()
		{
			GenerationData metricData = new GenerationData();

			metricData.outputFile = "C:/Users/KWilcox/Desktop/Test_Metric.dwg";

			metricData.eraseLayoutsContaining.Add(ImperialLayoutMarker);
			metricData.eraseLayoutsContaining.Add(TempLayoutMarker);

			metricData.cleanLayoutsContaining.Add(MetricLayoutMarker);

			metricData.eraseEntitiesWithinLayers.Add(ImperialTemplateLayer);
			metricData.eraseEntitiesWithinLayers.Add(BaseTemplateLayer);

			metricData.templateLayersToErase.Add(ImperialTemplateLayer);
			metricData.templateLayersToErase.Add(MetricTemplateLayer);
			metricData.templateLayersToErase.Add(BaseTemplateLayer);

			metricData.defaultTextStyle = DefaultMetricTextStyle;
			metricData.defaultDimStyle = DefaultMetricDimStyle;
			metricData.defaultTableStyle = DefaultMetricTableStyle;
			metricData.defaultMLeaderStyle = DefaultMetricMLeaderStyle;

			metricData.LUNITS = 2;
			metricData.LTSCALE = 100;
			metricData.MEASUREMENT = 1;
			metricData.PDSIZE = 50;

			metricData.modelSpaceDisplacement = new Vector3d(0, 0, 0);

			GenerateTemplate(metricData);
		}

		void GenerateImperial()
		{
			GenerationData imperialData = new GenerationData();

			imperialData.outputFile = "C:/Users/KWilcox/Desktop/Test_Imperial.dwg";

			imperialData.eraseLayoutsContaining.Add(MetricLayoutMarker);
			imperialData.eraseLayoutsContaining.Add(TempLayoutMarker);

			imperialData.cleanLayoutsContaining.Add(ImperialLayoutMarker);

			imperialData.eraseEntitiesWithinLayers.Add(MetricTemplateLayer);
			imperialData.eraseEntitiesWithinLayers.Add(BaseTemplateLayer);

			imperialData.templateLayersToErase.Add(ImperialTemplateLayer);
			imperialData.templateLayersToErase.Add(MetricTemplateLayer);
			imperialData.templateLayersToErase.Add(BaseTemplateLayer);

			imperialData.defaultTextStyle = DefaultImperialTextStyle;
			imperialData.defaultDimStyle = DefaultImperialDimStyle;
			imperialData.defaultTableStyle = DefaultImperialTableStyle;
			imperialData.defaultMLeaderStyle = DefaultImperialMLeaderStyle;

			imperialData.LUNITS = 4;
			imperialData.LTSCALE = 4;
			imperialData.MEASUREMENT = 0;
			imperialData.PDSIZE = 2;

			imperialData.modelSpaceDisplacement = new Vector3d(0, 500000, 0);

			GenerateTemplate(imperialData);
		}

		void GenerateTemplate(GenerationData data)
		{
			DocumentCollection documentCollection = Session.GetDocumentManager();

			string templatePath = Session.GetDatabase().Filename;

			using (Document document = documentCollection.Open(templatePath))
			{
				Database database = document.Database;

				using (DocumentLock documentLock = document.LockDocument())
				{
					using (Transaction transaction = Session.StartTransaction())
					{
						ProcessAndGenerateFile(document, transaction, data);

						database.SaveAs(data.outputFile, DwgVersion.Current);
					}
				}

				document.CloseAndDiscard();
			}
		}

		void ProcessAndGenerateFile(Document document, Transaction transaction, GenerationData data)
		{
			foreach (string s in data.eraseLayoutsContaining)
			{
				EraseLayoutsWithSubstring(document, transaction, s);
			}

			foreach (string s in data.cleanLayoutsContaining)
			{
				CleanLayoutNames(document, transaction, s);
			}

			foreach (string s in data.eraseEntitiesWithinLayers)
			{
				EraseObjectsWithin(document, transaction, s);
			}

			foreach (string s in data.templateLayersToErase)
			{
				LayerHelper.DeleteLayer(document, transaction, s);
			}

			// Set current styles
			SetTextStyle(transaction, data.defaultTextStyle);
			SetDimStyle(document, transaction, data.defaultDimStyle);
			SetTableStyle(transaction, data.defaultTableStyle);
			SetMLeaderStyle(transaction, data.defaultMLeaderStyle);

			// Set system variables
			AcApplication.SetSystemVariable("LUNITS", data.LUNITS);
			AcApplication.SetSystemVariable("LTSCALE", data.LTSCALE);
			AcApplication.SetSystemVariable("MEASUREMENT", data.MEASUREMENT);
			AcApplication.SetSystemVariable("PDSIZE", data.PDSIZE);

			// Reload linetypes 
			// ReloadLinetypes(document, transaction); // TODO do I actually need to do this?

			// Move the imperial template stuff up to middle
			if (data.modelSpaceDisplacement.Length > 0)
			{
				ShiftAllDrawingElements(document, transaction, data.modelSpaceDisplacement);
			}

			// TODO Purge. Figure out how to purge. Do it manually? Editor extension?
			// Can't run commands from session context.
			// Can't run SendStringToExecute without making it an await in the session context (maybe I should?)
			// Can't conveniently pass parameters to SendStringToExecute either
			// I should only purge styles and metric/imperial specific blocks, if I have any?
			//Session.Command("-purge", "all", "*", "n");

			document.Editor.ZoomExtents();
		}

		void EraseLayoutsWithSubstring(Document document, Transaction transaction, string targetString)
		{
			ProcessLayouts(document, transaction, (layout) =>
			{
				LayoutManager layoutManager = LayoutManager.Current;

				if (layout.LayoutName.ToUpper().Contains(targetString.ToUpper()))
				{
					Session.Log("Erasing layout: " + layout.LayoutName); 
					layoutManager.DeleteLayout(layout.LayoutName);
				}
			});
		}

		void CleanLayoutNames(Document document, Transaction transaction, string targetString)
		{
			ProcessLayouts(document, transaction, (layout) =>
			{
				string layoutName = layout.LayoutName.Replace(targetString, "");
				layoutName = layoutName.Trim();

				if (layout.IsEraseStatusToggled)
				{
					return;
				}

				layout.LayoutName = layoutName;
			});
		}

		void ProcessLayouts(Document document, Transaction transaction, Action<Layout> process)
		{
			Database database = document.Database;

			DBDictionary layoutDictionary = (DBDictionary)transaction.GetObject(database.LayoutDictionaryId, OpenMode.ForWrite);

			Layout[] layouts = layoutDictionary
				.Cast<System.Collections.DictionaryEntry>()
				.Select(entry => (Layout)transaction
				.GetObject((ObjectId)entry.Value, OpenMode.ForWrite))
				.ToArray();

			for (int i = 0; i < layouts.Length; i++)
			{
				Layout layout = layouts[i];

				if (layout.ModelType)
				{
					continue;
				}

				process(layout);
			}
		}

		void EraseObjectsWithin(Document document, Transaction transaction, string boundaryLayerName)
		{
			List<Entity> entities = new List<Entity>();

			ModelSpaceHelper.GetObjectsWithinBoundaryCurvesOfLayerByExtents(document, transaction, ref entities, boundaryLayerName, OpenMode.ForWrite);

			Session.Log("Found " + entities.Count + " entities in " + boundaryLayerName + " boundaries");

			foreach (Entity entity in entities)
			{
				//Session.LogDebug("Erasing " + entity.GetType().Name + " from layer " + entity.Layer);
				entity.UpgradeOpen();
				entity.Erase();
			}
		}

		void SetTextStyle(Transaction transaction, string desiredTextStyle)
		{
			AcApplication.SetSystemVariable("TEXTSTYLE", desiredTextStyle);
		}

		void SetDimStyle(Document document, Transaction transaction, string desiredDimStyle)
		{
			Database database = document.Database;

			DimStyleTable dimStyleTable = transaction.GetObject(database.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;

			bool success = false;

			foreach (ObjectId objectId in dimStyleTable)
			{
				DimStyleTableRecord dimStyleTableRecord = transaction.GetObject(objectId, OpenMode.ForRead) as DimStyleTableRecord;

				if (dimStyleTableRecord.Name == desiredDimStyle)
				{
					Session.Log("Setting dimstyle to " + dimStyleTableRecord.Name);
					database.Dimstyle = objectId;
					database.SetDimstyleData(dimStyleTableRecord);
					success = true;
					break;
				}
			}

			if (!success)
			{
				Session.Log("Failed to set dim style to " + desiredDimStyle);
			}
		}

		void SetTableStyle(Transaction transaction, string desiredTableStyle)
		{
			AcApplication.SetSystemVariable("CTABLESTYLE", desiredTableStyle);
		}

		void SetMLeaderStyle(Transaction transaction, string desiredMLeaderStyle)
		{
			AcApplication.SetSystemVariable("CMLEADERSTYLE", desiredMLeaderStyle);
		}

		void ReloadLinetypes(Document document, Transaction transaction)
		{
			Database database = document.Database;

			SymbolTable symbolTable = (SymbolTable)transaction.GetObject(database.LinetypeTableId, OpenMode.ForWrite);

			List<string> lineTypes = new List<string>();

			foreach (ObjectId objectId in symbolTable)
			{
				LinetypeTableRecord symbol = (LinetypeTableRecord)transaction.GetObject(objectId, OpenMode.ForWrite);
				
				lineTypes.Add(symbol.Name);
			}

			using (Database tempDatabase = new Database(false, true))
			{
				LinetypeTable linetypeTable = (LinetypeTable)transaction.GetObject(tempDatabase.LinetypeTableId, OpenMode.ForRead);

				ObjectIdCollection objectIdCollection = new ObjectIdCollection();

				foreach (string lineType in lineTypes)
				{
					tempDatabase.LoadLineTypeFile(lineType, database.Filename);
					ObjectId linetypeId = linetypeTable[lineType];
					objectIdCollection.Add(linetypeId);
				}

				IdMapping idMapping = new IdMapping();

				database.WblockCloneObjects(objectIdCollection, database.LinetypeTableId, idMapping, DuplicateRecordCloning.Replace, true);
			}
		}

		void ShiftAllDrawingElements(Document document, Transaction transaction, Vector3d displacement)
		{
			Database database = document.Database;

			ModelSpaceHelper.IterateAllEntities(transaction, database, ModelSpaceHelper.ERecurseFlags.None, (entity, parentMatrix) =>
			{
				entity.UpgradeOpen();

				entity.TransformBy(Autodesk.AutoCAD.Geometry.Matrix3d.Displacement(displacement));
			});
		}
	}
}
