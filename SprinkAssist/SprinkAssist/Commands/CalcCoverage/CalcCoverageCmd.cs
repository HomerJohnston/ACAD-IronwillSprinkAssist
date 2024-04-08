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
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.CalcCoverage.CalcCoverageCmd))]

namespace Ironwill.Commands.CalcCoverage
{
	public class CalcCoverageCmd
	{
		[CommandDescription("Simple helper to calculate rectangular coverage area from a sprinkler to two points (horizontal/vertical).", "Works in current coordinate space only.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "CalcCoverage", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
		public void Main()
		{
			Document document = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = document.Database;
			Editor editor = document.Editor;

			DrawingUnits drawingUnits = Session.GetPrimaryUnits();

			if (drawingUnits == DrawingUnits.Undefined)
			{
				editor.WriteMessage("Drawing not set to Decimal or Architectural units, aborting!");
				return;
			}

			using (Transaction trans = database.TransactionManager.StartTransaction())
			{
				var centerPointPrompt = new PromptPointOptions(Environment.NewLine + "Pick Centre");
				var horizPointPrompt = new PromptPointOptions(Environment.NewLine + "Pick HORIZONTAL Point");
				var vertPointPrompt = new PromptPointOptions(Environment.NewLine + "Pick VERTICAL Point");

				PromptPointResult CtrPt = editor.GetPoint(centerPointPrompt);
				if (CtrPt.Status != PromptStatus.OK)
					return;

				PromptPointResult FirstPt = editor.GetPoint(horizPointPrompt);
				if (FirstPt.Status != PromptStatus.OK)
					return;

				PromptPointResult SecondPt = editor.GetPoint(vertPointPrompt);
				if (SecondPt.Status != PromptStatus.OK)
					return;

				double CX = CtrPt.Value.X;
				double CY = CtrPt.Value.Y;
				double X1 = FirstPt.Value.X;
				double Y2 = SecondPt.Value.Y;

				double X = Math.Abs(CX - X1);
				double Y = Math.Abs(CY - Y2);

				double Area = X * Y;
				string LUnits = " ";

				switch (drawingUnits)
				{
					case DrawingUnits.Metric:
						X = Math.Round(X / 10) * 10;
						Y = Math.Round(Y / 10) * 10;
						Area /= 1000000;
						LUnits += "m";
						break;
					case DrawingUnits.Imperial:
						X = (Math.Round(X / 12 * 10) / 10);
						Y = (Math.Round(Y / 12 * 10) / 10);
						Area /= 144;
						LUnits += "ft";
						break;
				}

				Area = Math.Round(Area * 100) / 100;

				double Area2 = Area * 2;
				double Area4 = Area * 4;

				AcApplication.ShowAlertDialog("Width: " + X + LUnits + " Height: " + Y + LUnits + "\n" +
											"\nArea Calculations:" +
											"\n-------------------------" +
											"\nTo Sprkrs: " + Area + LUnits + "²" +
											"\n  1 x Wall: " + Area2 + LUnits + "²" +
											"\n  2 x Wall: " + Area4 + LUnits + "²");
			}
		}
	}
}
