using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.Colors;

[assembly: CommandClass(typeof(Ironwill.Commands.AddSprinkler_OLD))]

namespace Ironwill.Commands
{
	internal class AddSprinkler_OLD : SprinkAssistCommand
	{
		// Settings -----------------------------
		public CommandSetting<bool> TBarPlacementSetting;
		public CommandSetting<double> TBarTileGrid;

		public CommandSetting<double> MinRadiusSetting;
		public CommandSetting<double> MaxPrimaryCoverageRadiusSetting;
		public CommandSetting<double> MaxSecondaryCoverageRadiusSetting;

		CommandSetting<string> CeilingLayerSetting;
		CommandSetting<string> WallLayerSetting;
		CommandSetting<string> SprinklerBlockNameSetting;

		const string CoverageStyleKeyword = "Style";
		const string CoverageRadiusKeyword = "Radius";
		const string CoverageLengthKeyword = "Length";
		const string CoverageWidthKeyword = "Width";
		const string CoverageFlipKeyword = "Flip";
		const string CoverageAlignKeyword = "Align";



		// State --------------------------------
		List<Point3d> cachedSnapPoints = new List<Point3d>();
		List<Line> cachedCeilingLines = new List<Line>();
		List<Line> cachedWallLines = new List<Line>();
		ObjectId sprinklerBlockObjectId;

		public AddSprinkler_OLD()
		{
			switch (Session.GetPrimaryUnits())
			{
				case DrawingUnits.Metric:
				{
					// TODO rework CommandSetting so I can define its name once at declaration and set up its runtime data (default and dictionary) separately?
					MinRadiusSetting = new CommandSetting<double>("MinRadius", 1828.0, cmdSettings);
					MaxPrimaryCoverageRadiusSetting = new CommandSetting<double>("MaxPrimaryCoverageRadius", 4572.0, cmdSettings);
					MaxSecondaryCoverageRadiusSetting = new CommandSetting<double>("MaxSecondaryCoverageRadius", 2743.0, cmdSettings);

					TBarTileGrid = new CommandSetting<double>("TBarTileGrid", 609.6, cmdSettings);
					break;
				}
				case DrawingUnits.Imperial:
				{
					MinRadiusSetting = new CommandSetting<double>("MinRadius", 72.0, cmdSettings);
					MaxPrimaryCoverageRadiusSetting = new CommandSetting<double>("MaxPrimaryCoverageRadius", 180.0, cmdSettings);
					MaxSecondaryCoverageRadiusSetting = new CommandSetting<double>("MaxSecondaryCoverageRadius", 108.0, cmdSettings);

					TBarTileGrid = new CommandSetting<double>("TBarTileGrid", 24.0, cmdSettings);
					break;
				}
			}

			TBarPlacementSetting = new CommandSetting<bool>("TBarPlacement", true, cmdSettings);
			
			CeilingLayerSetting = new CommandSetting<string>("CeilingLayer", "CLNG", cmdSettings); // TODO - global settings for sensing layers names
			WallLayerSetting = new CommandSetting<string>("WallLayer", "WALL", cmdSettings);

			SprinklerBlockNameSetting = new CommandSetting<string>("SprinklerBlock", "", cmdSettings); // TODO - global settings for available sprinkler blocks
		}

		//[CommandMethod("SpkAssist", "AddSprinklerOLD", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
		public void AddSprinkler_OLDCmd()
		{
			int OSMODE = System.Convert.ToInt32(AcApplication.GetSystemVariable("OSMODE"));
			AcApplication.SetSystemVariable("OSMODE", 0);

			using (Transaction transaction = Session.StartTransaction())
			{
				GenerateCeilingGrid(transaction);

				if (!EnsureValidSprinklerBlock(transaction))
				{
					Session.Log("Failed to find a valid sprinkler block!");
					return;
				}

				Session.LogDebug("Selected object: " + sprinklerBlockObjectId.ToString());
				
				transaction.Commit();
			}

			PromptResult promptResult;

			do
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					AddSprinklerJig jigger = new AddSprinklerJig(this, transaction, cachedCeilingLines, cachedWallLines, sprinklerBlockObjectId);

					promptResult = Session.GetEditor().Drag(jigger);

					if (promptResult.Status == PromptStatus.OK)
					{
						BlockOps.CopyBlock(transaction, sprinklerBlockObjectId, jigger.snapPos);
					}
					transaction.Commit();
				}
			}
			while (promptResult.Status == PromptStatus.OK);

