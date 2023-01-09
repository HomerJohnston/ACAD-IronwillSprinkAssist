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

using Autodesk.AutoCAD.Colors;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoCleanBackground))]

namespace Ironwill.Commands
{
	public class AutoCleanBackground
	{
		class CleanupEntry
		{
			public List<string> matchExact = new List<string>();
			public List<string> matchStart = new List<string>();
			public List<string> matchAny = new List<string>();
			public List<string> matchEnd = new List<string>();

			// Utility --------------------------
			private bool _erase = false;
			public bool erase
			{
				get { return _erase; }
				set { _erase = value; }
			}

			// Color ----------------------------
			private bool _setsColor = false;
			public bool setsColor
			{
				get { return _setsColor; }
				private set { _setsColor = value; }
			}

			private short _color = 0;
			public short color
			{
				get { return _color; }
				set { _color = value; _setsColor = true; }
			}

			// Linetype -------------------------
			private bool _setsLinetype = false;
			public bool setsLinetype
			{
				get { return _setsLinetype; }
				private set { _setsLinetype = value; }
			}

			private string _linetype = string.Empty;
			public string linetype
			{
				get { return _linetype; }
				set { _linetype = value; _setsLinetype = true; }
			}

			// Linetype Scale -------------------
			private bool _setsLinetypeScale = false;
			public bool setsLinetypeScale
			{
				get { return _setsLinetypeScale; }
				private set { _setsLinetypeScale = value; }
			}

			private double _linetypeScale = 1.0;
			public double linetypeScale
			{
				get { return _linetypeScale; }
				set { _linetypeScale = value; _setsLinetypeScale = true; }
			}

			// Plot Style -----------------------
			private bool _setsPlotStyleName = false;
			public bool setsPlotStyleName
			{
				get { return _setsPlotStyleName; }
				private set { _setsPlotStyleName = value; }
			}

			private string _plotStyleName = string.Empty;
			public string plotStyleName
			{
				get { return _plotStyleName; }
				set { _plotStyleName = value; setsPlotStyleName = true; }
			}

			// API ------------------------------
			public bool Matches(string layerName)
			{
				foreach (string exact in matchExact)
				{
					if (layerName.ToUpper() == exact.ToUpper())
					{
						return true;
					}
				}

				foreach (string startsWith in matchStart)
				{
					if (layerName.ToUpper().StartsWith(startsWith))
					{
						return true;
					}
				}

				foreach (string contains in matchAny)
				{
					if (layerName.ToUpper().Contains(contains))
					{
						return true;
					}
				}

				foreach (string endsWith in matchEnd)
				{
					if (layerName.ToUpper().EndsWith(endsWith))
					{
						return true;
					}
				}

				return false;
			}
		}

