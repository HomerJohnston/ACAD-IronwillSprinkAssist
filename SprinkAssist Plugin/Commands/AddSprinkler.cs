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

[assembly: CommandClass(typeof(Ironwill.AddSprinkler))]

namespace Ironwill
{
	public class AddSprinkler : DrawJig
	{
		// Settings -----------------------------
		static DictionaryPath dictionaryPath = new DictionaryPath("PlaceSprinkler");

		BoolSetting tbarPlacement = new BoolSetting(dictionaryPath, "tbarPlacement", false);
		DoubleSetting minRadius = new DoubleSetting(dictionaryPath, "minRadius", 0.0);
		DoubleSetting maxRadius1 = new DoubleSetting(dictionaryPath, "maxRadius1", 0.0);
		DoubleSetting maxRadius2 = new DoubleSetting(dictionaryPath, "maxRadius2", 0.0);
		
		// TODO - global settings for layer names
		StringSetting CeilingLayer = new StringSetting(dictionaryPath, "ceilingLayer", "CLNG");
		StringSetting WallLayer = new StringSetting(dictionaryPath, "wallLayer", "WALL"); 

		StringSetting blockName = new StringSetting(dictionaryPath, "blockName", Blocks.Sprinkler_Head_02.Get());

		const string CoverageStyleKeyword = "Style";
		const string CoverageRadiusKeyword = "Radius";
		const string CoverageLengthKeyword = "Length";
		const string CoverageWidthKeyword = "Width";
		const string CoverageFlipKeyword = "Flip";
		const string CoverageAlignKeyword = "Align";

		// State --------------------------------
		List<Line> cachedCeilingLines = new List<Line>();
		List<Line> cachedWallLines = new List<Line>();

		Point3d cursorPos;
		Point3d snapPos;
		ObjectId sourceObject;

		List<Vector3d> snapPoints = new List<Vector3d>();
		List<Circle> snapPointMarkers = new List<Circle>();

		List<Point3d> candidatePositions = new List<Point3d>();
		List<Circle> candidatePositionMarkers = new List<Circle>();

		List<Tuple<Point3d, short>> debugPositions = new List<Tuple<Point3d, short>>();
		List<Circle> debugMarkers = new List<Circle>();

		Circle placementCircle;
		Circle coverageCircle;

		[CommandMethod("SpkAssist_AddSprinkler")]
		public void AddSprinklerCmd()
		{
			GenerateCeilingGrid();

			if (!EnsureSourceObjectIsValid())
			{
				Session.Log("Failed to find a valid sprinkler block!");
				return;
			}

			Session.Log("Selected object: " + sourceObject.ToString());


			
			PromptResult promptResult;

			int OSMODE = System.Convert.ToInt32(AcApplication.GetSystemVariable("OSMODE"));

			AcApplication.SetSystemVariable("OSMODE", 0);

			do
			{
				promptResult = Session.GetEditor().Drag(this);

				using (Transaction transaction = Session.StartTransaction())
				{
					if (promptResult.Status == PromptStatus.OK)
					{
						BlockOps.CopyBlock(transaction, sourceObject, snapPos);
					}
					transaction.Commit();
				}
			}

			while (promptResult.Status == PromptStatus.OK);

			AcApplication.SetSystemVariable("OSMODE", OSMODE);
		}

		// Constructor --------------------------

		// API ----------------------------------
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions("Click to place...");

			jigPromptPointOptions.Keywords.Add("teSt");

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

				if (vectorToPos.LengthSqrd > 1820 * 1820)
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

			if (snapPoints.Count != 4)
			{
				//return SamplerStatus.OK;
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

				// TODO imperial
				int increment = (SprinkMath.NearlyEqual(shortLine.Length, longLine.Length, 5.0)) ? 2 : 4;
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

					if (sourceObject != null)
					{
						snapPos = closest.Value;
					}
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
				marker.Diameter = 10 + 5 * i;
				marker.Color = Color.FromColorIndex(ColorMethod.ByAci, debugPositions[i].Item2);
				
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
				marker.Diameter = 25;
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
				marker.Diameter = 15;
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
				placementCircle.Diameter = 100; // TODO imperial
				placementCircle.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.Orange);
			}

