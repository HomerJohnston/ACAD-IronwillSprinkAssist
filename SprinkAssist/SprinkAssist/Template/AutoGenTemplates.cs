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

		public int LUNITS;
		public int LTSCALE;
		public int MEASUREMENT;
		public int PDSIZE;

		public string defaultTextStyle;
		public string defaultDimStyle;
		public string defaultTableStyle;
		public string defaultMLeaderStyle;
		public string defaultAnnoScale;

		public List<string> keepDimensionStyles = new List<string>();
		public List<string> keepMLeaderStyles = new List<string>();
		public List<string> keepTableStyles = new List<string>();
		public List<string> keepTextStyles = new List<string>();
		public List<string> keepAnnoScales = new List<string>();

		public Vector3d modelSpaceDisplacement;
	}

	// TODO move hard coded data into data files
	internal class AutoGenTemplates
	{
		readonly List<string> MetricLayouts = new List<string>()
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

		readonly List<string> ImperialLayouts = new List<string>()
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

		readonly string MetricLayoutMarker = "(MET)";
		readonly string ImperialLayoutMarker = "(IMP)";
		readonly string TempLayoutMarker = "(TEMP)";

		readonly string MetricTemplateLayer = "TEMPLATE_METRIC";
		readonly string ImperialTemplateLayer = "TEMPLATE_IMPERIAL";
		readonly string BaseTemplateLayer = "TEMPLATE_BASE";

		// -------------------------------------------------------

		readonly List<string> CommonDimStyles = new List<string>()
		{
			"Standard",
		};

		readonly List<string> MetricDimStyles = new List<string>()
		{
			"IFE-Met Dim (Anno)",
			"IFE-Met Dim 1-010",
			"IFE-Met Dim 1-025",
			"IFE-Met Dim 1-050",
			"IFE-Met Dim 1-100",
		};

		readonly List<string> ImperialDimStyles = new List<string>()
		{
			"IFE-Imp Dim (Anno)",
			"IFE-Imp Dim 1-006",
			"IFE-Imp Dim 1-012",
			"IFE-Imp Dim 1-024",
			"IFE-Imp Dim 1-048",
			"IFE-Imp Dim 1-096",
		};

		readonly string defaultMetricDimStyle = "IFE-Met (Anno)";
		readonly string defaultImperialDimStyle = "IFE-Imp (Anno)";

		// -------------------------------------------------------

		readonly List<string> CommonTextStyles = new List<string>()
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
			"Standard",
		};

		readonly List<string> MetricTextStyles = new List<string>()
		{
			"IFE-Met Hdg (Anno)",
			"IFE-Met Hdg 1-001",
			"IFE-Met Hdg 1-010",
			"IFE-Met Hdg 1-025",
			"IFE-Met Hdg 1-050",
			"IFE-Met Hdg 1-100",
			"IFE-Met Note (Anno)",
			"IFE-Met Note 1-001",
			"IFE-Met Note 1-010",
			"IFE-Met Note 1-025",
			"IFE-Met Note 1-050",
			"IFE-Met Note 1-100",
		};

		readonly List<string> ImperialTextStyles = new List<string>()
		{
			"IFE-Imp Hdg (Anno)",
			"IFE-Imp Hdg 1-001",
			"IFE-Imp Hdg 1-012",
			"IFE-Imp Hdg 1-024",
			"IFE-Imp Hdg 1-048",
			"IFE-Imp Hdg 1-096",
			"IFE-Imp Note (Anno)",
			"IFE-Imp Note 1-001",
			"IFE-Imp Note 1-012",
			"IFE-Imp Note 1-024",
			"IFE-Imp Note 1-048",
			"IFE-Imp Note 1-096",
		};

		readonly string defaultMetricTextStyle = "IFE-Met Note 1-100";
		readonly string defaultImperialTextStyle = "IFE-Imp Note 1-096";

		// -------------------------------------------------------

		readonly List<string> CommonTableStyles = new List<string>()
		{
			"Standard",
		};

		readonly List<string> MetricTableStyles = new List<string>()
		{
			"IFE-Met Device Legend",
			"IFE-Met Sprinkler Legend",
		};

		readonly List<string> ImperialTableStyles = new List<string>()
		{
			"IFE-Imp Device Legend",
			"IFE-Imp Sprinkler Legend",
		};

		readonly string defaultMetricTableStyle = "Standard";
		readonly string defaultImperialTableStyle = "Standard";

		// -------------------------------------------------------

		readonly List<string> CommonMLeaderStyles = new List<string>()
		{
			"Standard",
		};

		readonly List<string> MetricMLeaderStyles = new List<string>()
		{
			"IFE-Met Ldr (Anno)",
			"IFE-Met Ldr 1-010",
			"IFE-Met Ldr 1-025",
			"IFE-Met Ldr 1-050",
			"IFE-Met Ldr 1-100",
		};

		readonly List<string> ImperialMLeaderStyles = new List<string>()
		{
			"IFE-Imp Ldr (Anno)",
			"IFE-Imp Ldr 1-012",
			"IFE-Imp Ldr 1-024",
			"IFE-Imp Ldr 1-048",
			"IFE-Imp Ldr 1-096",
		};

		readonly string defaultMetricMLeaderStyle = "IFE-Met (Anno)";
		readonly string defaultImperialMLeaderStyle = "IFE-Imp (Anno)";

		// -------------------------------------------------------

		readonly List<string> CommonAnnoScales = new List<string>()
		{
			"1:1",
		};

		readonly List<string> MetricAnnoScales = new List<string>()
		{
			"1:1000",
			"1:750",
			"1:500",
			"1:250",
			"1:100",
			"1:50",
			"1:25",
			"1:10",
			"1:1",
		};

		readonly List<string> ImperialAnnoScales = new List<string>()
		{
			"1/64\" = 1'-0\" (1:768)",
			"1/32\" = 1'-0\" (1:384)",
			"1/16\" = 1'-0\" (1:192)",
			"1/8\" = 1'-0\" (1:96)",
			"1/4\" = 1'-0\" (1:48)",
			"1/2\" = 1'-0\" (1:24)",
			"1\" = 1'-0\" (1:12)",
			"2\" = 1'-0\" (1:6)",
			"1:1",
		};

		readonly string defaultMetricAnnoScale = "1:100";
		readonly string defaultImperialAnnoScale = "1/8\" = 1'-0\" (1:96)";

		// -------------------------------------------------------

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

			metricData.LUNITS = 2;
			metricData.LTSCALE = 100;
			metricData.MEASUREMENT = 1;
			metricData.PDSIZE = 50;

			metricData.defaultDimStyle = defaultMetricDimStyle;
			metricData.defaultMLeaderStyle = defaultMetricMLeaderStyle;
			metricData.defaultTableStyle = defaultMetricTableStyle;
			metricData.defaultTextStyle = defaultMetricTextStyle;
			metricData.defaultAnnoScale = defaultMetricAnnoScale;

			metricData.keepDimensionStyles.AddRange(CommonDimStyles);
			metricData.keepDimensionStyles.AddRange(MetricDimStyles);

			metricData.keepMLeaderStyles.AddRange(CommonMLeaderStyles);
			metricData.keepMLeaderStyles.AddRange(MetricMLeaderStyles);

			metricData.keepTableStyles.AddRange(CommonTableStyles);
			metricData.keepTableStyles.AddRange(MetricTableStyles);

			metricData.keepTextStyles.AddRange(CommonTextStyles);
			metricData.keepTextStyles.AddRange(MetricTextStyles);

			metricData.keepAnnoScales.AddRange(CommonAnnoScales);
			metricData.keepAnnoScales.AddRange(MetricAnnoScales);

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

			imperialData.LUNITS = 4;
			imperialData.LTSCALE = 4;
			imperialData.MEASUREMENT = 0;
			imperialData.PDSIZE = 2;

			imperialData.defaultTextStyle = defaultImperialTextStyle;
			imperialData.defaultDimStyle = defaultImperialDimStyle;
			imperialData.defaultTableStyle = defaultImperialTableStyle;
			imperialData.defaultMLeaderStyle = defaultImperialMLeaderStyle;
			imperialData.defaultAnnoScale = defaultImperialAnnoScale;

			imperialData.keepDimensionStyles.AddRange(CommonDimStyles);
			imperialData.keepDimensionStyles.AddRange(ImperialDimStyles);

			imperialData.keepMLeaderStyles.AddRange(CommonMLeaderStyles);
			imperialData.keepMLeaderStyles.AddRange(ImperialMLeaderStyles);

			imperialData.keepTableStyles.AddRange(CommonTableStyles);
			imperialData.keepTableStyles.AddRange(ImperialTableStyles);

			imperialData.keepTextStyles.AddRange(CommonTextStyles);
			imperialData.keepTextStyles.AddRange(ImperialTextStyles);

			imperialData.keepAnnoScales.AddRange(CommonAnnoScales);
			imperialData.keepAnnoScales.AddRange(ImperialAnnoScales);

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

			SetDimStyle(document, transaction, data.defaultDimStyle);
			CleanDimStyles(document, transaction, data.keepDimensionStyles);
			
			SetMLeaderStyle(transaction, data.defaultMLeaderStyle);
			CleanMLeaderStyles(document, transaction, data.keepMLeaderStyles);

			SetTableStyle(transaction, data.defaultTableStyle);
			CleanTableStyles(document, transaction, data.keepTableStyles);

			SetTextStyle(transaction, data.defaultTextStyle);
			CleanTextStyles(document, transaction, data.keepTextStyles);

			SetAnnoScale(transaction, data.defaultAnnoScale);

			CleanAnnoScales(document, transaction, data.keepAnnoScales);

			// Set system variables
			AcApplication.SetSystemVariable("LUNITS", data.LUNITS);
			AcApplication.SetSystemVariable("LTSCALE", data.LTSCALE);
			AcApplication.SetSystemVariable("MEASUREMENT", data.MEASUREMENT);
			AcApplication.SetSystemVariable("PDSIZE", data.PDSIZE);

			// Set tab to model space
			document.Editor.ZoomExtents();
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

		private void CleanDimStyles(Document document, Transaction transaction, List<string> keepDimensionStyles)
		{
			Database database = document.Database;

			DimStyleTable dimStyleTable = transaction.GetObject(database.DimStyleTableId, OpenMode.ForWrite) as DimStyleTable;

			foreach (ObjectId objectId in dimStyleTable)
			{
				DimStyleTableRecord dimStyleTableRecord = transaction.GetObject(objectId, OpenMode.ForRead) as DimStyleTableRecord;

				if (!keepDimensionStyles.Contains(dimStyleTableRecord.Name))
				{
					dimStyleTableRecord.UpgradeOpen();
					dimStyleTableRecord.Erase();
				}
				else
				{
					if (dimStyleTableRecord.Name.Contains("IFE-Met") || dimStyleTableRecord.Name.Contains("IFE-Imp"))
					{
						//dimStyleTableRecord.UpgradeOpen();
						//dimStyleTableRecord.Name = "IFE" + dimStyleTableRecord.Name.Substring(7);
					}
				}
			}
		}

		void SetMLeaderStyle(Transaction transaction, string desiredMLeaderStyle)
		{
			AcApplication.SetSystemVariable("CMLEADERSTYLE", desiredMLeaderStyle);
		}

		private void CleanMLeaderStyles(Document document, Transaction transaction, List<string> keepMLeaderStyles)
		{
			Database database = document.Database;

			DBDictionary MLeaderStyles = transaction.GetObject(database.MLeaderStyleDictionaryId, OpenMode.ForWrite) as DBDictionary;

			foreach (DBDictionaryEntry mLeaderEntry in MLeaderStyles)
			{
				MLeaderStyle mLeaderStyle = transaction.GetObject(mLeaderEntry.Value, OpenMode.ForRead) as MLeaderStyle;

				if (!keepMLeaderStyles.Contains(mLeaderStyle.Name))
				{
					mLeaderStyle.UpgradeOpen();
					mLeaderStyle.Erase();
				}
				else
				{
					if (mLeaderStyle.Name.Contains("IFE-Met") || mLeaderStyle.Name.Contains("IFE-Imp"))
					{
						mLeaderStyle.UpgradeOpen();
						mLeaderStyle.Name = "IFE" + mLeaderStyle.Name.Substring(7);
					}
				}
			}
		}

		void SetTableStyle(Transaction transaction, string desiredTableStyle)
		{
			AcApplication.SetSystemVariable("CTABLESTYLE", desiredTableStyle);
		}

		private void CleanTableStyles(Document document, Transaction transaction, List<string> keepTableStyles)
		{
			Database database = document.Database;

			DBDictionary tableStyles = transaction.GetObject(database.TableStyleDictionaryId, OpenMode.ForWrite) as DBDictionary;

			foreach (DBDictionaryEntry tableEntry in tableStyles)
			{
				TableStyle tableStyle = transaction.GetObject(tableEntry.Value, OpenMode.ForRead) as TableStyle;

				if (!keepTableStyles.Contains(tableStyle.Name))
				{
					tableStyle.UpgradeOpen();
					tableStyle.Erase();
				}
			}
		}

		void SetTextStyle(Transaction transaction, string desiredTextStyle)
		{
			AcApplication.SetSystemVariable("TEXTSTYLE", desiredTextStyle);
		}

		private void CleanTextStyles(Document document, Transaction transaction, List<string> keepTextStyles)
		{
			Database database = document.Database;

			SymbolTable textStyles = transaction.GetObject(database.TextStyleTableId, OpenMode.ForWrite) as SymbolTable;
			
			foreach (ObjectId textEntry in textStyles)
			{
				TextStyleTableRecord textStyle = transaction.GetObject(textEntry, OpenMode.ForRead) as TextStyleTableRecord;

				if (!keepTextStyles.Contains(textStyle.Name))
				{
					textStyle.UpgradeOpen();
					textStyle.Erase();
				}
				else
				{
					if (textStyle.Name.Contains("IFE-Met") || textStyle.Name.Contains("IFE-Imp"))
					{
						textStyle.UpgradeOpen();
						textStyle.Name = "IFE" + textStyle.Name.Substring(7);
					}
				}
			}
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

		void SetAnnoScale(Transaction transaction, string defaultAnnoScale)
		{
			AcApplication.SetSystemVariable("CANNOSCALE", defaultAnnoScale);
		}

		void CleanAnnoScales(Document document, Transaction transaction, List<string> keepAnnoScales)
		{
			ObjectContextManager objectContextManager = document.Database.ObjectContextManager;

			const string annoScalesCollectionName = "ACDB_ANNOTATIONSCALES";

			ObjectContextCollection annoScalesCollection = objectContextManager.GetContextCollection(annoScalesCollectionName);

			if (annoScalesCollection != null)
			{
				List<string> dead = new List<string>();

				foreach (ObjectContext s in annoScalesCollection)
				{
					if (!keepAnnoScales.Contains(s.Name))
					{
						dead.Add(s.Name);
					}
				}

				foreach (string s in dead)
				{
					annoScalesCollection.RemoveContext(s);
				}
			}
			else
			{
				Session.Log("Failed to find ObjectContextCollection " + annoScalesCollectionName);
			}

		}
	}
}
