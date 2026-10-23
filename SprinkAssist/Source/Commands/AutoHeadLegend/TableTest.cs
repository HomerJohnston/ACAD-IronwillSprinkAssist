using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClosedXML.Excel;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoHeadLegend.TableTestCmd))]

/*
 * Concept
 * Something needs to hold the data for each head block. Should this be a sample table?
*/


namespace Ironwill.Commands.AutoHeadLegend
{
	internal struct SprinklerMfg
	{
		string name;

		List<SprinklerFamily> families;
	}

	internal struct SprinklerFamily
	{
		string Name;


	}

	internal struct SprinklerModel
	{
		string Name;
	}


	internal class SprinklerEntry
	{
		string Manufacturer;
		string Model;
		string Family;
		string Dry;
		string SIN;
		string Orientation;
		string Response;
		string KFactor;


	}

	internal class TableTestCmd
	{		
		[CommandMethod("CRT")]
		static public void CreateTable()
		{
			ReadSprinklerDatabase();

			Document doc = Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;

			PromptPointResult pr = ed.GetPoint("\nEnter table insertion point: ");

			if (pr.Status == PromptStatus.OK)
			{
				Table tb = new Table();
				tb.TableStyle = db.Tablestyle;
				tb.NumRows = 5;
				tb.NumColumns = 3;
				tb.SetRowHeight(3);
				tb.SetColumnWidth(15);
				tb.Position = pr.Value;

				string[,] str = new string[5, 3];
				str[0, 0] = "Part No.";
				str[0, 1] = "Naasasme ";
				str[0, 2] = "Material ";
				str[1, 0] = "1876-1";
				str[1, 1] = "Flange";
				str[1, 2] = "Perspex";
				str[2, 0] = "0985-4";
				str[2, 1] = "Bolt";
				str[2, 2] = "Steel";
				str[3, 0] = "34asasas76-K";
				str[3, 1] = "Tile";
				str[3, 2] = "Ceramic";
				str[4, 0] = "8734-3";
				str[4, 1] = "Kean";
				str[4, 2] = "Mostly water";

				for (int i = 0; i < 5; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						tb.SetTextHeight(i, j, 1);
						tb.SetTextString(i, j, str[i, j]);
						tb.SetAlignment(i, j, CellAlignment.MiddleCenter);
					}
				}

				tb.GenerateLayout();

				Transaction tr = doc.TransactionManager.StartTransaction();

				using (tr)
				{
					BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);

					BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

					btr.AppendEntity(tb);

					tr.AddNewlyCreatedDBObject(tb, true);

					tr.Commit();
				}
			}
		}

		static public void ReadSprinklerDatabase()
		{
			using var sprinklerDB = new XLWorkbook("D:\\Ironwill Design\\WIP\\SprinklerDatabase.xlsx");

			var worksheet = sprinklerDB.Worksheet("SprinklerDB");
			var usedRange = worksheet.RangeUsed();

			if (usedRange != null)
			{
				foreach (var row in usedRange.RowsUsed().Skip(1)) // Skip header row
				{
					var manufacturer = row.Cell(1).GetString();
					var model = row.Cell(2).GetString();
					var family = row.Cell(3).GetString();
					var dry = row.Cell(4).GetString();
					var sin = row.Cell(5).GetString();
					var orientation = row.Cell(6).GetString();
					var response = row.Cell(7).GetString();
					var kFactor = row.Cell(8).GetString();
					var NPT = row.Cell(9).GetString();
					var TempCSV = row.Cell(10).GetString();
					var DefaultFinish = row.Cell(11).GetString();
					var DefaultEscutcheon = row.Cell(12).GetString();
				
					Session.Log(manufacturer + ", " + model + ", " + family + ", " + dry + ", " + sin + ", " + orientation + ", " + response + ", " + kFactor + ", " + NPT + ", " + TempCSV + ", " + DefaultFinish + ", " + DefaultEscutcheon);
				}
			}
		}

		static protected void GatherHeadCounts()
		{

		}
	}
}
