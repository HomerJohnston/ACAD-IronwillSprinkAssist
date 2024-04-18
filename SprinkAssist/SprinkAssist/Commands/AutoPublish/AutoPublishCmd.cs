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

using Ironwill.Commands.Help;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoPublish.AutoPublishCmd))]

namespace Ironwill.Commands.AutoPublish
{
	enum ELayoutSortMethod
	{
		Default,		// AutoCAD default - as of this writing, it defaults to alphabetical sort
		Alphabetical,	// 
		TabOrder,		// Keeps the layout list in the same order as the dwg file shows them
	}

	internal class AutoPublishCmd : SprinkAssistCommand
	{
		string currentPdfFile;

		object CTAB_original;
		object BACKGROUNDPLOT_original;

		readonly string defaultOverrideName = "FP Dwgs - [WHATEVER YOU TYPE HERE] - IssuedFor (Date)";

		CommandSetting<bool> useAutoSheetNumbering;

		public AutoPublishCmd()
		{
			useAutoSheetNumbering = settings.RegisterNew("UseAutoSheetNumbering", true);
		}

		[CommandDescription("Choose whether Auto Publish command automatically renumbers sheets or not.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoPublishUseAutoSheetNumbering", CommandFlags.NoBlockEditor | CommandFlags.Modal)]
		public void SetUseAutoSheetNumbering()
		{
			string currentValue = useAutoSheetNumbering.Get() ? "Yes" : "No";
			PromptKeywordOptions promptKeywordOptions = new PromptKeywordOptions($"Automatically renumber sheets before publishing PDFs? <{currentValue}>");
			promptKeywordOptions.AppendKeywordsToMessage = true;
			promptKeywordOptions.Keywords.Add("Yes");
			promptKeywordOptions.Keywords.Add("No");

			PromptResult promptResult = null;
			
			bool repeatPrompt = true;

			while (repeatPrompt)
			{
				promptResult = Session.GetEditor().GetKeywords(promptKeywordOptions);

				switch (promptResult.Status)
				{
					case PromptStatus.OK:
						{
							string keyword = promptResult.StringResult;

							bool result;

							if (keyword == "Yes")
							{
								result = true;
								repeatPrompt = false;
							}
							else if (keyword == "No")
							{
								result = false;
								repeatPrompt = false;
							}
							else
							{
								Session.Log("Requires Y or N, or escape to cancel.");
								break;
							}

							useAutoSheetNumbering.Set(result);

							break;
						}
					case PromptStatus.Cancel:
						{
							repeatPrompt = false;
							break;
						}
					default:
						{
							Session.Log("Requires Y or N, or escape to cancel.");
							break;
						}
				}
			}
		}

        // TODO remove hardcoded property strings somehow`
        [CommandDescription("Quick-publish PDF of drawings.", "Publishes all layouts starting with 'FP' to a PDF file named according to the ProjectName_1 file property.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoPublish", CommandFlags.NoBlockEditor | CommandFlags.Modal | CommandFlags.NoHistory | CommandFlags.NoUndoMarker)]
		public void Main()
		{
			Session.Log("Running AutoPublish - Make sure your drawing has been saved to pick up any tab changes.");

			StoreSystemVariables();

            CheckSystemPlotVariables();

			if (useAutoSheetNumbering.Get())
			{
				AutoSetSheetNumbers();
			}

            // Get all layout names
            List<string> layouts = GetLayoutNames(ELayoutSortMethod.TabOrder);

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

		List<string> GetLayoutNames(ELayoutSortMethod sortMethod)
		{
			List<string> layoutNames = new List<string>();

			List<Layout> layouts = GetLayouts(sortMethod);
			
			foreach (Layout layout in layouts)
			{
				layoutNames.Add(layout.LayoutName);
			}

			return layoutNames;
		}


		List<Layout> GetLayouts(ELayoutSortMethod sortMethod)
		{
			List<Layout> layouts = new List<Layout>();

			using (Transaction transaction = Session.StartTransaction())
			{
				Database db = Session.GetDatabase();
				DBDictionary layoutDictionary = transaction.GetObject(db.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

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

					layouts.Add(layout);
				}

				switch (sortMethod)
				{
					case ELayoutSortMethod.TabOrder:
						{
							layouts.Sort((x, y) => { return x.TabOrder - y.TabOrder; });
							break;
						}
					case ELayoutSortMethod.Alphabetical:
						{
							layouts.Sort((x, y) => { return x.LayoutName.CompareTo(y.LayoutName); });
							break;
						}
					default:
						{
							break;
						}
				}

				transaction.Commit();
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

		[CommandDescription("Automatic layout sheet numbering.", "Sets the X/Y numbers of all FP___ sheets automatically.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoSheetNumbers", CommandFlags.NoBlockEditor | CommandFlags.Modal)]
		public void AutoSetSheetNumbers()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;
			Database db = doc.Database;

			string blockname = "_TitleblockDwgNo";

			Dictionary<string, int> layoutIndices = GetLayoutIndices();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = (BlockTable)transaction.GetObject(db.BlockTableId, OpenMode.ForRead);

				foreach (ObjectId blockID in blockTable)
				{
					BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockID, OpenMode.ForRead);

					if (!blockTableRecord.IsLayout && !blockTableRecord.IsAnonymous)
					{
						if (blockTableRecord.Name.ToUpper() == blockname.ToUpper())
						{
							ObjectIdCollection allBlocks = blockTableRecord.GetBlockReferenceIds(true, false);

							foreach (ObjectId blockReferenceId in allBlocks)
							{
								BlockReference blockReference = (BlockReference)transaction.GetObject(blockReferenceId, OpenMode.ForRead);

								BlockTableRecord layoutBTR = (BlockTableRecord)transaction.GetObject(blockReference.OwnerId, OpenMode.ForRead);

								if (!layoutBTR.IsLayout)
								{
									continue;
								}

								Layout layout = (Layout)transaction.GetObject(layoutBTR.LayoutId, OpenMode.ForRead);

								Session.Log($"Found {blockname} on layout {layout.LayoutName}");

								int index;

								if (layoutIndices.TryGetValue(layout.LayoutName, out index))
								{
									// I first set them to some "null" value inside a nested transaction. This is to work around an AutoCAD 'bug' where the values won't change if they already exist as the same value.
									using (Transaction dummy = Session.StartTransaction())
									{
										BlockOps.SetBlockAttribute(dummy, blockReference, "X", "-");
										BlockOps.SetBlockAttribute(dummy, blockReference, "Y", "-");
										dummy.Commit();
									}

									BlockOps.SetBlockAttribute(transaction, blockReference, "X", index.ToString());
									BlockOps.SetBlockAttribute(transaction, blockReference, "Y", layoutIndices.Count.ToString());
								}
							}
						}
					}
				}

				transaction.Commit();

				return;
			}
		}

		public Dictionary<string, int> GetLayoutIndices()
		{
			Dictionary<string, int> layoutIndices = new Dictionary<string, int>();

			Database db = Session.GetDatabase();

			int index = 1;

			List<Layout> layouts = GetLayouts(ELayoutSortMethod.TabOrder);

			foreach (Layout layout in layouts)
			{
				layoutIndices.Add(layout.LayoutName, index++);
			}

			return layoutIndices;
		}
	}
}
