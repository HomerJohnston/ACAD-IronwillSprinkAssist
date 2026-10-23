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
using Ironwill.Structures;
using System.Diagnostics;
using System.Windows.Navigation;

[assembly: CommandClass(typeof(Ironwill.Commands.AutoCouplingsCmd))]

namespace Ironwill.Commands
{
	class AutoCouplingsCmd : SprinkAssistCommand
	{
		CommandSetting<double> endOfLineOffset;

		public AutoCouplingsCmd()
		{
			endOfLineOffset = settings.RegisterNew("EndOfLineOffset", 50.8 * Session.UnitsScaleFactor());
		}

		private bool PipelineMatch(Entity e)
		{
			if (!(e is Line))
			{
				return false;
			}

			return Layer.PipeLayers.Contains(e.Layer);
		}
		private bool FittingMatch(Entity e)
		{
			BlockReference fitting = e as BlockReference;
			
			if (fitting == null)
			{
				return false;
			}

			HashSet<string> fittingLayers = new HashSet<string>()
			{
				Layer.SystemFitting,
				Layer.SystemPipe_Main, 
				Layer.SystemPipe_Branchline, 
				Layer.SystemPipe_Armover
			};

			HashSet<string> fittingBlocks = new HashSet<string>()
			{
				Blocks.Fitting_GroovedReducingCoupling,
				Blocks.Fitting_GroovedCoupling,
				Blocks.Fitting_Elbow,
				Blocks.Fitting_Tee,
				//Blocks.Fitting_Cap,
				Blocks.Fitting_Riser,
				Blocks.Fitting_ConcentricReducer,
				Blocks.Fitting_EccentricReducer
			};

			return fittingBlocks.Contains(fitting.Name);
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		[CommandDescription("Attempts to place simple coupling symbols along the pipe.", "This should be used AFTER labelling is complete and BEFORE you trim pipe lines around other pipes.", "This does not currently take fitting take-outs into account; it is a simple system to help fitters list material only.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AutoCouplings", CommandFlags.UsePickSet | CommandFlags.Modal | CommandFlags.NoBlockEditor)]
		public void Main()
		{
			if (!VerifyCouplingBlock())
			{
				return;
			}

			using (Transaction transaction = Session.StartTransaction())
			{
				List<Line> selectedLines = GetSelectedLines(transaction);
				//List<Line> allPipeLines = new List<Line>();
				//List<BlockReference> allFittings = new List<BlockReference>();

				if (selectedLines.Count == 0)
				{
					transaction.Commit();
					return;
				}

				BoundingVolumeHierarchyManager.Rebuild(transaction);

				foreach (Line selectedLine in selectedLines)
				{
					List<Point3d> breakPoints = FindBreakPoints(transaction, selectedLine);

					List<Line> segments = DivideLineIntoSegments(transaction, selectedLine, breakPoints);

					OrientSegments(ref segments);

					CreateCouplings(transaction, ref segments);

					segments.ForEach((x) => { x.Dispose(); });
				}

				transaction.Commit();
			}
		}

		private bool VerifyCouplingBlock()
		{
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				string couplingBlockName = Blocks.Fitting_GroovedCoupling.Get();

				if (!blockTable.Has(couplingBlockName))
				{
					AcApplication.ShowAlertDialog("Coupling block" + couplingBlockName + "missing, aborting");
					return false;
				}

				transaction.Commit();
			}

			return true;
		}

		private List<Line> GetSelectedLines(Transaction transaction)
		{
			List<Line> lines = new List<Line>();

			Editor editor = Session.GetEditor();

			TypedValue[] filter = {
				new TypedValue((int)DxfCode.Start, "LINE"),
				new TypedValue((int)DxfCode.LayerName, $"{Layer.SystemPipe_Branchline.Get()},{Layer.SystemPipe_Main.Get()}"),
			};

			PromptSelectionResult selectionResult = editor.GetSelection(new SelectionFilter(filter));

			if (selectionResult.Status == PromptStatus.OK)
			{
				foreach (ObjectId objectId in selectionResult.Value.GetObjectIds())
				{
					Line line = transaction.GetObject(objectId, OpenMode.ForRead) as Line;

					if (line == null)
					{
						continue;
					}

					lines.Add(line);
				}
			}

			return lines;
		}

