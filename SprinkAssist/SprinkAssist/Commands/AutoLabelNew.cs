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

[assembly: CommandClass(typeof(Ironwill.AutoLabelNew))]

namespace Ironwill
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
			AutoLabelDialog autoLabelDialog = new AutoLabelDialog();
			AcApplication.ShowModelessDialog(null, autoLabelDialog, false);
		}
	}
}
