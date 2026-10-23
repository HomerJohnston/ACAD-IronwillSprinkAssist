using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using Ironwill.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using Region = Autodesk.AutoCAD.DatabaseServices.Region;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoHeadLegendCmd))]

namespace Ironwill.Commands
{
	internal class AutoHeadLegendCmd : SprinkAssistCommand
	{
		// ----------------------------------------------------------------------------------------

		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoHeadLegend", /*CommandFlags.UsePickSet | CommandFlags.Modal |*/ CommandFlags.NoBlockEditor)]
		public void Main()
		{
			Polyline? countArea = GetCountArea();

			if (countArea == null)
			{
				Session.Log("Command failed!");
				return;
			}

			List<BlockReference> sprinklerHeads = GetHeadBlocks(countArea);

			Session.Log("Sprinkler heads found: " + sprinklerHeads.Count);

			AutoHeadTableGenerator.Generate(sprinklerHeads);
		}

		// ----------------------------------------------------------------------------------------

		static private Polyline? GetCountArea()
		{
			TypedValue[] filter = new TypedValue[]
			{
				new TypedValue((int)DxfCode.Start, "LWPOLYLINE"),
				new TypedValue((int)DxfCode.LayerName, "0-CountArea") // TODO HARD CODED LAYER NAME
			};

			SelectionFilter selectionFilter = new SelectionFilter(filter);

			PromptSelectionResult result = Session.GetEditor().SelectAll(selectionFilter);

			List<Polyline> polylines = new();

			if (result.Status == PromptStatus.OK)
			{
				SelectionSet selectionSet = result.Value;

				polylines.EnsureCapacity(selectionSet.Count);

				using (Transaction tr = Session.GetDatabase().TransactionManager.StartTransaction())
				{
					foreach (SelectedObject selectedObject in selectionSet)
					{
						Polyline? polyline = tr.GetObject(selectedObject.ObjectId, OpenMode.ForRead) as Polyline;

						if (polyline != null)
						{
							polylines.Add(polyline);
						}
					}
				}
			}

			if (polylines.Count != 1)
			{
				Session.Log("Error: the drawing must contain a single polyline on the 0-CountArea layer!"); // TODO HARD CODED LAYER NAME

				return null;
			}
			
			return polylines[0];
		}

		// ----------------------------------------------------------------------------------------

		static private List<BlockReference> GetHeadBlocks(Polyline countArea)
		{
			// Grab count area polyline boundary
			List<Brep> allBoundaryBreps = new List<Brep>();

			if (!GetBreps(countArea, ref allBoundaryBreps))
			{
				Session.Log("Warning: could not find any boundary objects, aborting!");
				return [];
			}

			// Grab all head blocks in model space
			PromptSelectionResult selectionResult = GetHeadBlocksAsSelection();

			if (selectionResult.Status != PromptStatus.OK)
			{
				Session.Log("Warning: could not find any sprinkler head blocks, aborting!");
				return [];
			}


			List<BlockReference> sprinklerHeads = new List<BlockReference>();
			sprinklerHeads.EnsureCapacity(selectionResult.Value.Count);

			// Iterate through the head blocks and stash ones which are within the boundary polyline
			using (Transaction tr = Session.GetDatabase().TransactionManager.StartTransaction())
			{
				foreach (SelectedObject headObject in selectionResult.Value)
				{
					BlockReference? headBlock = tr.GetObject(headObject.ObjectId, OpenMode.ForRead) as BlockReference;

					if (headBlock == null)
					{
						continue;
					}

					if (!HeadInBoundary(headBlock, allBoundaryBreps))
					{
						continue;
					}

					sprinklerHeads.Add(headBlock);
				}
			}

			return sprinklerHeads;
		}

		// ----------------------------------------------------------------------------------------

