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
	public class AutoLabelNew
	{		
		[CommandMethod("AutoLabelNew", CommandFlags.UsePickSet | CommandFlags.UsePickSet)]
		public void AutoLabelNewCmd()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = doc.Database;
			Editor editor = doc.Editor;

			using (Transaction transaction = Session.StartTransaction())
			{
				AutoLabelDialog autoLabelDialog = ShowAutoLabelDialog(transaction);

				if (autoLabelDialog.DialogResult == DialogResult.OK)
				{
					editor.WriteMessage("We're OK");
				}
				else
				{
					editor.WriteMessage("We're not OK");
				}

				transaction.Commit();
			}
		}

		protected AutoLabelDialog ShowAutoLabelDialog(Transaction transaction)
		{
			AutoLabelDialog autoLabelDialog = new AutoLabelDialog(transaction);
			AcApplication.ShowModalDialog(null, autoLabelDialog, false);
			return autoLabelDialog;
		}
	}
}
