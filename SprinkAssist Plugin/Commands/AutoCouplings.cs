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

[assembly: CommandClass(typeof(Ironwill.AutoCouplings))]

namespace Ironwill
{
	class AutoCouplings
	{
		/// ---------------------------------------------------------------------------------------
		/**  */
		[CommandMethod("SpkAssist_AutoCouplings", CommandFlags.UsePickSet)]
		public void AutoCouplingsCmd()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = doc.Database;
			Editor editor = doc.Editor;

			TypedValue[] filter = {
				new TypedValue((int)DxfCode.Operator, "<or"),
				//new TypedValue((int)DxfCode.LayerName, Layers.Armover.Get()),
				new TypedValue((int)DxfCode.LayerName, Layer.SystemPipe_Branchline.Get()),
				new TypedValue((int)DxfCode.LayerName, Layer.SystemPipe_Main.Get()),
				new TypedValue((int)DxfCode.Operator, "or>"),
			};

			PromptSelectionResult selectionResult = editor.GetSelection(new SelectionFilter(filter));

			if (selectionResult.Status != PromptStatus.OK)
			{
				return;
			}

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				string couplingBlockName = Blocks.Fitting_GroovedCoupling.Get();

				if (!blockTable.Has(couplingBlockName))
				{
					AcApplication.ShowAlertDialog("Coupling block" + couplingBlockName + "missing, aborting");
					transaction.Abort();
					return;
				}

				SelectionSet selectionSet = selectionResult.Value;

				foreach (ObjectId objectId in selectionSet.GetObjectIds())
				{
					Line line = transaction.GetObject(objectId, OpenMode.ForRead) as Line;

					if (line == null)
					{
						continue;
					}

					List<Line> segments = GetLineSegments(line);

					OrderLineSegments(ref segments);

					foreach (Line segment in segments)
					{
						Session.WriteMessage("Creating couplings for Line [" + segment.StartPoint.ToString() + ", " + segment.EndPoint.ToString() + "]");
						CreateCouplings(transaction, segment);
					}
				}

				transaction.Commit();
			}

			// Iterate over all pipes
			// For each pipe, find the end(s) of the pipe
			// Determine how many couplings are required and place them
			// TODO: check for other nearby fittings or heads along the line and adjust spacing
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static List<Line> GetLineSegments(Line line)
		{
			List<Line> segments = new List<Line>();
			List<Point3d> breakPoints = GetBreakPoints(line, true, true);
			
			GetLineSegments(line, ref segments, ref breakPoints);

			Session.WriteMessage("Divided line into " + segments.Count + " segments");

			return segments;
		}