		static private bool GetBreps(Polyline countArea, ref List<Brep> allBoundaryBreps)
		{
			List<Region> boundaryRegions = new List<Region>();

			DBObjectCollection curves = [countArea];

			using (DBObjectCollection regions = Region.CreateFromCurves(curves))
			{
				if (regions == null || regions.Count == 0)
				{
					Session.Log("Failed to create regions from a boundary shape, ignoring");
					return false;
				}

				if (regions.Count > 1)
				{
					Session.Log("Multiple regions created for one curve (invalid geometry?), ignoring");
					return false;
				}

				boundaryRegions.Add(regions.Cast<Region>().First());
			}

			foreach (Region region in boundaryRegions)
			{
				allBoundaryBreps.Add(new Brep(region));
			}

			return allBoundaryBreps.Count == 1;
		}

		// ----------------------------------------------------------------------------------------

		static private PromptSelectionResult GetHeadBlocksAsSelection()
		{
			List<BlockReference> sprinklerHeads = new List<BlockReference>();

			TypedValue[] filter = new TypedValue[]
			{
				new TypedValue((int)DxfCode.Operator, "<AND"),
				new TypedValue((int)DxfCode.Start, "INSERT"),
				new TypedValue((int)DxfCode.BlockName, "*"),
				new TypedValue((int)DxfCode.LayerName, Layer.SystemHead.Get()), // TODO HARD CODED LAYER NAME
				new TypedValue((int)DxfCode.Operator, "AND>"),
			};

			SelectionFilter selectionFilter = new SelectionFilter(filter);

			PromptSelectionResult selectionResult = Session.GetEditor().SelectAll(selectionFilter);

			return selectionResult;
		}

		// ----------------------------------------------------------------------------------------

		static private bool HeadInBoundary(BlockReference block, List<Brep> boundaries)
		{
			foreach (Brep brep in boundaries)
			{
				PointContainment containmentResult;

				bool brepContainmentResult = true;

				using (BrepEntity brepEntity = brep.GetPointContainment(block.Position, out containmentResult))
				{
					if (!(brepEntity is Autodesk.AutoCAD.BoundaryRepresentation.Face))
					{
						brepContainmentResult = false;
						return false;
					}
				}
			}

			return true;
		}
	}

	internal class AutoHeadTableGenerator
	{
		static public void Generate(List<BlockReference> sprinklerHeads)
		{
			//sprinklerHeads.Sort()

			Dictionary<string, int> sprinklerCounts = [];

			foreach (BlockReference sprinklerHead in sprinklerHeads)
			{
				ref int count = ref CollectionsMarshal.GetValueRefOrAddDefault(sprinklerCounts, sprinklerHead.Name, out bool exists);

				if (exists)
				{
					count++;
				}
				else
				{
					count = 1;
				}
			}

			Document doc = Session.GetDocument();
			Transaction tr = doc.TransactionManager.StartTransaction();

			using (tr)
			{
				Table table = new Table();

				TableStyle tableStyle = GetTableStyle();

				table.TableStyle = tableStyle.Id;

				const int baseRows = 3;
				const int baseColumns = 13;

				table.SetSize(baseRows + sprinklerCounts.Count, baseColumns);

				SetupTableBase(table, tr);

				SetupTableEntries(table, sprinklerHeads, tr);

				table.GenerateLayout();

				BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
				BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

				btr.AppendEntity(table);
				tr.AddNewlyCreatedDBObject(table, true);
				tr.Commit();
			}
		}

		static private TableStyle GetTableStyle()
		{
			string tableStyleName = (Session.GetPrimaryUnits() == DrawingUnits.Imperial) ? "IFE-Imp Sprinkler Legend" : "IFE-Met Sprinkler Legend";
			
			using (Transaction tr = Session.StartTransaction())
			{
				DBDictionary tableStyles = (DBDictionary)tr.GetObject(Session.GetDatabase().TableStyleDictionaryId, OpenMode.ForRead);

				ObjectId styleId;

				try
				{
					styleId = tableStyles.GetAt(tableStyleName);
				}
				catch
				{
					styleId = Session.GetDatabase().Tablestyle;
				}

				return (TableStyle)tr.GetObject(styleId, OpenMode.ForRead);
			}
		}