		private List<Point3d> FindBreakPoints(Transaction transaction, Line selectedLine)
		{
			List<Point3d> breakPoints = new List<Point3d>();

			List<Entity> nearbyEntities = BoundingVolumeHierarchyManager.Get().FindEntities(selectedLine.GeometricExtents);

			List<Line> nearbyPipes = nearbyEntities.FindAll(e => e.GetType() == typeof(Line) && Layer.PipeLayers.Contains(e.Layer)).Cast<Line>().ToList();
			List<BlockReference> nearbyFittings = nearbyEntities.FindAll(e => e.GetType() == typeof(BlockReference) && e.Layer == Layer.SystemFitting).Cast<BlockReference>().ToList();

			foreach (Line otherLine in nearbyPipes)
			{
				FindBreakPoints(selectedLine, otherLine, ref breakPoints);
			}

			foreach (BlockReference fitting in nearbyFittings)
			{
				FindBreakPoints(transaction, selectedLine, fitting, ref breakPoints);
			}

			return breakPoints;
		}

		private static void FindBreakPoints(Line selectedLine, Line otherLine, ref List<Point3d> breakPoints)
		{
			if (otherLine == selectedLine)
			{
				return;
			}

			List<string> validLineLayers;

			if (selectedLine.Layer == Layer.SystemPipe_Main)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main };
			}
			else if (selectedLine.Layer == Layer.SystemPipe_Branchline)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main, Layer.SystemPipe_Branchline };
			}
			else if (selectedLine.Layer == Layer.SystemPipe_Armover)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main, Layer.SystemPipe_Branchline, Layer.SystemPipe_Armover };
			}
			else
			{
				return;
			}

			if (!validLineLayers.Contains(otherLine.Layer))
			{
				return;
			}

			breakPoints.Add(otherLine.StartPoint);
			breakPoints.Add(otherLine.EndPoint);
		}

		private static void FindBreakPoints(Transaction transaction, Line selectedLine, BlockReference fitting, ref List<Point3d> breakPoints)
		{
			double threshold = 0.5; // TODO hardcoded shit

			if (selectedLine.GetClosestPointTo(fitting.Position, false).DistanceTo(fitting.Position) > threshold)
			{
				return;
			}

			// I need to find the line which this block would attach the target pipe to and evaluate if the other line is a smaller type
			List<string> validLineLayers;

			if (selectedLine.Layer == Layer.SystemPipe_Main)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main };
			}
			else if (selectedLine.Layer == Layer.SystemPipe_Branchline)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main, Layer.SystemPipe_Branchline };
			}
			else if (selectedLine.Layer == Layer.SystemPipe_Armover)
			{
				validLineLayers = new List<string> { Layer.SystemPipe_Main, Layer.SystemPipe_Branchline, Layer.SystemPipe_Armover };
			}
			else
			{
				validLineLayers = new List<string> { };
			}

			if (!validLineLayers.Contains(fitting.Layer))
			{
				return;
			}

			breakPoints.Add(fitting.Position);
		}

		private List<Line> DivideLineIntoSegments(Transaction transaction, Line selectedLine, List<Point3d> breakPoints)
		{
			Stack<Line> undividedSegments = new Stack<Line>();
			
			undividedSegments.Push(selectedLine);

			List<Line> foundSegments = new List<Line>();

			while (undividedSegments.Count > 0)
			{
				Line segmentToCheck = undividedSegments.Pop();

				bool split = false;

				for (int i = breakPoints.Count - 1; i >= 0; --i)
				{
					Point3d breakPoint = breakPoints[i];

					const double threshold = 0.5; // TODO tolerance!

					// Check if this break point is on one of the ends of the line, ignore it if so
					if (breakPoint.DistanceTo(segmentToCheck.StartPoint) < threshold || breakPoint.DistanceTo(segmentToCheck.EndPoint) < threshold)
					{
						continue;
					}

					// Check if this break point is not on the line, ignore it if so
					Point3d closestPoint = segmentToCheck.GetClosestPointTo(breakPoint, false);

					if (closestPoint.DistanceTo(breakPoint) > threshold)
					{
						continue;
					}

					List<Line> splitLines = SplitLine(segmentToCheck, breakPoint);
					splitLines.ForEach(line => undividedSegments.Push(line));

					breakPoints.RemoveAt(i--);

					split = true;

					if (segmentToCheck.Database == null)
					{
						segmentToCheck.Dispose();
					}

					break;
				}

				if (!split)
				{
					if (segmentToCheck.Database == null)
					{
						foundSegments.Add(segmentToCheck);
					}
					else
					{
						Line copy = new Line(segmentToCheck.StartPoint, segmentToCheck.EndPoint);
						copy.Layer = segmentToCheck.Layer;
						foundSegments.Add(copy);
					}
				}
			}

			return foundSegments;
		}

		private List<Line> SplitLine(Line lineToSplit, Point3d splitPoint)
		{
			Line firstSegment = new Line(lineToSplit.StartPoint, splitPoint);
			Line secondSegment = new Line(lineToSplit.EndPoint, splitPoint);

			firstSegment.Layer = lineToSplit.Layer;
			secondSegment.Layer = lineToSplit.Layer;

			return new List<Line>() { firstSegment, secondSegment };
		}

		enum EConnectionType
		{
			None = 0,
			Armover = 1,
			Branchline = 2,
			Main = 3,
			Fitting = 4,
		}

		void OrientSegments(ref List<Line> segments)
		{
			// Note: these lines are local copies. None of them are in the BHV.
			foreach (Line segment in segments)
			{
				List<Entity> entitiesNearStart = BoundingVolumeHierarchyManager.Get().FindEntities(segment.StartPoint);
				List<Entity> entitiesNearEnd = BoundingVolumeHierarchyManager.Get().FindEntities(segment.StartPoint);

				List<Line> linesNearStart = entitiesNearStart.FindAll(e => e.GetType().IsSubclassOf(typeof(Line))).Cast<Line>().ToList();
				List<Line> linesNearEnd = entitiesNearEnd.FindAll(e => e.GetType().IsSubclassOf(typeof(Line))).Cast<Line>().ToList();

				Clean(segment, ref linesNearStart);
				Clean(segment, ref linesNearEnd);

				EConnectionType atStart = EConnectionType.None;
				EConnectionType atEnd = EConnectionType.None;

				CheckIfPointConnected(segment.StartPoint, ref linesNearStart, ref atStart);
				CheckIfPointConnected(segment.EndPoint, ref linesNearEnd, ref atEnd);

				if (atStart > atEnd)
				{
					FlipLine(segment);
				}
			}
		}

		void Clean(Line segment, ref List<Line> nearbyLines)
		{
			if (segment == null)
			{
				return;
			}

			for (int i = 0; i < nearbyLines.Count; ++i)
			{
				Line otherLine = nearbyLines[i];

				if (otherLine == null)
				{
					continue;
				}

				if (segment.StartPoint == otherLine.StartPoint && segment.EndPoint == otherLine.EndPoint)
				{
					nearbyLines.RemoveAt(i);
					break;
				}
			}
		}

		private void CheckIfPointConnected(Point3d point, ref List<Line> otherLinesNearEndPoint, ref EConnectionType endConnected)
		{
			foreach (Line otherLine in otherLinesNearEndPoint)
			{
				if (otherLine == null)
				{
					continue; 
				}

				if (otherLine.Layer != Layer.SystemPipe_Branchline && otherLine.Layer != Layer.SystemPipe_Main)
				{
					continue;
				}

				Point3d closestPoint = otherLine.GetClosestPointTo(point, false);
				double distance = closestPoint.DistanceTo(point);

				if (distance < 0.5)
				{
					if (otherLine.Layer == Layer.SystemPipe_Main)
					{
						endConnected = (EConnectionType)Math.Max((int)endConnected, (int)EConnectionType.Main);
					}
					else if (otherLine.Layer == Layer.SystemPipe_Branchline)
					{
						endConnected = (EConnectionType)Math.Max((int)endConnected, (int)EConnectionType.Branchline);
					}
				}
			}
		}

		private void FlipLine(Line line)
		{
			Point3d temp = line.EndPoint;
			line.EndPoint = line.StartPoint;
			line.StartPoint = temp;
		}

		private void CreateCouplings(Transaction transaction, ref List<Line> segments)
		{
			foreach (Line segment in segments)
			{
				double remainingLength = segment.Length;

				// TODO units and setting up global constants
				// TODO plastic?

				double pipeLength;

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
					Session.Log("Drawing units not properly set! Must be metric or architectural");
					return;
				}

				Point3d currentPoint = segment.StartPoint;

				while (remainingLength > pipeLength)
				{
					Vector3d lineDir = segment.EndPoint - segment.StartPoint;
					Vector3d normalizedDir = lineDir.GetNormal();
					Vector3d pipeLengthDir = pipeLength * normalizedDir;

					currentPoint += pipeLengthDir;

					BlockReference blockReference = BlockOps.InsertBlock(transaction, Blocks.Fitting_GroovedCoupling.Get());

					if (blockReference == null)
					{
						Session.Log("Block " + Blocks.Fitting_GroovedCoupling.Get() + " was not found, aborting");
						return;
					}

					Vector3d segmentDirection = (segment.EndPoint - segment.StartPoint).GetNormal();

					blockReference.Position = currentPoint;
					blockReference.ScaleFactors = new Scale3d(Session.GetBlockScaleFactor());
					blockReference.Rotation = Session.SanitizeAngle(segment.Angle + Session.Radians(90), segmentDirection);
					blockReference.Layer = Layer.SystemFitting;

					remainingLength -= pipeLength;
				}
			}
		}
	}
}
