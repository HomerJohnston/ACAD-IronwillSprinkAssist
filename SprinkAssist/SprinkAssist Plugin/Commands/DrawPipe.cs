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

[assembly: CommandClass(typeof(Ironwill.DrawPipe))]

namespace Ironwill
{
	public class DrawPipe
	{
		[CommandMethod("SpkAssist_DrawDraftAid")]
		public void DrawDraftAid()
		{
			SetLayer(Layer.DraftAid.Get());
			Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
		}

		[CommandMethod("SpkAssist_DrawPipeMain")]
		public void DrawPipeMain()
		{
			SetLayer(Layer.SystemPipe_Main.Get());
			Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
		}

		[CommandMethod("SpkAssist_DrawPipeBranchline")]
		public void DrawPipeBranchline()
		{
			SetLayer(Layer.SystemPipe_Branchline.Get());
			Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
		}

		[CommandMethod("SpkAssist_DrawPipeArmover")]
		public void DrawPipeArmover()
		{
			SetLayer(Layer.SystemPipe_Armover.Get());
			Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
		}

		[CommandMethod("SpkAssist_DrawPipeDrain")]
		public void DrawPipeDrain()
		{
			SetLayer(Layer.SystemPipe_AuxDrain.Get());
			Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
		}

		public void SetLayer(string layerName)
		{
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				LayerTable layerTable;
				layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

				if (layerTable.Has(layerName))
				{
					database.Clayer = layerTable[layerName];
					transaction.Commit();
				}
			}
		}
	}
}
