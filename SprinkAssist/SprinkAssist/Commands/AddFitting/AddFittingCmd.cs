﻿using System;
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
using Autodesk.AutoCAD.Colors;
using Ironwill.Objects;
using Autodesk.Windows;
using Autodesk.AutoCAD.Internal;

[assembly: CommandClass(typeof(Ironwill.Commands.AddFittingCmd))]

namespace Ironwill.Commands
{
	internal struct FittingTransaction
	{
		Transaction transaction;
		BlockReference fitting;

		public FittingTransaction(Transaction transaction, BlockReference fitting)
		{
			this.transaction = transaction;
			this.fitting = fitting;
		}

		public void Commit()
		{
			transaction.Commit();
		}

		public void CommitWithoutFitting()
		{
			fitting.Erase();
			transaction.Commit();
		}

		public void Abort()
		{
			transaction.Abort();
		}

		public void Draw(WorldDraw drawer)
		{
			fitting.Draw(drawer);
		}
	}

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
		CommandSetting<double> snapDistanceScreenPercentage;

		public BoundingVolumeHierarchy pipeBVH;

		public Stack<FittingTransaction> transactionStack;

		public double GetSnapDistanceScreenPercentage(Transaction transaction)
		{
			return snapDistanceScreenPercentage.Get(transaction);
		}

		public AddFittingCmd()
		{
			selectedFittingSetting = settings.RegisterNew("SelectedFitting", elbowKeyword);
			snapDistanceScreenPercentage = settings.RegisterNew("SnapDistance", 0.05);
		}