			placementCircle.Center = snapPos;
		}

		private void CreateCoverageCircle()
		{
			if (coverageCircle == null)
			{
				coverageCircle = new Circle();
				coverageCircle.Diameter = 4572; // TODO imperial
				coverageCircle.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.DarkGrey);
				coverageCircle.Linetype = Linetypes.Dashed; // TODO cleaner error handling in the event any types are not available
			}

			coverageCircle.Center = snapPos;
		}

		private bool EnsureSourceObjectIsValid()
		{
			//if (sourceObject == ObjectId.Null)
			//{
				using (Transaction transaction = Session.StartTransaction())
				{
					BlockReference blockReference = BlockOps.PickSprinkler(transaction, "Pick sprinkler type to place");

					if (blockReference != null)
					{
						sourceObject = blockReference.ObjectId;
					}
				}
			//}

			return sourceObject != ObjectId.Null;
		}

		static List<string> ceilingLayers;
		static List<string> wallLayers;

		private void GenerateCeilingGrid()
		{
			cachedCeilingLines.Clear();

			ceilingLayers = LayerHelper.CollectLayersWithString(CeilingLayer.stringValue);
			wallLayers = LayerHelper.CollectLayersWithString(WallLayer.stringValue);

			Database database = Session.GetDatabase();
			ModelSpace.xxx = 0;
			ModelSpace.ERecurseFlags recurseFlags = ModelSpace.ERecurseFlags.RecurseXrefs | ModelSpace.ERecurseFlags.RecurseBlocks;
			ModelSpace.IterateAllEntities(database, recurseFlags, CacheRelevantEntity);

			Session.LogDebug("Found " + cachedWallLines.Count + " wall lines");
			Session.LogDebug("Found " + cachedCeilingLines.Count + " ceiling lines");

			ProcessCeilingLines();
		}

		
		void CacheRelevantEntity(Entity entity)
		{
			if (!wallLayers.Contains(entity.Layer) && !ceilingLayers.Contains(entity.Layer))
			{
				return;
			}

			if (TryCacheLine(entity))
			{
				return;
			}

			if (TryCachePolyline(entity))
			{
				return;
			}

			if (TryCacheHatch(entity))
			{
				return;
			}
		}

		bool TryCacheLine(Entity entity)
		{
			Line line = entity as Line;

			if (line == null)
			{
				return false;
			}

			Line lineCopy = new Line(line.StartPoint, line.EndPoint);

			if (wallLayers.Contains(line.Layer))
			{
				if (line.Length > 300)
				{
					cachedWallLines.Add(lineCopy);
				}
			}
			else if (ceilingLayers.Contains(line.Layer))
			{
				if (line.Length > 1250)
				{
					cachedCeilingLines.Add(lineCopy);
				}
			}

			return true;
		}

		bool TryCachePolyline(Entity entity)
		{
			Polyline polyline = entity as Polyline;

			if (polyline == null)
			{
				return false;
			}

			DBObjectCollection lineSegments = new DBObjectCollection();

			polyline.Explode(lineSegments);

			foreach (Entity lineSegment in lineSegments)
			{
				CacheRelevantEntity(lineSegment);
			}

			return true;
		}

		bool TryCacheHatch(Entity entity)
		{
			Hatch hatch = entity as Hatch;

			if (hatch == null)
			{
				return false;
			}

			if (hatch.NumberOfHatchLines == 0 || hatch.HatchObjectType != HatchObjectType.HatchObject)
			{
				return true;
			}

			DBObjectCollection hatchObjects = new DBObjectCollection();

			hatch.Explode(hatchObjects);

			foreach (Entity hatchEntity in hatchObjects)
			{
				CacheRelevantEntity(hatchEntity);
			}

			return true;
		}

		private void ProcessCeilingLines()
		{
			// TODO imperial / metric
			double equalPointTolerance = 2.0 * 2.0; // Tolerance.Global.EqualPoint * Tolerance.Global.EqualPoint;

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
					// TODO imperial/metric
					double extendAmount = 612.0;
					ceilingLine.ExtendBy(bStartFound ? 0.0 : extendAmount, bEndFound ? 0.0 : extendAmount);
				}
			}
		}
	}
}