			AcApplication.SetSystemVariable("OSMODE", OSMODE);
		}

		private bool EnsureValidSprinklerBlock(Transaction transaction)
		{
			BlockReference blockReference = BlockOps.PickSprinkler(transaction, "Pick sprinkler type to place");
				
			if (blockReference != null)
			{
				sprinklerBlockObjectId = blockReference.ObjectId;
			}

			return sprinklerBlockObjectId != ObjectId.Null;
		}

		static List<string> ceilingLayers;

		static List<string> wallLayers;

		private void GenerateCeilingGrid(Transaction transaction)
		{
			ceilingLayers = LayerHelper.CollectLayersWithString(CeilingLayerSetting.Get(transaction)); // TODO: global settings for ceiling / wall layers
			wallLayers = LayerHelper.CollectLayersWithString(WallLayerSetting.Get(transaction));

			if (cachedCeilingLines.Count == 0)
			{
				FindCeilingsAndWalls(transaction, ref cachedCeilingLines, ref cachedWallLines);

				Session.LogDebug("found {0} ceiling lines", cachedCeilingLines.Count.ToString());
				Session.LogDebug("found {0} wall lines", cachedWallLines.Count.ToString());
			}

			ProcessCeilingLines(transaction);
		}

		void FindCeilingsAndWalls(Transaction transaction, ref List<Line> foundCeilings, ref List<Line> foundWalls)
		{
			Database database = Session.GetDatabase();
			
			BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForRead) as BlockTableRecord;

			foreach (ObjectId objectId in modelSpaceBTR)
			{
				Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
				Process(transaction, entity, ref foundCeilings, ref foundWalls);
			}
		}

		void Process(Transaction transaction, Entity entity, ref List<Line> foundCeilings, ref List<Line> foundWalls)
		{
			if (entity is Line line)
			{
				if (wallLayers.Contains(line.Layer) && line.Length > 12)
				{
					foundWalls.Add(line);
					return;
				}

				if (ceilingLayers.Contains(line.Layer) && line.Length > 50)
				{
					foundCeilings.Add(line);
					return;
				}
			}

			if (entity is Polyline || entity is BlockReference || entity is Hatch)
			{
				if (entity is Hatch hatch)
				{
					if (hatch.NumberOfHatchLines == 0 || hatch.HatchObjectType != HatchObjectType.HatchObject)
					return;
				}

				DBObjectCollection subEntities = new DBObjectCollection();

				entity.Explode(subEntities);

				foreach (Entity blockEntity in subEntities)
				{
					Process(transaction, blockEntity, ref foundCeilings, ref foundWalls);
				}
			}

			// TODO process arcs and splines for ceiling edges
		}

		private void ProcessCeilingLines(Transaction transaction)
		{
			double equalPointTolerance = Tolerance.Global.EqualPoint;

			switch (Session.GetPrimaryUnits())
			{
				case DrawingUnits.Metric:
				{
					equalPointTolerance = 5.0;
					break;
				}
				case DrawingUnits.Imperial:
				{
					equalPointTolerance = 0.25;
					break;
				}
			}

			foreach (Line ceilingLine in cachedCeilingLines)
			{
				bool bStartFound = false;
				bool bEndFound = false;

				Point3d startPoint = ceilingLine.StartPoint;
				Point3d endPoint = ceilingLine.EndPoint;

				foreach (Line wallLine in cachedWallLines)
				{
					if (!bStartFound)
					{
						Point3d closestPointToStart = wallLine.GetClosestPointTo(startPoint, false);
						
						if ((startPoint - closestPointToStart).LengthSqrd < equalPointTolerance)
						{
							bStartFound = true;
						}
					}

					if (!bEndFound)
					{
						Point3d closestPointToEnd = wallLine.GetClosestPointTo(endPoint, false);

						if ((endPoint - closestPointToEnd).LengthSqrd < equalPointTolerance)
						{
							bEndFound = true;
						}
					}

					if (bStartFound && bEndFound)
					{
						break;
					}
				}

				if (!bStartFound || !bEndFound)
				{
					double extendAmount = TBarTileGrid.Get(transaction);
					
					//ceilingLine.ExtendBy(bStartFound ? 0.0 : extendAmount, bEndFound ? 0.0 : extendAmount);
				}
			}
		}
	}

	internal class AddSprinklerJig : DrawJig
	{
		// SETTINGS -------------------------------------
		double debugCircleSize;

		// STATE ----------------------------------------
		AddSprinkler_OLD owningCommand;

		Transaction transaction;
		
		Point3d cursorPos;

		List<Vector3d> snapPoints = new List<Vector3d>();
		List<Circle> snapPointMarkers = new List<Circle>();

		List<Point3d> candidatePositions = new List<Point3d>();
		List<Circle> candidatePositionMarkers = new List<Circle>();

		List<Tuple<Point3d, short>> debugPositions = new List<Tuple<Point3d, short>>();
		List<Circle> debugMarkers = new List<Circle>();

		Circle placementCircle;
		Circle coverageCircle;

		ObjectId sprinklerBlockObjectId;

		List<Line> cachedCeilingLines = new List<Line>();
		List<Line> cachedWallLines = new List<Line>();

		public Point3d snapPos { get; private set; }

		// CONSTRUCTOR ------------------------------------
		public AddSprinklerJig(AddSprinkler_OLD inOwningCommand, Transaction inTransaction, List<Line> inCachedCeilingLines, List<Line> inCachedWallLines, ObjectId inSprinklerBlockObjectId)
		{
			owningCommand = inOwningCommand;
			transaction = inTransaction;

			cachedCeilingLines = inCachedCeilingLines;
			cachedWallLines = inCachedWallLines;
			sprinklerBlockObjectId = inSprinklerBlockObjectId;
		}

		// API --------------------------------------------
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions("Click to place...");

			jigPromptPointOptions.Keywords.Add("Undo");

			PromptPointResult promptResult = prompts.AcquirePoint(jigPromptPointOptions);

			if (promptResult.Status == PromptStatus.Keyword)
			{
				string choice = promptResult.StringResult;
			}
			else if (promptResult.Status == PromptStatus.Cancel)
			{
				return SamplerStatus.Cancel;
			}

			cursorPos = promptResult.Value;
			snapPos = cursorPos;

			List<Line> offsetLines = new List<Line>();

			snapPoints.Clear();

			foreach (Line line in cachedCeilingLines)
			{
				Point3d nearPos = line.GetClosestPointTo(cursorPos, false);
				Vector3d vectorToPos = nearPos - cursorPos;

				double distanceThreshold = owningCommand.TBarTileGrid.Get(transaction) * 3.0;
				
				if (vectorToPos.LengthSqrd > distanceThreshold * distanceThreshold)
				{
					continue;
				}

				if (Math.Abs(line.Delta.GetNormal().DotProduct(vectorToPos.GetNormal())) > 0.0001)
				{
					continue;
				}

				bool foundAlignedVector = false;

				for (int i = 0; i < snapPoints.Count; ++i)
				{
					Vector3d vectorData = snapPoints[i];

					Vector3d dir1 = vectorToPos.GetNormal();
					Vector3d dir2 = vectorData.GetNormal();

					if (dir1.DotProduct(dir2) > 0.99)
					{
						foundAlignedVector = true;

						if (vectorToPos.LengthSqrd > vectorData.LengthSqrd)
						{
							break;
						}
						else
						{
							snapPoints[i] = vectorToPos;
						}
					}
				}

				if (!foundAlignedVector)
				{
					snapPoints.Add(vectorToPos);
				}
			}

			List<Line> tileLines = new List<Line>();

			for (int i = 0; i < snapPoints.Count; ++i)
			{
				Vector3d v1 = snapPoints[i];

				for (int j = i + 1; j < snapPoints.Count; ++j)
				{
					Vector3d v2 = snapPoints[j];

					if (Math.Abs(v1.DotProduct(v2)) > 0.99)
					{
						Point3d p1 = cursorPos + v1;
						Point3d p2 = cursorPos + v2;

						tileLines.Add(new Line(p1, p2));
					}
				}
			}

			if (tileLines.Count == 2)
			{
				Line shortLine = null;
				Line longLine = null;

				shortLine = tileLines[0];
				longLine = tileLines[1];

				if (shortLine.Length > longLine.Length)
				{
					shortLine = tileLines[1];
					longLine = tileLines[0];
				}

				Point3d shortMiddle = shortLine.StartPoint + (0.5 * (shortLine.Delta));
				Point3d longMiddle = longLine.StartPoint + (0.5 * (longLine.Delta));

				Point3d longLineNearestShortMid = longLine.GetClosestPointTo(shortMiddle, false);
				Point3d shortLineNearestLongMid = shortLine.GetClosestPointTo(longMiddle, false);

				debugPositions.Clear();
				debugPositions.Add(new Tuple<Point3d, short>(shortMiddle, Colors.Cyan));
				debugPositions.Add(new Tuple<Point3d, short>(longMiddle, Colors.OrangeRed));
				debugPositions.Add(new Tuple<Point3d, short>(shortLineNearestLongMid, Colors.Yellow));
				debugPositions.Add(new Tuple<Point3d, short>(longLineNearestShortMid, Colors.LightGreen));

				longLine = new Line(
					longLine.StartPoint + (shortMiddle - longLineNearestShortMid),
					longLine.EndPoint + (shortMiddle - longLineNearestShortMid)
					);
				shortLine = new Line(
					shortLine.StartPoint + (longMiddle - shortLineNearestLongMid),
					shortLine.EndPoint + (longMiddle - shortLineNearestLongMid)
					);

				Point3dCollection point3DCollection = new Point3dCollection();

				Point3d? closest = null;
				double closestDistance = 0;

				candidatePositions.Clear();
				
				int increment = (SprinkMath.NearlyEqual(shortLine.Length, longLine.Length)) ? 2 : 4; // TODO: do I need looser tolerance?
				for (int i = 1; i < increment; ++i)
				{
					Point3d p = longLine.StartPoint + 1.0 / increment * i * (longLine.Delta);

					candidatePositions.Add(p);

					double candidateDist = (p - cursorPos).LengthSqrd;

					if (closest == null || candidateDist < closestDistance)
					{
						closest = p;
						closestDistance = candidateDist;
					}
				}

				if (closest != null && closestDistance > 0)
				{
					snapPos = closest.Value;
				}
			}
			else
			{
				// TODO error handling
			}

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			CreateSnapPoints();

			CreateCandidateMarkers();

			CreatePlacementCircle();

			CreateCoverageCircle();

			CreateDebugMarkers();

			foreach (Circle c in snapPointMarkers)
			{
				draw.Geometry.Draw(c);
			}

			foreach (Circle c in candidatePositionMarkers)
			{
				draw.Geometry.Draw(c);
			}

			foreach (Circle c in debugMarkers)
			{
				draw.Geometry.Draw(c);
			}

			draw.Geometry.Draw(placementCircle);

			draw.Geometry.Draw(coverageCircle);

			foreach (Line ceilingLine in cachedCeilingLines)
			{
				draw.Geometry.Draw(ceilingLine);
			}

			return true;
		}

		private void CreateDebugMarkers()
		{
			ClearDebugMarkers();

			for (int i = 0; i < debugPositions.Count; i++)
			{
				Circle marker = new Circle();
				marker.Center = debugPositions[i].Item1;

				double markerSizeFactor = 0;

				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Metric:
					{
						markerSizeFactor = 10.0;
						break;
					}
					case DrawingUnits.Imperial:
					{
						markerSizeFactor = 0.5;
						break;
					}
				}

				marker.Diameter = markerSizeFactor + 0.5 * markerSizeFactor * i;
				marker.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.LightGreen);// debugPositions[i].Item2);

				debugMarkers.Add(marker);
			}
		}

		private void ClearDebugMarkers()
		{
			foreach (Entity ent in debugMarkers)
			{
				ent.Dispose();
			}

			debugMarkers.Clear();
		}

		private void CreateSnapPoints()
		{
			ClearSnapPoints();

			for (int i = 0; i < snapPoints.Count; ++i)
			{
				Circle marker = new Circle();
				marker.Center = cursorPos + snapPoints[i];

				double markerDiameter = 0;

				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Metric:
					{
						markerDiameter = 25;
						break;
					}
					case DrawingUnits.Imperial:
					{
						markerDiameter = 1;
						break;
					}
				}

				marker.Diameter = markerDiameter;
				marker.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.LightRed);

				snapPointMarkers.Add(marker);
			}
		}

		private void ClearSnapPoints()
		{
			foreach (Circle circle in snapPointMarkers)
			{
				circle.Dispose();
			}

			snapPointMarkers.Clear();
		}

		private void CreateCandidateMarkers()
		{
			ClearCandidateMarkers();

			for (int i = 0; i < candidatePositions.Count; ++i)
			{
				Circle marker = new Circle();
				marker.Center = candidatePositions[i];

				double markerDiameter = 0;

				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Metric:
					{
						markerDiameter = 15;
						break;
					}
					case DrawingUnits.Imperial:
					{
						markerDiameter = 0.75;
						break;
					}
				}

				marker.Diameter = markerDiameter;
				marker.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.Blue);

				candidatePositionMarkers.Add(marker);
			}
		}

		private void ClearCandidateMarkers()
		{
			foreach (Circle circle in candidatePositionMarkers)
			{
				circle.Dispose();
			}

			candidatePositionMarkers.Clear();
		}

		private void CreatePlacementCircle()
		{
			if (placementCircle == null)
			{
				placementCircle = new Circle();

				// TODO place the actual sprinkler block

				double markerDiameter = 0;

				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Metric:
					{
						markerDiameter = 100;
						break;
					}
					case DrawingUnits.Imperial:
					{
						markerDiameter = 4;
						break;
					}
				}

				placementCircle.Diameter = markerDiameter;
				placementCircle.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.Orange);
			}

			placementCircle.Center = snapPos;
		}

		private void CreateCoverageCircle()
		{
			if (coverageCircle == null)
			{
				coverageCircle = new Circle();

				double markerDiameter = owningCommand.MaxPrimaryCoverageRadiusSetting.Get(transaction);

				coverageCircle.Diameter = markerDiameter;
				coverageCircle.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.DarkGrey);
				coverageCircle.Linetype = Linetypes.Dashed; // TODO cleaner error handling in the event any types are not available
			}

			coverageCircle.Center = snapPos;
		}
	}

	internal class SnapPointGenerator
	{

	}
}
