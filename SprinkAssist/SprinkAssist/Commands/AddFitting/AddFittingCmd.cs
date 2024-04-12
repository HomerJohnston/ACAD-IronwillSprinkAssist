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
using System.Collections.ObjectModel;

using Ironwill.Commands.Help;
using Autodesk.AutoCAD.GraphicsInterface;
using Ironwill.Commands.AddSprinkler;
using Ironwill.Structures;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

[assembly: CommandClass(typeof(Ironwill.Commands.AddFittingCmd))]

namespace Ironwill.Commands
{
	internal class AddFittingCmd : SprinkAssistCommand
	{
		const string elbowKeyword = "Elbow";
		const string teeKeyword = "Tee";
		const string capKeyword = "Cap";
		const string riserKeyword = "Riser";
		const string reducerKeyword = "REducer";
		const string couplingKeyword = "COupling";

		readonly IList<string> Keywords = new ReadOnlyCollection<string>( new List<string> { elbowKeyword, teeKeyword, capKeyword, riserKeyword, reducerKeyword, couplingKeyword } );

		CommandSetting<string> selectedFittingSetting;

		AddFittingJigger jigger = null;

		public AddFittingCmd()
		{
			selectedFittingSetting = settings.RegisterNew("SelectedFitting", elbowKeyword);
		}

		[CommandDescription("Draws fittings onto sprinkler pipe.", "Attempts to place fittings on ends or midpoints of pipe lines.", "Will also place it anywhere lines if another line's endpoint touches the line nearby.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AddFitting", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoMultiple)]
		public void Main()
		{
			PromptResult promptResult = null;

			object OSMODE_Value = Application.GetSystemVariable("OSMODE");
			Application.SetSystemVariable("OSMODE", 0);

			while (promptResult == null || promptResult.Status == PromptStatus.OK || promptResult.Status == PromptStatus.Keyword)
			{
				using (Transaction transactionNew = Session.StartTransaction())
				{
					jigger = new AddFittingJigger(transactionNew, selectedFittingSetting.Get(transactionNew));

					promptResult = Session.GetEditor().Drag(jigger);

					switch (promptResult.Status)
					{
						case PromptStatus.Keyword:
							{
								selectedFittingSetting.Set(transactionNew, promptResult.StringResult);
								break;
							}
						case PromptStatus.Cancel:
							{
								break;
							}
					}

					if (!jigger.draw)
					{
						jigger.EraseJiggedFitting();
					}

					transactionNew.Commit();

					jigger.Close();
					jigger = null;
				}
			}
			
			Application.SetSystemVariable("OSMODE", OSMODE_Value);

			return;
		}

		string GetFittingName(Transaction transaction)
		{
			switch (selectedFittingSetting.Get(transaction))
			{
				case elbowKeyword:
					return Blocks.Fitting_Elbow.Get();
				case teeKeyword:
					return Blocks.Fitting_Tee.Get();
				case capKeyword:
					return Blocks.Fitting_Cap.Get();
				case riserKeyword:
					return Blocks.Fitting_Riser.Get();
				case couplingKeyword:
					return Blocks.Fitting_GroovedCoupling.Get();
				case reducerKeyword:
					return Blocks.Fitting_GroovedReducingCoupling.Get();
			}

			return "";
		}

		bool FindPlacementPoint(Transaction transaction, Point3d cursorPoint, Line pickedLine, out Point3d foundPoint)
		{
			Point3d closestCandidatePoint = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);

			bool foundFittingPoint = false;

			BlockTableRecord blockTableRecord = Session.GetModelSpaceBlockTableRecord(transaction);

			Point3dCollection candidatePoints = new Point3dCollection() { pickedLine.StartPoint, pickedLine.EndPoint};

			Point3d centrePoint = new Point3d(pickedLine.StartPoint.X + pickedLine.EndPoint.X, pickedLine.StartPoint.Y + pickedLine.EndPoint.Y, pickedLine.StartPoint.Z + pickedLine.EndPoint.Z).MultiplyBy(0.5);
			candidatePoints.Add(centrePoint);

