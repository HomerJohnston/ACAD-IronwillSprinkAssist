using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Ironwill.Commands.Help;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Geometry;
using static Ironwill.ModelSpaceHelper;
using Ironwill.MessageFilters;
using Ironwill.Structures;
using static System.Net.Mime.MediaTypeNames;

[assembly: CommandClass(typeof(Ironwill.Commands.DragEntityCmd))]


namespace Ironwill.Commands
{
	public class DragEntityCmd
	{
		[CommandDescription("Drags an entity. Drags pipe perpendicularly left or right. Drags sprinklers arbitrarily.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DragEntity", CommandFlags.Modal)]
		public void Main()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				PromptEntityResult promptEntityResult = null;

				Entity draggedEntity = null;

				while (promptEntityResult == null || (promptEntityResult.Status == PromptStatus.OK && draggedEntity == null))
				{
					PromptEntityOptions promptEntityOptions = new PromptEntityOptions("Pick a pipe or sprinkler head");

					promptEntityResult = Session.GetEditor().GetEntity(promptEntityOptions);

                    ObjectId objectId = promptEntityResult.ObjectId;

					if (!objectId.IsValid)
					{
						continue;
					}

					draggedEntity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;
				}

				if (draggedEntity == null)
				{
					return;
				}

				switch (draggedEntity)
				{
					case Line draggedLine:
						{
							if (Layer.PipeLayers.Contains(draggedLine.Layer))
							{
								DragPipe(transaction, draggedLine);
							}
							break;
						}
					case BlockReference draggedBlockReference:
						{
							if (draggedBlockReference.Layer == Layer.SystemHead)
							{
								DragHead(transaction, draggedBlockReference);
							}
							break;
						}
				}
			}
		}

		void DragPipe(Transaction transaction, Line draggedLine)
		{
			BoundingVolumeHierarchyManager.Rebuild(transaction);

			List<Entity> nearbyEntities = BoundingVolumeHierarchyManager.Get().FindEntities(draggedLine.GeometricExtents);

			// Iterate through every drawing element to find things that are on or touching the line to be dragged
			List<Line> attachedLines = new List<Line>();
			List<BlockReference> attachedSprinklerHeads = new List<BlockReference>();
			List<BlockReference> attachedFittings = new List<BlockReference>();

			foreach (Entity nearbyEntity in nearbyEntities)
			{
				if (nearbyEntity == draggedLine)
				{
					continue;
				}

				switch (nearbyEntity)
				{
					case Line nearbyLine:
						{
							if (!Layer.PipeLayers.Contains(nearbyLine.Layer))
							{
								continue;
							}

							List<Point3d> nearbyLineAttachPoints = new List<Point3d>()
								{
									nearbyLine.StartPoint,
									nearbyLine.EndPoint
								};

							foreach (Entity nearbyEntity2 in nearbyEntities)
							{
								BlockReference nearbyBlockReference = nearbyEntity2 as BlockReference;

								if (nearbyBlockReference == null)
								{
									continue;
								}

								if (nearbyBlockReference.IsOnLine(nearbyLine, BlockReferenceExtensions.LineTest.EndsOnly))
								{
									nearbyLineAttachPoints.Add(nearbyBlockReference.Position);
								}
							}

							bool bNearbyLineIsAttachedToDraggedLine = false;

							foreach (Point3d potentialAttachPoint in nearbyLineAttachPoints)
							{
								if (potentialAttachPoint.IsOnLine(draggedLine, 0.01))
								{
									bNearbyLineIsAttachedToDraggedLine = true;
									break;
								}
							}

							if (bNearbyLineIsAttachedToDraggedLine)
							{
								attachedLines.Add(nearbyLine);
							}

							break;
						}
					case BlockReference nearbyBlockReference:
						{
							if (nearbyBlockReference.Layer != Layer.SystemHead && nearbyBlockReference.Layer != Layer.SystemFitting && !Layer.PipeLayers.Contains(nearbyBlockReference.Layer))
							{
								continue;
							}

							if (!nearbyBlockReference.IsOnLine(draggedLine, BlockReferenceExtensions.LineTest.EndsOnly))
							{
								continue;
							}

							if (nearbyBlockReference.Layer == Layer.SystemHead)
							{
								attachedSprinklerHeads.Add(nearbyBlockReference);
							}
							else
							{
								attachedFittings.Add(nearbyBlockReference);

								Handle test = new Handle();


								Session.Log(test.Value.ToString());
							}

							break;
						}
				}
			}

			DragPipeJigger jigger = new DragPipeJigger(transaction, draggedLine, attachedLines, attachedSprinklerHeads);

			PromptResult jigResult = Session.GetEditor().Drag(jigger);

			if (jigResult != null)
			{
				switch (jigResult.Status)
				{
					case PromptStatus.OK:
						{
							if (jigger.deltaVector.Length < 0.01)
							{
								break;
							}

							draggedLine.UpgradeOpen();

							(draggedLine).StartPoint += jigger.deltaVector;
							(draggedLine).EndPoint += jigger.deltaVector;

							foreach (Line newArmover in jigger.newJiggedSprinklerArmovers)
							{
								if (newArmover.Length > 0.01)
								{
									Session.GetModelSpaceBlockTableRecordForWrite(transaction).AppendEntity(newArmover);
									transaction.AddNewlyCreatedDBObject(newArmover, true);
								}
							}

							List<double> attachedFittingLineRotations = new List<double>();

						/*
							for (int i = 0; i < attachedFittings.Count; ++i)
							{
								Handle attachedLine = new Handle();

								if (attachedFittings[i].GetXRecord(transaction, "SprinkAssist_FittingLineHandleTest", ref attachedLine))
								{
									ObjectId attachedLineId;

									if (Session.GetDatabase().TryGetObjectId(attachedLine, out attachedLineId))
									{
										Line lineEntity = transaction.GetObject(attachedLineId, OpenMode.ForWrite) as Line;

										if (lineEntity != null)
										{
											attachedFittingLineRotations.Add(lineEntity.Angle);
										}
									}
								}
							}
						*/
							for (int i = 0; i < jigger.stretchedLines.Count; ++i)
							{
								jigger.stretchedLines[i].Apply();
							}

						/*
							for (int i = 0; i < attachedFittings.Count; ++i)
							{
								attachedFittings[i].UpgradeOpen();
								attachedFittings[i].Position += jigger.deltaVector;

								Handle attachedLine = new Handle();

								if (attachedFittings[i].GetXRecord(transaction, "SprinkAssist_FittingLineHandleTest", ref attachedLine))
								{
									ObjectId attachedLineId;

									if (Session.GetDatabase().TryGetObjectId(attachedLine, out attachedLineId))
									{
										Line lineEntity = transaction.GetObject(attachedLineId, OpenMode.ForWrite) as Line;

										if (lineEntity != null)
										{
											double rot = attachedFittingLineRotations[i] - lineEntity.Angle;

											Session.Log("Rot... " + rot.ToString());

											attachedFittings[i].Rotation += rot;
										}
									}
								}
							}
						*/

							transaction.Commit();

							break;
						}
				}
			}

			// For some reason calling dispose of jig lines in destructor spams output log with warnings about forgetting to call dispose. Why?? Use a separate manually called method instead.
			jigger.Destroy();
		}

		void DragHead(Transaction transaction, BlockReference draggedHead)
		{

		}
	}

	// =================================================================================================================================================================================================


	// =================================================================================================================================================================================================

	internal class DragPipeJigger : DrawJig
	{
		// Settings
		Transaction transaction;

		Line draggedLine;

		List<BlockReference> heads;

		ModifierKey modifierKeyFilter;

		// Runtime State
		public Point3d cursorPosition;

		public Vector3d deltaVector;

		public List<Line> newJiggedSprinklerArmovers = new List<Line>();
				
		List<bool> jiggedLinesStartConnected = new List<bool>();

		Line draggedLineJigLine;

		Point3d pointMonitorPosition;

		public List<StretchingLine> stretchedLines = new List<StretchingLine>();

		public DragPipeJigger(Transaction inTransaction, Line inDraggedLine, List<Line> inLines, List<BlockReference> inHeads)
		{
			transaction = inTransaction;
			draggedLine = inDraggedLine;
			heads = inHeads;

			foreach (BlockReference sprinkler in heads)
			{
				Line newArmover = new Line();
				newArmover.Layer = Layer.SystemPipe_Armover;

				newJiggedSprinklerArmovers.Add(newArmover);
			}

			draggedLineJigLine = new Line();

			draggedLineJigLine.StartPoint = draggedLine.StartPoint;
			draggedLineJigLine.EndPoint = draggedLine.EndPoint;
			draggedLineJigLine.Layer = draggedLine.Layer;
			draggedLineJigLine.Linetype = draggedLine.Linetype;
			draggedLineJigLine.Transparency = new Autodesk.AutoCAD.Colors.Transparency(150);

			foreach (Line line in inLines)
			{
				StretchingLine stretchingLine = new StretchingLine(line, draggedLine, draggedLineJigLine);
				stretchedLines.Add(stretchingLine);
			}

			Session.GetEditor().PointMonitor += PointMonitor;
		}

		private void PointMonitor(object sender, PointMonitorEventArgs e)
		{
			pointMonitorPosition = e.Context.ComputedPoint;
			cursorPosition = pointMonitorPosition;
		}

		public void Destroy()
		{
			for (int i = 0; i < stretchedLines.Count; ++i)
			{
				stretchedLines[i].Destroy();
			}

			for (int i = 0; i < newJiggedSprinklerArmovers.Count; ++i)
			{
				newJiggedSprinklerArmovers[i].Dispose();
			}

			draggedLineJigLine.Dispose();

			Session.GetEditor().PointMonitor -= PointMonitor;
		}

		// API
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Click to place...");
			jigPromptPointOptions.UseBasePoint = false;
			jigPromptPointOptions.UserInputControls = UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates /* | UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode*/;
			
			PromptPointResult promptPointResult = prompts.AcquirePoint(jigPromptPointOptions);

			switch (promptPointResult.Status)
			{
				case PromptStatus.Cancel:
					{
						return SamplerStatus.Cancel;
					}
				case PromptStatus.Error:
					{
						return SamplerStatus.Cancel;
					}
				case PromptStatus.None:
					{
						return SamplerStatus.Cancel;
					}
				case PromptStatus.OK:
					{
						//cursorPosition = promptPointResult.Value;

						break;
					}
			}

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw drawer)
		{
			Point3d pointOnOriginalLine = draggedLine.GetClosestPointTo(cursorPosition, true);

			deltaVector = cursorPosition - pointOnOriginalLine;

			draggedLineJigLine.StartPoint = draggedLine.StartPoint + deltaVector;

			draggedLineJigLine.EndPoint = draggedLine.EndPoint + deltaVector;

			draggedLineJigLine.Draw(drawer);

			for (int i = 0; i < heads.Count; ++i)
			{
				Line newArmover = newJiggedSprinklerArmovers[i];
				BlockReference head = heads[i];
				newArmover.StartPoint = head.Position;
				newArmover.EndPoint = head.Position + deltaVector;

				newArmover.Draw(drawer);
			}

			for (int i = 0; i < stretchedLines.Count; ++i)
			{
				stretchedLines[i].Update(drawer);
			}

			return true;
		}
	}
}