		static private void SetupTableBase(Table table, Transaction tr)
		{
			int titleRow = table.Rows.Count - 1;
			int headerRow = table.Rows.Count - 2;

			CellRange cells = table.Cells;

			table.UnmergeCells(cells);

			table.MergeCells(CellRange.Create(table, 0, 2, 0, 12));
			table.MergeCells(CellRange.Create(table, titleRow, 0, titleRow, 12));

			cells[0, 0].TextString = "TOTAL:";

			cells[0, 1].TextString = "-";

			cells[0, 2].TextString = "SPRINKLERS/NOZZLES (ENTIRE PROJECT)";

			cells[titleRow, 0].TextString = "SPRINKLER LEGEND";
			cells[titleRow, 0].TextStyleId = GetTitleTextStyleId();
			cells[titleRow, 0].TextHeight = 1.5 * cells[titleRow, 0].TextHeight;

			cells[headerRow, 0].TextString = "SYMB";
			cells[headerRow, 1].TextString = "QTY";
			cells[headerRow, 2].TextString = "ORIENT";
			cells[headerRow, 3].TextString = "MODEL";
			cells[headerRow, 4].TextString = "TEMP";
			cells[headerRow, 5].TextString = "FINISH";
			cells[headerRow, 6].TextString = "KFACT";
			cells[headerRow, 7].TextString = "NPT";
			cells[headerRow, 8].TextString = "MFG";
			cells[headerRow, 9].TextString = "SIN";
			cells[headerRow, 10].TextString = "RESP";
			cells[headerRow, 11].TextString = "ESCUTCH";
			cells[headerRow, 12].TextString = "NOTE";

			ObjectId x = Session.GetBlockTable(tr)["S_Head04"];

			DBObject obj = tr.GetObject(x, OpenMode.ForRead);

			cells[1, 0].BlockTableRecordId = x;
		}

		static private void SetupTableEntries(Table table, List<BlockReference> sprinklerBlocks, Transaction tr)
		{
			CellRange cells = table.Cells;

			Dictionary<BlockReference, int> counts;
			
			for (int i = 0; i < sprinklerBlocks.Count; i++)
			{
				BlockReference sprinkler = sprinklerBlocks[i];


				int row = i + 1;

				cells[row, 0].BlockTableRecordId = sprinkler.BlockId;
			}


			for (int i = 0; i < sprinklerBlocks.Count; i++)
			{
				BlockReference sprinkler = sprinklerBlocks[i];

				int row = i + 1;

				cells[row, 0].BlockTableRecordId = sprinkler.BlockId;
			}
		}

		static private ObjectId GetTitleTextStyleId()
		{
			string textStyleName = (Session.GetPrimaryUnits() == DrawingUnits.Imperial) ? "IFE-Imp Hdg 1-001" : "IFE-Met Hdg 1-001";

			using (Transaction tr = Session.StartTransaction())
			{
				TextStyleTable textStyles = (TextStyleTable)tr.GetObject(Session.GetDatabase().TextStyleTableId, OpenMode.ForRead);

				ObjectId styleId;

				try
				{
					styleId = textStyles[textStyleName];
				}
				catch
				{
					styleId = Session.GetDatabase().Textstyle;
				}

				return styleId;
			}
		}

		static private ObjectId GetNormalTextStyleId()
		{
			string textStyleName = (Session.GetPrimaryUnits() == DrawingUnits.Imperial) ? "IFE-Imp Note 1-001" : "IFE-Met Note 1-001";

			using (Transaction tr = Session.StartTransaction())
			{
				TextStyleTable textStyles = (TextStyleTable)tr.GetObject(Session.GetDatabase().TextStyleTableId, OpenMode.ForRead);

				ObjectId styleId;

				try
				{
					styleId = textStyles[textStyleName];
				}
				catch
				{
					styleId = Session.GetDatabase().Textstyle;
				}

				return styleId;
			}
		}
	}
}