		private static void GetLineSegments(Line line, ref List<Line> segments, ref List<Point3d> breakPoints)
		{
			Document document = AcApplication.DocumentManager.MdiActiveDocument;

			Database database = document.Database;

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				bool lineSplit = false;

				foreach (Point3d breakPoint in breakPoints)
				{
					// TODO units scale
					const double threshold = 0.5;

					// Check if this break point is on one of the ends of the line, ignore it if so
					if (breakPoint.DistanceTo(line.StartPoint) < threshold || breakPoint.DistanceTo(line.EndPoint) < threshold)
					{
						continue;
					}

					// Check if this break point is not on the line, ignore it if so
					Point3d closestPoint = line.GetClosestPointTo(breakPoint, false);
					double distance = closestPoint.DistanceTo(breakPoint);

					if (distance > threshold)
					{
						continue;
					}

					// Divide up the line into two pieces and recursively get breakpoints of the new pieces
					// The order of the points is imporant - other code elsewhere assumes the first point is the end of the line
					Line firstSegment = new Line(line.StartPoint, closestPoint);
					Line secondSegment = new Line(line.EndPoint, closestPoint);

					GetLineSegments(firstSegment, ref segments, ref breakPoints);
					GetLineSegments(secondSegment, ref segments, ref breakPoints);

					lineSplit = true;

					// Don't process this line anymore, it's been split into two new lines
					break;
				}

				if (!lineSplit)
				{
					segments.Add(line);
				}
			}
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static List<Point3d> GetBreakPoints(Line targetLine, bool breakOnBlocks, bool breakOnLines)
		{
			var points = new List<Point3d>();

			Document document = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = document.Database;

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				BlockTableRecord blockTableRecord = Session.GetBlockTableRecord(transaction);

				foreach (ObjectId objectId in blockTableRecord)
				{
					DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);

					TryGetBreakPointsAsBlock(targetLine, dbObject, ref points);
					TryGetBreakPointsAsLine(targetLine, dbObject, ref points);
				}
			}

			return points;
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static void TryGetBreakPointsAsBlock(Line targetLine, DBObject dbObject, ref List<Point3d> breakPoints)
		{
			BlockReference block = dbObject as BlockReference;

			if (block == null)
			{
				return;
			}

			if (block.Name == Blocks.Fitting_Cap.Get())
			{
				return;
			}

			List<string> validBlockLayers = new List<string> { Layer.SystemPipe_Main.Get(), Layer.SystemPipe_Branchline.Get(), Layer.SystemPipe_Armover.Get() };

			if (!validBlockLayers.Contains(block.Layer))
			{
				return;
			}

			double threshold = 0.5;

			if (targetLine.GetClosestPointTo(block.Position, false).DistanceTo(block.Position) > threshold)
			{
				return;
			}

			// I need to find the line which this block would attach the target pipe to and evaluate if the other line is a smaller type
			Document document = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = document.Database;

			List<string> validLineLayers;

			if (targetLine.Layer == Layer.SystemPipe_Main.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get() };
			}
			else if (targetLine.Layer == Layer.SystemPipe_Branchline.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get(), Layer.SystemPipe_Branchline.Get() };
			}
			else if (targetLine.Layer == Layer.SystemPipe_Armover.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get(), Layer.SystemPipe_Branchline.Get(), Layer.SystemPipe_Armover.Get() };
			}
			else
			{
				validLineLayers = new List<string> { };
			}

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				BlockTableRecord blockTableRecord = Session.GetBlockTableRecord(transaction);

				//bool handled = false;

				// Check all lines 
				foreach (ObjectId objectId in blockTableRecord)
				{
					DBObject otherObject = transaction.GetObject(objectId, OpenMode.ForRead);

					Line otherLine = otherObject as Line;

					if (otherLine == null)
					{
						continue;
					}

					if (otherLine == targetLine)
					{
						continue;
					}

					if (otherLine.GetClosestPointTo(block.Position, false).DistanceTo(block.Position) <= threshold)
					{
						if (!validLineLayers.Contains(otherLine.Layer))
						{
							continue;
						}

						breakPoints.Add(block.Position);
						//handled = true;
						break;
					}
				}

				// TODO: This won't catch every possibility. If there is not info about the line below, I can't judge whether or not there is actually a pipe diameter change.
				// if (!handled) {  }

				transaction.Commit();
			}
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static void TryGetBreakPointsAsLine(Line targetLine, DBObject dbObject, ref List<Point3d> breakPoints)
		{
			Line line = dbObject as Line;

			if (line == null || line == targetLine)
			{
				return;
			}

			List<string> validLineLayers;

			if (targetLine.Layer == Layer.SystemPipe_Main.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get() };
			}
			else if (targetLine.Layer == Layer.SystemPipe_Branchline.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get(), Layer.SystemPipe_Branchline.Get() };
			}
			else if (targetLine.Layer == Layer.SystemPipe_Armover.Get())
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main.Get(), Layer.SystemPipe_Branchline.Get(), Layer.SystemPipe_Armover.Get() };
			}
			else
			{
				validLineLayers = new List<string> { };
			}

			if (!validLineLayers.Contains(line.Layer))
			{
				return;
			}

			breakPoints.Add(line.StartPoint);
			breakPoints.Add(line.EndPoint);
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private void OrderLineSegments(ref List<Line> segments)
		{
			List<Point3d> connectionPoints = GetConnectionPoints(segments);

			// For each segment, check if the start and/or end are connected to another pipe. Set the first point as the unconnected point, if applicable. Do nothing otherwise (e.g. for grid systems)
			// TODO mouse click for start point?

			foreach (Line line in segments)
			{
				bool startPointConnected = false;
				bool endPointConnected = false;

				foreach (Point3d point in connectionPoints)
				{
					if (line.StartPoint.DistanceTo(point) < 0.5)
					{
						startPointConnected = true;
					}

					if (line.EndPoint.DistanceTo(point) < 0.5)
					{
						endPointConnected = true;
					}

					if (startPointConnected && endPointConnected)
					{
						break;
					}
				}

				if (startPointConnected && !endPointConnected)
				{
					Point3d temp = line.EndPoint;
					line.EndPoint = line.StartPoint;
					line.StartPoint = temp;
				}
			}
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private List<Point3d> GetConnectionPoints(List<Line> segments)
		{
			List<Point3d> connectionPoints = new List<Point3d>();

			foreach (Line line in segments)
			{
				// Find all points where A) an end of this line touches another line or B) where there is a fitting such as a tee or riser along the length of the line *and* the other connected pipe is of a larger type

				Document document = AcApplication.DocumentManager.MdiActiveDocument;
				Database database = document.Database;

				using (Transaction transaction = database.TransactionManager.StartTransaction())
				{
					BlockTableRecord blockTableRecord = Session.GetBlockTableRecord(transaction);

					foreach (ObjectId objectId in blockTableRecord)
					{
						DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);


					}
				}
			}

			return connectionPoints;
		}


		/// ---------------------------------------------------------------------------------------
		/**  */
		private void CreateCouplings(Transaction transaction, Line segment)
		{
			double remainingLength = segment.Length;

			// TODO units and setting up global constants
			// TODO plastic?
			double pipeLength = 0;
			DrawingUnits units = Session.GetPrimaryUnits();

			if (units == DrawingUnits.Metric)
			{
				pipeLength = 6400.8;
			}
			else if (units == DrawingUnits.Imperial)
			{
				pipeLength = 252;
			}
			else
			{
				Session.WriteMessage("Drawing units not properly set! Must be metric or architectural");
				return;
			}

			Point3d currentPoint = segment.StartPoint;

			while (remainingLength > pipeLength)
			{
				Vector3d lineDir = segment.EndPoint - segment.StartPoint;
				Vector3d normalizedDir = lineDir.GetNormal();
				Vector3d pipeLengthDir = pipeLength * normalizedDir;

				currentPoint += pipeLengthDir;

				BlockReference blockReference = BlockDictionary.InsertBlock(Blocks.Fitting_GroovedCoupling.Get());

				if (blockReference == null)
				{
					Session.WriteMessage("Block " + Blocks.Fitting_GroovedCoupling.Get() + " was not found, aborting");
					return;
				}

				Vector3d segmentDirection = (segment.EndPoint - segment.StartPoint).GetNormal();

				blockReference.Position = currentPoint;
				blockReference.ScaleFactors = new Scale3d(Session.GetScaleFactor());
				blockReference.Rotation = Session.SanitizeAngle(segment.Angle + Session.Radians(90), segmentDirection);
				blockReference.Layer = Layer.SystemFitting.Get();

				remainingLength -= pipeLength;
			}
		}
	}
}
