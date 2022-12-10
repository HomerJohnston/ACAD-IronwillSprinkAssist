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
using DialogResult = System.Windows.Forms.DialogResult;
using Autodesk.AutoCAD.Windows;
using System.Windows.Forms.Integration;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoLabelNew))]

namespace Ironwill.Commands
{
	internal class AutoLabelNew : SprinkAssistCommand
	{		
		[CommandMethod("SpkAssist_AutoLabelNew", CommandFlags.UsePickSet)]
		public void AutoLabelNewCmd()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = doc.Database;
			Editor editor = doc.Editor;

			ShowAutoLabelDialog();
		}

		protected void ShowAutoLabelDialog()
		{
			//AutoLabelDialog autoLabelDialog = new AutoLabelDialog();
			//AcApplication.ShowModelessDialog(null, autoLabelDialog, false);

			var test = new AutoLabelPaletteSet("AutoLabelTest");

			test.Visible = true;
		}

		public static DBDictionary GetPipeGroupsDictionary(Transaction transaction)
		{
			return XRecordLibrary.GetNamedDictionary(transaction, "PipeGroups");
		}

		public static DBDictionary GetPipeGroupAssignmentsDictionary(Transaction transaction)
		{
			return XRecordLibrary.GetNamedDictionary(transaction, "PipeGroupAssignments");
		}

		public static List<string> GetPipeLayers()
		{
			return new List<string>
			{
				Layer.SystemPipe.Get(),
				Layer.SystemPipe_Armover.Get(),
				Layer.SystemPipe_Branchline.Get(),
				Layer.SystemPipe_Main.Get(),
				Layer.SystemPipe_AuxDrain.Get(),
			};
		}

		public static List<string> GetPipeBlocks()
		{
			return new List<string>
			{
				Blocks.Fitting_Elbow.Get(),
				Blocks.Fitting_Riser.Get(),
				Blocks.Fitting_Tee.Get(),
			};
		}
	}

	internal class AutoLabelPaletteSet : PaletteSet
	{
		private Dialogs.PipesPalette uc1;

		public AutoLabelPaletteSet(string name) : base(name, "", new Guid("A9BF7F7D-F6DF-4201-850E-782AFCEF8AFB"))
		{
			Style = PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton | PaletteSetStyles.Snappable;
			Opacity = 100;
			
			Initialize();
		}

		void Initialize()
		{
			uc1 = new Dialogs.PipesPalette();

			AddVisual("Test", uc1);
			Activate(0);
		}
	}
}
