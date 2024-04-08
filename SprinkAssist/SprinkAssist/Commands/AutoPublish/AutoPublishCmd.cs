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
using Ironwill.Commands.Help;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


[assembly: CommandClass(typeof(Ironwill.Commands.AutoPublish.AutoPublishCmd))]

namespace Ironwill.Commands.AutoPublish
{
	public class AutoPublishCmd
	{
		string currentPdfFile;

		object CTAB_original;
		object BACKGROUNDPLOT_original;

		readonly string defaultOverrideName = "FP Dwgs - [WHATEVER YOU TYPE HERE] - IssuedFor (Date)";

        // TODO remove hardcoded property strings somehow
        [CommandDescription("Quick-publish PDF of drawings.", "Publishes all layouts starting with 'FP' to a PDF file named according to the ProjectName_1 file property.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoPublish", CommandFlags.NoBlockEditor | CommandFlags.Modal | CommandFlags.NoHistory | CommandFlags.NoUndoMarker)]
		public void Main()
		{
			Session.Log("Running AutoPublish - Make sure your drawing has been saved to pick up any tab changes.");

			StoreSystemVariables();

            CheckSystemPlotVariables();

            // Get all layout names
            List<string> layouts = GetLayouts();

			if (layouts.Count == 0)
			{
				Session.Log("Warning: No layouts found that start with FP, aborting!");
				return;
			}

            string overrideName = Session.GetDatabase().GetCustomProperty("PlotFilename");
            string revisionString = Session.GetDatabase().GetCustomProperty("PlotRevision");

            string projectName;

            if (overrideName != null && overrideName != defaultOverrideName && overrideName != string.Empty)
            {
				projectName = overrideName;
            }
			else
			{
                projectName = Session.GetDatabase().GetCustomProperty("ProjectName_1");
            }

			if (revisionString != null && revisionString != string.Empty)
			{
				projectName += " - Rev " + revisionString;
            }

            string dwgName = Session.GetDocument().Name;
			string dwgDirectory = Path.GetDirectoryName(dwgName);
			string destinationPath = dwgDirectory;

			string plotFileNameBase = "FP Dwgs";

			string issuedFor = Session.GetDatabase().GetCustomProperty("IssuedFor");
			string issuedForAbbrev = "IF" + new string(issuedFor.Split(' ').Select(s => s[0]).ToArray()).ToUpper();

			string dateAsString = "(" + DateTime.Now.ToString(@"yyyy-MM-dd") + ")";

			string pdfFileName = plotFileNameBase;
			pdfFileName = string.Join(" - ", plotFileNameBase, projectName, issuedForAbbrev);
			pdfFileName = string.Join(" ", pdfFileName, dateAsString);

			foreach (char c in System.IO.Path.GetInvalidFileNameChars())
			{
				pdfFileName = pdfFileName.Replace(c.ToString(), string.Empty);
			}

			pdfFileName = pdfFileName.Replace(".", string.Empty);

			pdfFileName = Path.ChangeExtension(pdfFileName, "pdf");

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
				entry.Title = layoutName; // TODO read the page title from the titleblock
				dsdEntryCollection.Add(entry);
			}

			DsdData dsdData = new DsdData();
			dsdData.SetDsdEntryCollection(dsdEntryCollection);
			dsdData.ProjectPath = destinationPath;
			dsdData.LogFilePath = Path.Combine(destinationPath, destinationLogName);
			dsdData.SheetType = SheetType.MultiPdf;
			dsdData.NoOfCopies = 1;
			dsdData.DestinationName = Path.Combine(destinationPath, pdfFileName);
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
				progressDialog.set_PlotMsgString(PlotMessageIndex.SheetProgressCaption, "Sheet progress");

				progressDialog.UpperPlotProgressRange = 100;
				progressDialog.LowerPlotProgressRange = 0;

				progressDialog.UpperSheetProgressRange = 100;
				progressDialog.LowerSheetProgressRange = 0;

				progressDialog.IsVisible = false;
				
				Publisher publisher = Application.Publisher;
				
				PlotConfigManager.SetCurrentConfig("DWG to PDF.pc3");
				
				publisher.EndPublish += EndPublishEventHandler;
				
				Session.Log("Starting publish...");
				publisher.PublishExecute(dsdData, PlotConfigManager.CurrentConfig);
			}

			RestoreSystemVariables();
		}

		void StoreSystemVariables()
		{
            CTAB_original = AcApplication.GetSystemVariable("CTAB");
            BACKGROUNDPLOT_original = AcApplication.GetSystemVariable("BACKGROUNDPLOT");
            AcApplication.SetSystemVariable("BACKGROUNDPLOT", 0);
        }

		void RestoreSystemVariables()
		{
            AcApplication.SetSystemVariable("CTAB", CTAB_original);
            AcApplication.SetSystemVariable("BACKGROUNDPLOT", BACKGROUNDPLOT_original);
        }

		void CheckSystemPlotVariables()
		{
            int PDFSHX = System.Convert.ToInt32(AcApplication.GetSystemVariable("PDFSHX"));

            if (PDFSHX > 0)
            {
                Session.Log("Warning: PDFSHX was 1, setting to 0!");
                AcApplication.SetSystemVariable("PDFSHX", 0);
            }
        }

		void EndPublishEventHandler(object sender, PublishEventArgs e)
		{
			Session.Log("Publish ending... Opening file");
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

					if (!IsValidLayoutName(layoutName))
					{
						continue;
					}

					layouts.Add(layoutName);
				}
			}

			return layouts;
		}

		bool IsValidLayoutName(string layoutName)
		{
			if (layoutName == String.Empty)
			{
				return false;
			}

			return layoutName.StartsWith("FP");
		}
	}
}