			foreach (ObjectId objectId in blockTableRecord)
			{
				DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
				Line testLine = dbObject as Line;

				if (testLine == null)
				{
					continue;
				}

				Point3dCollection points = new Point3dCollection();
				pickedLine.IntersectWith(testLine, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);

				if (points.Count != 1)
				{
					continue;
				}

				candidatePoints.Add(points[0]);
			}

			if (candidatePoints.Count == 0)
			{
				foundPoint = new Point3d(0.0, 0.0, 0.0);
				return false;
			}

			foreach (Point3d candidateIntersectionPoint in candidatePoints)
			{
				if (candidateIntersectionPoint.DistanceTo(cursorPoint) < closestCandidatePoint.DistanceTo(cursorPoint))
				{
					foundFittingPoint = true;
					closestCandidatePoint = candidateIntersectionPoint;
				}
			}

			if (!foundFittingPoint)
			{
				foundPoint = pickedLine.GetClosestPointTo(cursorPoint, true);
				return true;
			}

			foundPoint = closestCandidatePoint;
			return true;
		}

		double GetFittingRotation(Point3d clickPoint, Point3d fittingPoint, Line line)
		{
			Point3d pointOnLine = line.GetClosestPointTo(clickPoint, true);

			Point2d fittingPoint2d = new Point2d(fittingPoint.X, fittingPoint.Y);
			Point2d clickPoint2d = new Point2d(pointOnLine.X, pointOnLine.Y);

			return fittingPoint2d.GetVectorTo(clickPoint2d).Angle;
		}