		[CommandMethod("SpkAssist", "AutoCleanBackground", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
		public void AutoCleanBackgroundCmd()
		{
			// TODO: make this data driven instead of hard coded
			// Start with the important ones, work towards less important ones near the end. Helps make it easier to ensure we don't erase anything important.
			List<CleanupEntry> layerCleanupEntries = new List<CleanupEntry>
			{
				new CleanupEntry()
				{
					matchStart = { Layer.Pfix } // Do nothing if it's a SprinkAssist layer
				},
				new CleanupEntry()
				{
					matchEnd = { "GRID-IDEN", "GRID-ID" },
					color = Colors.DarkGrey,
					linetype = Linetypes.Continous
				},
				new CleanupEntry()
				{
					matchAny = { "GRID" },
					color = Colors.DarkGrey,
					linetype = Linetypes.Center,
					linetypeScale = 25
				},
				new CleanupEntry() {
					matchExact = { "A-ANNO-NOTE", "A-ANNO-TEXT", "G-ANNO-TEXT" },
					matchAny = { "FLOR-IDEN", "AREA-IDEN" },
					matchEnd = { "CLNG-IDEN", "CLG-IDEN", "CLNG-ID", "CLG-ID" },
					color = Colors.White,
				},
				new CleanupEntry() {
					matchAny = { "CLNG", "CLG" },
					color = Colors.DarkGrey
				},
				new CleanupEntry() {
					matchExact = { "A-ANNO-DOOR", "A-ANNO-WALL" },
					matchStart = { "A-ANNO-DIMS" },
					matchEnd = { "IDEN", "IDEN-1", "IDEN-2", "IDEN-3", "IDEN-4", "ID" },
					matchAny = { "ELEV", "ANNO-SYMB", "REV" },
					erase = true
				},
				new CleanupEntry() {
					matchEnd = { "WALL-PATT", "WALL-PATTERN" },
					color = Colors.DarkGrey
				},
				new CleanupEntry() {
					matchStart = { "A-WALL" },
					matchExact = { "WALL" },
					matchEnd = { "A-WALL", "COLS" },
					color = Colors.Green
				},
				new CleanupEntry() {
					matchAny = { "WALL", "COLS" },
					color = Colors.LightGreen
				},
				new CleanupEntry() {
					matchEnd = { "GENM", "GENF" },
					erase = true
				},
				new CleanupEntry() {
					matchAny = { "A-DET", "GLAZ", "ROOF", "DOOR", "STAIR", "STRS", "SANR", "CASE", "PFIX", "EQPM", "A-MECH", "FIXTURE", "FXTR" },
					color = Colors.DarkGrey,
				},
				new CleanupEntry() {
					matchAny = { "JOIST" },
					color = Colors.DarkGrey,
					linetype = Linetypes.Dashed,
					linetypeScale = 25
				},
				new CleanupEntry() {
					matchAny = { "STEEL", "BEAM" },
					color = Colors.DarkGrey,
					linetype = Linetypes.Continous,
				},
				new CleanupEntry() {
					matchEnd = { "DIFF" },
					color = Colors.White
				},
				new CleanupEntry() {
					matchEnd = { "HVAC" },
					color = Colors.Cyan
				},
				new CleanupEntry() {
					matchAny = { "LITE" }, // A-EQPM used for stoves
					color = Colors.White
				},
				new CleanupEntry() {
					matchAny = { "A-AREA", "FURN", "EQPM", "SPCQ", "RADON" },
					erase = true
				},
				new CleanupEntry() {
					matchAny = { "POWR", "POWER", "SWCH" },
					erase = true
				}
			};

			Document document = Session.GetDocument();
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				Session.Command("_LtScale", "1");
				Session.Command("_MsLtScale", "1");
				Session.Command("_CAnnoScale", "1:1");
				Session.Command("_Measurement", "0");

				SelectionSet ss = Session.GetEditor().SelectAll().Value;
				Session.Command("_SetByLayer", ss, "", "Y", "Y");

				// TODO: flatten everything

				SymbolTable symbolTable = transaction.GetObject(database.LayerTableId, OpenMode.ForWrite) as SymbolTable;

				if (symbolTable == null)
				{
					return;
				}

				foreach (ObjectId objectId in symbolTable)
				{
					LayerTableRecord layerTableRecord = transaction.GetObject(objectId, OpenMode.ForWrite) as LayerTableRecord;

					if (layerTableRecord == null)
					{
						continue;
					}

					string layerName = layerTableRecord.Name;

					if (layerTableRecord.IsLocked)
					{
						Session.Log("Warning: layer " + layerName + " was locked, ignoring");
						continue;
					}

					if (layerTableRecord.IsFrozen)
					{
						Session.Log("Warning: layer " + layerName + " was frozen, ignoring");
						continue;
					}

					foreach (CleanupEntry cleanupEntry in layerCleanupEntries)
					{
						if (cleanupEntry.Matches(layerName))
						{
							if (cleanupEntry.erase)
							{
								LayerHelper.Delete(document, transaction, layerName);
								break;
							}

							if (cleanupEntry.setsColor)
							{
								LayerHelper.SetColor(layerTableRecord, cleanupEntry.color);
							}

							if (cleanupEntry.setsLinetype)
							{
								LayerHelper.SetLinetype(transaction, layerTableRecord, cleanupEntry.linetype, true);
							}

							if (cleanupEntry.setsLinetypeScale)
							{
								LayerHelper.SetAllObjectsLinetypeScale(transaction, layerTableRecord, cleanupEntry.linetypeScale);
							}

							if (cleanupEntry.setsPlotStyleName)
							{
								layerTableRecord.PlotStyleName = cleanupEntry.plotStyleName;
							}

							break;
						}
					}
				}

				transaction.Commit();
			}
		}
	}
}
