using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Ironwill.Commands;
using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Template.AutoGenTemplates))]

namespace Ironwill.Template
{
	internal class AutoGenTemplates
	{
		List<string> TemplateLayouts = new List<string>()
		{
			"---------------"
		};

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
			"NOTES (Imp)",
			"ESFR (Imp)",
			"DETAILS (Imp)",
			"11x17 (Imp)",
			"24x36 (Imp)",
			"30x42 (Imp)",
			"36x48 (Imp)",
			"11x17 (H) (Imp)",
			"24x36 (H) (Imp)",
			"30x42 (H) (Imp)",
			"36x48 (H) (Imp)"
		};

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

		string TemplatePath = "C:/Users/KWilcox/Desktop/Test.dwg";

		[CommandMethod("GenTemplates", CommandFlags.Session)]
		public void GenTemplates()
		{
			DocumentCollection documentCollection = Session.GetDocumentManager();

			using (Document document = documentCollection.Open(TemplatePath))
			{
				Database database = document.Database;

				using (DocumentLock documentLock = document.LockDocument())
				{
					using (Transaction transaction = Session.StartTransaction())
					{
						GenMetric(document, transaction);
						database.SaveAs("C:/Users/KWilcox/Desktop/Test_Metric.dwg", DwgVersion.Current);
					}
				}

				document.CloseAndDiscard();
			}

			using (Document document = documentCollection.Open(TemplatePath))
			{
				Database database = document.Database;

				using (DocumentLock documentLock = document.LockDocument())
				{
					using (Transaction transaction = Session.StartTransaction())
					{
						GenImperial(document, transaction);
						database.SaveAs("C:/Users/KWilcox/Desktop/Test_Imperial.dwg", DwgVersion.Current);
					}
				}

				document.CloseAndDiscard();
			}
		}

		void GenMetric(Document document, Transaction transaction)
		{
			// Delete imperial layouts
			EraseLayouts(document, transaction, "Imperial");
			EraseLayouts(document, transaction, "------");

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

			// Purge
		}

		void GenImperial(Document document, Transaction transaction)
		{
			Database database = document.Database;

			// Delete imperial layouts
			EraseLayouts(document, transaction, "Metric");
			EraseLayouts(document, transaction, "------");

			// Cleanly rename all layout tabs
			CleanLayoutNames(document, transaction, "(Imperial)");

			// Delete imperial model space objects // TODO: my dynamic extinguisher blocks' extents are huge, why?
			EraseObjectsWithin(document, transaction, "TEMPLATE_BASE");
			EraseObjectsWithin(document, transaction, "TEMPLATE_METRIC");

			// Erase template layers
			LayerHelper.Delete(document, transaction, "TEMPLATE_IMPERIAL");
			LayerHelper.Delete(document, transaction, "TEMPLATE_METRIC");
			LayerHelper.Delete(document, transaction, "TEMPLATE_BASE");

			// Set current styles
			SetTextStyle(transaction, DefaultImperialTextStyle);
			SetDimStyle(document, transaction, DefaultImperialDimStyle);
			SetTableStyle(transaction, DefaultImperialTableStyle);
			SetMLeaderStyle(transaction, DefaultImperialMLeaderStyle);

			// Scale down the modelspace borders

			// Set other system variables

			// Purge

		}

		void EraseLayouts(Document document, Transaction transaction, string targetString)
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
	}
}