		void PlaceFitting(string blockName, Point3d position, double rotation, string layer)
		{
			BlockReference blockRef = BlockOps.InsertBlock(blockName);

			if (blockRef == null)
			{
				return;
			}

			blockRef.Position = position;
			blockRef.Rotation = rotation;
			blockRef.ScaleFactors = new Scale3d(Session.GetBlockScaleFactor());
			blockRef.Layer = layer;
		}
	}

	internal class AddFittingKeywordHandler<T> : KeywordActionHandler<T> where T : AddFittingJigger
	{
		const string elbowKeyword = "Elbow";
		const string teeKeyword = "Tee";
		const string capKeyword = "Cap";
		const string riserKeyword = "Riser";
		const string reducerKeyword = "REducer";
		const string couplingKeyword = "COupling";

		public AddFittingKeywordHandler(T inCommand) : base(inCommand)
		{
			RegisterKeyword(elbowKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_Elbow);
			});
			RegisterKeyword(teeKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_Tee);
			});
			RegisterKeyword(capKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_Cap);
			});
			RegisterKeyword(riserKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_Riser);
			});
			RegisterKeyword(reducerKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_GroovedReducingCoupling);
			});
			RegisterKeyword(couplingKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(Blocks.Fitting_GroovedCoupling);
			});
		}
	}

	internal class AddFittingJigger : DrawJig
	{
		// State ----------------------------------------------------------------------------------
		Transaction transaction;

		BlockReference jiggedFitting = null;

		Point3d cursorPosition;

		AddFittingKeywordHandler<AddFittingJigger> keywordHandler;

		BoundingVolumeHierarchy pipeBVH;

		List<Polyline3d> pipeBVHPolylines = new List<Polyline3d>();

		List<Line> currentLines = new List<Line>();

		Point3d jigPosition;
		
		double jigRotation = 0;

		public bool draw = false;

		// Constructor ----------------------------------------------------------------------------
		public AddFittingJigger(Transaction transaction, string blockName) 
		{
			this.transaction = transaction;

			keywordHandler = new AddFittingKeywordHandler<AddFittingJigger>(this);
			keywordHandler.Consume(transaction, blockName);
			
			BlockTableRecord blockTableRecord = Session.GetModelSpaceBlockTableRecord(transaction);

			List<string> pipeLayers = Layer.PipeLayers;

			List<Entity> cachedPipeLines = new List<Entity>();

			foreach (ObjectId objectId in blockTableRecord)
			{
				DBObject dbObject = transaction.GetObject(objectId, OpenMode.ForRead);
				Line testLine = dbObject as Line;

				if (testLine == null)
				{
					continue;
				}

				if (pipeLayers.Contains(testLine.Layer))
				{
					cachedPipeLines.Add(testLine);
				}
			}

			if (cachedPipeLines.Count > 0)
			{
				pipeBVH = new BoundingVolumeHierarchy(cachedPipeLines);
				//pipeBVHPolylines = pipeBVH.GeneratePolylines();
			}

			Session.GetEditor().PointMonitor += PointMonitor;
		}

		public void Close()
		{
			Session.GetEditor().PointMonitor -= PointMonitor;
		}

		// Methods --------------------------------------------------------------------------------		
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Click to place...");
			
			keywordHandler.SetKeywordsForPrompt(jigPromptPointOptions);

			PromptPointResult promptPointResult = prompts.AcquirePoint(jigPromptPointOptions);

			switch (promptPointResult.Status)
			{
				case PromptStatus.Keyword:
					{
						string keyword = promptPointResult.StringResult;
						keywordHandler.Consume(transaction, keyword);

						return SamplerStatus.NoChange;
					}
				case PromptStatus.None:
					{
						EraseJiggedFitting();
						return SamplerStatus.NoChange;
					}
				case PromptStatus.Cancel:
					{
						EraseJiggedFitting();
						return SamplerStatus.Cancel;
					}
				case PromptStatus.Error:
					{
						EraseJiggedFitting();
						return SamplerStatus.Cancel;
					}
			}

			Point3d? selectedPosition = null;
			Point3d? selectedNearestPointOnLine = null;

			Line selectedLine = null;

			if (currentLines.Count == 0)
			{
				draw = false;
				selectedPosition = cursorPosition;
			}
			else
			{
				draw = true;
				foreach (Line line in currentLines)
				{
					Point3d candidatePosition;
					Point3d candidateNearestPointOnLine;

					if (FindPreferredPointOnLine(transaction, cursorPosition, line, out candidatePosition))
					{
						if (selectedPosition == null)
						{
							selectedPosition = candidatePosition;
							selectedNearestPointOnLine = line.GetClosestPointTo(cursorPosition, false);

							selectedLine = line;
							continue;
						}

						candidateNearestPointOnLine = line.GetClosestPointTo(cursorPosition, false);

						if (candidateNearestPointOnLine.DistanceTo(cursorPosition) < selectedNearestPointOnLine.Value.DistanceTo(cursorPosition))
						{
							selectedPosition = candidatePosition;
							selectedLine = line;
						}
					}
				}
			}

			if (selectedPosition != null && jiggedFitting  != null)
			{
				jiggedFitting.Position = selectedPosition.Value;

				if (selectedLine != null)
				{
					jiggedFitting.Rotation = GetFittingRotation(cursorPosition, selectedPosition.Value, selectedLine);
					jiggedFitting.Layer = selectedLine.Layer;
				}
				else
				{
					jiggedFitting.Rotation = 0;
					jiggedFitting.Layer = Layer.DraftAid;
				}
			}

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw drawer)
		{
			if (jiggedFitting == null || !draw)
			{
				return false;
			}

			jiggedFitting.Draw(drawer);

			foreach (Polyline3d polyline in pipeBVHPolylines)
			{
				//polyline.Draw(drawer);
			}

			return true;
		}

		public void SetFittingBlock(string newBlock)
		{
			EraseJiggedFitting();
			
			jiggedFitting = BlockOps.InsertBlock(newBlock);
			jiggedFitting.Layer = Layer.DraftAid;
			jiggedFitting.ScaleFactors = new Scale3d(Session.GetBlockScaleFactor());
		}

		public void EraseJiggedFitting()
		{
			if (jiggedFitting != null)
			{
				jiggedFitting.Erase();
			}
		}


		void PointMonitor(object sender, PointMonitorEventArgs args)
		{
			//bool snapped = (args.Context.History & PointHistoryBits.ObjectSnapped) == PointHistoryBits.ObjectSnapped;
			//cursorPosition = snapped ? args.Context.ObjectSnappedPoint : args.Context.ComputedPoint;

			cursorPosition = args.Context.ComputedPoint;
			cursorPosition.TransformBy(Session.GetEditor().CurrentUserCoordinateSystem.Inverse());

			if (args.Context == null)
			{
				return;
			}

			FullSubentityPath[] entityPaths = args.Context.GetPickedEntities();

			if (entityPaths.Length == 0)
			{
				jigPosition = args.Context.ComputedPoint;
				jigRotation = 0;
			}

			List<string> lineLayers = Layer.PipeLayers;

			using (Transaction transaction = Session.StartTransaction())
			{
				currentLines.Clear();

				foreach (FullSubentityPath entityPath in entityPaths)
				{
					Line line = transaction.GetObject(entityPath.GetObjectIds().First(), OpenMode.ForRead) as Line;

					if (line != null && Layer.PipeLayers.Contains(line.Layer))
					{
						currentLines.Add(line);
					}
				}

				transaction.Commit();
			}
		}

		bool FindPreferredPointOnLine(Transaction transaction, Point3d cursorPoint, Line pickedLine, out Point3d foundPoint)
		{
			System.Windows.Forms.Keys mods = System.Windows.Forms.Control.ModifierKeys;
			bool shift = (mods & System.Windows.Forms.Keys.Shift) > 0;
			bool control = (mods & System.Windows.Forms.Keys.Control) > 0;

			Point3d closestCandidatePoint = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);

			bool foundFittingPoint = false;

			Point3dCollection candidatePoints = new Point3dCollection() { pickedLine.StartPoint, pickedLine.EndPoint };

			if (control && !shift)
			{
				Point3d centrePoint = new Point3d(pickedLine.StartPoint.X + pickedLine.EndPoint.X, pickedLine.StartPoint.Y + pickedLine.EndPoint.Y, pickedLine.StartPoint.Z + pickedLine.EndPoint.Z).MultiplyBy(0.5);
				candidatePoints.Add(centrePoint);
			}

			if (shift && !control)
			{
				Point3d nearestPoint = pickedLine.GetClosestPointTo(cursorPoint, false);
				candidatePoints.Add(nearestPoint);
			}

			var pipeEntities = pipeBVH.FindEntities(cursorPoint);

			foreach (Line otherLine in pipeEntities)
			{
				if (otherLine == null)
				{
					continue;
				}

				Point3dCollection points = new Point3dCollection();
				pickedLine.IntersectWith(otherLine, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);

				if (points.Count != 1)
				{
					continue;
				}

				candidatePoints.Add(points[0]);
			}

			foreach (Point3d candidateIntersectionPoint in candidatePoints)
			{
				if (candidateIntersectionPoint.DistanceTo(cursorPoint) < closestCandidatePoint.DistanceTo(cursorPoint))
				{
					foundFittingPoint = true;
					closestCandidatePoint = candidateIntersectionPoint;
				}
			}

			if (!foundFittingPoint)
			{
				foundPoint = pickedLine.GetClosestPointTo(cursorPoint, true);
				return true;
			}

			foundPoint = closestCandidatePoint;
			return true;
		}

		double GetFittingRotation(Point3d clickPoint, Point3d fittingPoint, Line line)
		{
			System.Windows.Forms.Keys mods = System.Windows.Forms.Control.ModifierKeys;
			bool shift = (mods & System.Windows.Forms.Keys.Shift) > 0;
			bool control = (mods & System.Windows.Forms.Keys.Control) > 0;

			Point3d pointOnLine = line.GetClosestPointTo(clickPoint, true);

			Point2d fittingPoint2d = new Point2d(fittingPoint.X, fittingPoint.Y);
			Point2d clickPoint2d = new Point2d(pointOnLine.X, pointOnLine.Y);

			if (shift)
			{
				Point2d cursorPos2d = new Point2d(cursorPosition.X, cursorPosition.Y);
				return fittingPoint2d.GetVectorTo(cursorPos2d).Angle + Math.PI / 2;
			}
			else
			{
				return fittingPoint2d.GetVectorTo(clickPoint2d).Angle;
			}
		}

	}
}
