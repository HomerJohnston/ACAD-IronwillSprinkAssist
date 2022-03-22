using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Publishing;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;


[assembly: CommandClass(typeof(Ironwill.AutoPublish))]

namespace Ironwill
{
	public class AutoPublish
	{
		string currentPdfFile;

		[CommandMethod("SpkAssist_AutoPublish")]
		public void AutoPublishCmd()
		{
			Session.WriteMessage("Running AutoPublish - Make sure your drawing has been saved to pick up any tab changes.");

			// Store the current tab, we'll need to return to it when we're done
			var ctab = Application.GetSystemVariable("CTAB");
			
			// Get all layout names
			List<string> layouts = GetLayouts();

			if (layouts.Count == 0)
			{
				Session.WriteMessage("No layouts found that start with FP!");
				return;
			}

			string dwgName = Session.GetDocument().Name;
			string dwgDirectory = Path.GetDirectoryName(dwgName);
			
			string destinationPath = dwgDirectory;
			
			string destinationFileName = Path.GetFileName(dwgName);
			destinationFileName = Path.ChangeExtension(destinationFileName, "pdf");

			string destinationLogName = Path.GetFileName(dwgName);
			destinationLogName = Path.ChangeExtension(destinationLogName, "log");

			// TODO fix hardcoding of folder names
			if (dwgDirectory.EndsWith("3. Design"))
			{
				destinationPath = Path.Combine(dwgDirectory, "..", "5. Submittals");
			}

			DsdEntryCollection dsdEntryCollection = new DsdEntryCollection();

			foreach (string layoutName in layouts)
			{
				DsdEntry entry = new DsdEntry();

				entry.Layout = layoutName;
				entry.DwgName = dwgName;
				//entry.Nps = "Setup1";
				entry.Title = layoutName; // TODO read the page title from the titleblock
				dsdEntryCollection.Add(entry);
			}

			DsdData dsdData = new DsdData();
			dsdData.SetDsdEntryCollection(dsdEntryCollection);
			dsdData.ProjectPath = destinationPath;
			dsdData.LogFilePath = Path.Combine(destinationPath, destinationLogName);
			dsdData.SheetType = SheetType.MultiPdf;
			dsdData.NoOfCopies = 1;
			dsdData.DestinationName = Path.Combine(destinationPath, destinationFileName);
			dsdData.PromptForDwfName = false;
			//dsdData.SheetSetName = "PublisherSet"; // TODO what is this? 
			
			int nbSheets = dsdEntryCollection.Count;

			currentPdfFile = dsdData.DestinationName;

			using (PlotProgressDialog progressDialog = new PlotProgressDialog(false, nbSheets, true))
			{
				progressDialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "AutoPublish FP Drawings");
				progressDialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Cancel publishing");
				progressDialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Cancel this sheet");
				progressDialog.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Publish progress");
				progressDialog.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet progrees");

				progressDialog.UpperPlotProgressRange = 100;
				progressDialog.LowerPlotProgressRange = 0;

				progressDialog.UpperSheetProgressRange = 100;
				progressDialog.LowerSheetProgressRange = 0;

				progressDialog.IsVisible = false;
				
				Publisher publisher = Application.Publisher;
				
				PlotConfigManager.SetCurrentConfig("DWG to PDF.pc3");
				
				publisher.EndPublish += EndPublishEventHandler;
				publisher.PublishExecute(dsdData, PlotConfigManager.CurrentConfig);
				publisher.EndPublish -= EndPublishEventHandler;
			}

			AcApplication.SetSystemVariable("CTAB", ctab);
		}

		void EndPublishEventHandler(object sender, PublishEventArgs e)
		{
			Session.WriteMessage("Opening...");
			System.Diagnostics.Process.Start(currentPdfFile);
		}

		List<string> GetLayouts()
		{
			List<string> layouts = new List<string>();

			using (Transaction transaction = Session.StartTransaction())
			{
				Database db = Session.GetDatabase();
				DBDictionary layoutDictionary = transaction.GetObject(db.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

				if (layoutDictionary == null)
				{
					return layouts;
				}

				foreach (DBDictionaryEntry layoutEntry in layoutDictionary)
				{
					ObjectId layoutId = layoutEntry.Value;

					Layout layout = transaction.GetObject(layoutId, OpenMode.ForRead) as Layout;

					if (layout == null)
					{
						continue;
					}

					string layoutName = layout.LayoutName;

					if (!ValidLayoutName(layoutName))
					{
						continue;
					}

					layouts.Add(layoutName);
				}
			}

			return layouts;
		}

		bool ValidLayoutName(string layoutName)
		{
			if (layoutName == String.Empty)
			{
				return false;
			}

			return layoutName.StartsWith("FP");
		}
	}
}