		[CommandDescription("Draws fittings onto sprinkler pipe.", "Attempts to place fittings on ends or midpoints of pipe lines.", "Will also place it anywhere lines if another line's endpoint touches the line nearby.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AddFitting", CommandFlags.Modal | CommandFlags.NoBlockEditor | CommandFlags.NoMultiple)]
		public void Main()
		{
			Session.Log("Hold <shift> to snap to midpoints. Hold <ctrl> to flip the fitting.");

			PromptResult promptResult = null;
			
			transactionStack = new Stack<FittingTransaction>();
			
			object OSMODE_Value = Application.GetSystemVariable("OSMODE");
			Application.SetSystemVariable("OSMODE", 0);

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTableRecord blockTableRecord = Session.GetModelSpaceBlockTableRecord(transaction);

				List<string> pipeLayers = Layer.PipeLayers;

				List<Entity> pipeLines = new List<Entity>();

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
						pipeLines.Add(testLine);
					}
				}

				pipeBVH = new BoundingVolumeHierarchy(pipeLines);

				transaction.Commit();
			}


			while (promptResult == null || promptResult.Status == PromptStatus.OK || promptResult.Status == PromptStatus.Keyword)
			{
				Transaction transaction = Session.StartTransaction();

				AddFittingJigger jigger = new AddFittingJigger(transaction, selectedFittingSetting.Get(transaction), this);
				
				transactionStack.Push(new FittingTransaction(transaction, jigger.jiggedFitting));

				promptResult = Session.GetEditor().Drag(jigger);

				switch (promptResult.Status)
				{
					case PromptStatus.Keyword:
						{
							string keyword = promptResult.StringResult;

							if (keyword == "Undo")
							{
								// Cancel the current action
								transactionStack.Pop().Abort();

								if (transactionStack.Count > 0)
								{
									// Cancel the previous action
									transactionStack.Pop().Abort();
								}
								else
								{
									Session.Log("Everything has been undone.");
								}
							}
							else
							{
								transactionStack.Pop().CommitWithoutFitting();

								using (Transaction keywordTransaction = Session.StartTransaction())
								{
									selectedFittingSetting.Set(keywordTransaction, promptResult.StringResult);
									keywordTransaction.Commit();
								}
							}

							break;
						}
					case PromptStatus.OK:
						{
							if (!jigger.HasValidResult())
							{
								transactionStack.Pop().CommitWithoutFitting();
							}
							
							break;
						}
					case PromptStatus.None:
						{
							transactionStack.Pop().CommitWithoutFitting();
							break;
						}
					case PromptStatus.Cancel:
						{
							transactionStack.Pop().CommitWithoutFitting();
							break;
						}
					default:
						{
							throw new System.Exception("Unknown error");
						}
				}

				Session.Log($"Transaction stack: {transactionStack.Count}");

				jigger.Close();
			}

			while (transactionStack.Count > 0)
			{
				transactionStack.Pop().Commit();
			}

			Application.SetSystemVariable("OSMODE", OSMODE_Value);

			return;
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
				cmd.SetFittingBlock(transaction, Blocks.Fitting_Elbow);
			});
			RegisterKeyword(teeKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(transaction, Blocks.Fitting_Tee);
			});
			RegisterKeyword(capKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(transaction, Blocks.Fitting_Cap);
			});
			RegisterKeyword(riserKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(transaction, Blocks.Fitting_Riser);
			});
			RegisterKeyword(reducerKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(transaction, Blocks.Fitting_GroovedReducingCoupling);
			});
			RegisterKeyword(couplingKeyword, (transaction, cmd) =>
			{
				cmd.SetFittingBlock(transaction, Blocks.Fitting_GroovedCoupling);
			});
		}
	}

	internal class AddFittingJigger : DrawJig
	{
		// State ----------------------------------------------------------------------------------
		Transaction transaction;

		AddFittingCmd owningCommand;

		public BlockReference jiggedFitting = null;

		Point3d cursorPosition;

		AddFittingKeywordHandler<AddFittingJigger> keywordHandler;

		List<Polyline3d> pipeBVHPolylines = new List<Polyline3d>();

		List<Point3d> previewSnapPoints = new List<Point3d>();

		List<Line> linesUnderCursor = new List<Line>();

		Point3d? jigPosition = null;

		double? jigRotation = null;

		double snapDistanceScreenPercentage = double.MaxValue;

		double snapDistance;

		public bool HasValidResult()
		{
			return jigPosition != null && jigRotation != null;
		}

		// Constructor ----------------------------------------------------------------------------
		public AddFittingJigger(Transaction inTransaction, string blockName, AddFittingCmd inOwningCommand) 
		{
			transaction = inTransaction;
			owningCommand = inOwningCommand;

			snapDistanceScreenPercentage = owningCommand.GetSnapDistanceScreenPercentage(transaction);

#if DEBUG
			//pipeBVHPolylines = owningCommand.pipeBVH.GeneratePolylines();
#endif

			keywordHandler = new AddFittingKeywordHandler<AddFittingJigger>(this);
			keywordHandler.Consume(transaction, blockName);
			
			Session.GetEditor().PointMonitor += CursorUpdateMonitor;
		}

		public void Close()
		{
			Session.GetEditor().PointMonitor -= CursorUpdateMonitor;
		}

		void CursorUpdateMonitor(object sender, PointMonitorEventArgs args)
		{
			linesUnderCursor.Clear();

			if (args.Context == null)
			{
				return;
			}

			cursorPosition = args.Context.ComputedPoint;
			cursorPosition.TransformBy(Session.GetEditor().CurrentUserCoordinateSystem.Inverse());

			FullSubentityPath[] entityPaths = args.Context.GetPickedEntities();

			using (Transaction transaction = Session.StartTransaction())
			{
				foreach (FullSubentityPath entityPath in entityPaths)
				{
					Line line = transaction.GetObject(entityPath.GetObjectIds().First(), OpenMode.ForRead) as Line;

					if (line != null && Layer.PipeLayers.Contains(line.Layer))
					{
						linesUnderCursor.Add(line);
					}
				}

				transaction.Commit();
			}
		}

		// Methods --------------------------------------------------------------------------------		
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Click to place...");
			jigPromptPointOptions.Cursor = CursorType.Crosshair;
			jigPromptPointOptions.UserInputControls = UserInputControls.NullResponseAccepted;
			keywordHandler.SetKeywordsForPrompt(jigPromptPointOptions);
			jigPromptPointOptions.Keywords.Add("Undo");

			PromptPointResult promptPointResult = prompts.AcquirePoint(jigPromptPointOptions);
			
			switch (promptPointResult.Status)
			{
				case PromptStatus.Keyword:
					{
						string keyword = promptPointResult.StringResult;

						if (keyword == "Undo")
						{
							return SamplerStatus.NoChange;
						}

						keywordHandler.Consume(transaction, keyword);
						return SamplerStatus.NoChange;
					}
				case PromptStatus.Cancel:
					{
						return SamplerStatus.Cancel;
					}
				case PromptStatus.None:
					{
						Session.Log("None!");
						return SamplerStatus.Cancel;
					}
				case PromptStatus.Error:
					{
						return SamplerStatus.Cancel;
					}
				case PromptStatus.OK:
					{
						UpdateJig(promptPointResult);
						return SamplerStatus.OK;
					}
			}

			return SamplerStatus.Cancel;
		}
		 
		private void UpdateJig(PromptPointResult promptPointResult)
		{
			using (var View = Session.GetEditor().GetCurrentView())
			{
				snapDistance = Math.Min(Session.GlobalSelectDistance(), snapDistanceScreenPercentage * View.Height);
			}

			jigPosition = null;
			jigRotation = null;

			previewSnapPoints.Clear();

			if (jiggedFitting == null || linesUnderCursor.Count == 0)
			{
				return;
			}

			Line selectedLine = null;
			Point3d selectedSnapPoint;
			double selectedRotation;
			double selectedLineDistToCursor = double.MaxValue;

			foreach (Line line in linesUnderCursor)
			{
				Point3d pointOnLine = line.GetClosestPointTo(cursorPosition, false);

				double candidateDistance = cursorPosition.DistanceTo(pointOnLine);

				if (candidateDistance < selectedLineDistToCursor)
				{
					selectedLine = line;
					selectedLineDistToCursor = candidateDistance;
				}
			}

			if (selectedLine != null)
			{
				if (FindSnapPoint(selectedLine, out selectedSnapPoint) && FindSnapRotation(selectedLine, selectedSnapPoint, out selectedRotation))
				{
					jigPosition = selectedSnapPoint;
					jigRotation = selectedRotation;
				}
			}

			if (jigPosition != null && jigRotation != null)
			{
				jiggedFitting.Position = jigPosition.Value;
				jiggedFitting.Rotation = jigRotation.Value;
				jiggedFitting.Layer = selectedLine.Layer;
			}
		}

		bool FindSnapPoint(Line line, out Point3d foundPoint)
		{
			if (line == null)
			{
				foundPoint = Point3d.Origin;
				return false;
			}

			// TODO extract this into a global helper
			System.Windows.Forms.Keys modifierKeys = System.Windows.Forms.Control.ModifierKeys;
			bool shift = (modifierKeys & System.Windows.Forms.Keys.Shift) > 0;
			bool control = (modifierKeys & System.Windows.Forms.Keys.Control) > 0;

			Point3d? closestPoint = null;
			double closestPointDistToCursor = double.MaxValue;

			Point3dCollection candidatePoints = new Point3dCollection();

			List<Entity> candidatePipeEntities = owningCommand.pipeBVH.FindEntities(cursorPosition);

			List<Entity> closePipeEntities = new List<Entity>();

			if (shift)
			{
				Point3d centrePoint = line.StartPoint + 0.5 * (line.EndPoint - line.StartPoint);
				candidatePoints.Add(centrePoint);
				previewSnapPoints.Add(centrePoint);
			}
			else
			{
				candidatePoints = new Point3dCollection() { line.StartPoint, line.EndPoint };
				previewSnapPoints.Add(line.StartPoint);
				previewSnapPoints.Add(line.EndPoint);

				foreach (Entity entity in candidatePipeEntities)
				{
					Line candidateLine = entity as Line;

					if (candidateLine == null)
					{
						continue;
					}

					if (candidateLine.GetClosestPointTo(cursorPosition, false).DistanceTo(cursorPosition) < snapDistance)
					{
						closePipeEntities.Add(entity);
					}
				}

				foreach (Entity otherLine in closePipeEntities)
				{
					if (otherLine == null)
					{
						continue;
					}

					Point3dCollection points = new Point3dCollection();

					line.IntersectWith(otherLine, Intersect.OnBothOperands, /*out*/ points, IntPtr.Zero, IntPtr.Zero);

					if (points.Count != 1)
					{
						continue;
					}

					candidatePoints.Add(points[0]);
					previewSnapPoints.Add(points[0]);
				}
			}

			foreach (Point3d candidatePoint in candidatePoints)
			{
				double distToCursor = candidatePoint.DistanceTo(cursorPosition);

				if (distToCursor > snapDistance)
				{
					continue;
				}

				if (distToCursor < closestPointDistToCursor)
				{
					closestPoint = candidatePoint;
					closestPointDistToCursor = distToCursor;
				}
			}

			if (closestPoint != null)
			{
				foundPoint = closestPoint.Value;
				return true;
			}

			foundPoint = line.GetClosestPointTo(cursorPosition, false);
			return true;
		}

		bool FindSnapRotation(Line line, Point3d snapPoint, out double rotation)
		{
			System.Windows.Forms.Keys mods = System.Windows.Forms.Control.ModifierKeys;
			bool shift = (mods & System.Windows.Forms.Keys.Shift) > 0;
			bool ctrl = (mods & System.Windows.Forms.Keys.Control) > 0;

			Point3d pointOnLineExt = line.GetClosestPointTo(cursorPosition, true);
			Point2d pointOnLineExt2d = new Point2d(pointOnLineExt.X, pointOnLineExt.Y);

			Point3d pointOnLine = line.GetClosestPointTo(cursorPosition, false);
			Point2d pointOnLine2d = new Point2d(pointOnLine.X, pointOnLine.Y);

			Point2d snapPoint2d = new Point2d(snapPoint.X, snapPoint.Y);
			Point2d cursorPos2d = new Point2d(cursorPosition.X, cursorPosition.Y);

			Vector3d lineVector3d = line.EndPoint - line.StartPoint;
			Vector2d perpendicularVector = new Vector2d(lineVector3d.X, lineVector3d.Y).GetPerpendicularVector();

			Line2d line2D = new Line2d(snapPoint2d, perpendicularVector);

			Tolerance tolerance = new Tolerance(0.01 * Session.AutoScaleFactor(), 0.01 * Session.AutoScaleFactor());

			Dictionary<string, double> rotationOffsets = new Dictionary<string, double>()
			{
				{ Blocks.Fitting_Elbow, Math.PI },
				{ Blocks.Fitting_Tee, 0 },
				{ Blocks.Fitting_GroovedCoupling, Math.PI/2 },
				{ Blocks.Fitting_GroovedReducingCoupling, Math.PI/2 },
				{ Blocks.Fitting_Cap, Math.PI },
				{ Blocks.Fitting_Riser, 0 }
			};

			rotation = rotationOffsets.TryGetValueOrDefault(jiggedFitting.Name, 0);

			if (ctrl)
			{
				rotation += Math.PI;
			}

			if (pointOnLineExt2d.IsEqualTo(pointOnLine2d, tolerance))
			{
				if (pointOnLine2d.IsEqualTo(snapPoint2d, tolerance))
				{
					Point2d startPoint = new Point2d(line.StartPoint.X, line.StartPoint.Y);
					Point2d endPoint = new Point2d(line.EndPoint.X, line.EndPoint.Y);

					Vector2d lineVector = endPoint - startPoint;

					rotation += lineVector.Angle;
					return true;
				}
				else
				{
					rotation += (snapPoint2d - pointOnLine2d).Angle;
					return true;
				}
			}
			else
			{
				// Cursor is off the end of the line
				rotation += 0;
				return false;
			}
		}

		public void SetFittingBlock(Transaction transaction, string newBlock)
		{
			EraseJiggedFitting();
			
			jiggedFitting = BlockOps.InsertBlock(transaction, newBlock);
			jiggedFitting.Layer = Layer.DraftAid;
			jiggedFitting.ScaleFactors = new Scale3d(Session.GetBlockScaleFactor());
			jiggedFitting.Visible = false;
		}

		public void EraseJiggedFitting()
		{
			if (jiggedFitting != null)
			{
				jiggedFitting.Erase();
				jiggedFitting = null;
			}
		}

		protected override bool WorldDraw(WorldDraw drawer)
		{

			foreach (FittingTransaction fittingTransaction in owningCommand.transactionStack)
			{
				fittingTransaction.Draw(drawer);
			}
			
			if (jiggedFitting != null && jigPosition != null && jigRotation != null)
			{
				jiggedFitting.Visible = true;
				//jiggedFitting.Draw(drawer);
			}
			else
			{
				jiggedFitting.Visible = false;
				//Session.Log("Can't draw");
			}

			using (Circle snapPreviewCircle = new Circle())
			{

				snapPreviewCircle.Color = Color.FromColorIndex(ColorMethod.ByColor, Colors.VeryDarkGrey);

				snapPreviewCircle.Radius = Math.Min(1000, snapDistance);
				snapPreviewCircle.Center = cursorPosition;
				snapPreviewCircle.Draw(drawer);
			}

			foreach (Point3d snapPoint in previewSnapPoints)
			{
				using (Cross snapMidpoint = new Cross())
				{
					snapMidpoint.Color = Color.FromColorIndex(ColorMethod.ByColor, Colors.Yellow);
					snapMidpoint.Center = snapPoint;
					snapMidpoint.SetScreenSize(0.006);

					Tolerance tolerance = new Tolerance(0.01 * Session.AutoScaleFactor(), 0.01 * Session.AutoScaleFactor());

					if (jiggedFitting.Position.IsEqualTo(snapPoint, tolerance))
					{
						snapMidpoint.Color = Color.FromColorIndex(ColorMethod.ByColor, Colors.Green);
					}
					else
					{
						snapMidpoint.Color = Color.FromColorIndex(ColorMethod.ByColor, Colors.Yellow);
					}

					snapMidpoint.Draw(drawer);
				}
			}

#if DEBUG
			foreach (Polyline3d polyline in pipeBVHPolylines)
			{
				polyline.Draw(drawer);
			}
#endif

			return true;
		}
	}
}
