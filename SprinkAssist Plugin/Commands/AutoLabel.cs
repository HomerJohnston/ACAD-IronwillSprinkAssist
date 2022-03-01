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

[assembly: CommandClass(typeof(Ironwill.AutoLabel))]

namespace Ironwill
{
	public class AutoLabel
	{
		/// ---------------------------------------------------------------------------------------
		/**  */
		[CommandMethod("AutoLabel", CommandFlags.UsePickSet)]
		public void AutoLabelCmd()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database database = doc.Database;
			Editor editor = doc.Editor;

			TypedValue[] filter = {
				new TypedValue((int)DxfCode.Operator, "<or"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Armover"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Branchline"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Main"),
				new TypedValue((int)DxfCode.LayerName, "SpkPipe_Drain"),
				new TypedValue((int)DxfCode.Operator, "or>"),
			};

			PromptSelectionResult selectionResult = editor.GetSelection(new SelectionFilter(filter));

			if (selectionResult.Status != PromptStatus.OK)
			{
				return;
			}

			PipeLabelDialog pipeLabelDialog = ShowPipeLabelDialog();

			if (!pipeLabelDialog.okPressed)
				return;

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				SelectionSet selectionSet = selectionResult.Value;

				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				List<Point3d> breakPoints = GetBreakPoints(true, true);

				foreach (ObjectId objectId in selectionSet.GetObjectIds())
				{
					Line line = transaction.GetObject(objectId, OpenMode.ForRead) as Line;
					if (line == null)
					{
						// TODO: Log warning
						continue;
					}

					string labelBlockName = GetLabelBlockName(line.Layer, pipeLabelDialog.groupID);
					if (!blockTable.Has(labelBlockName))
					{
						// TODO: Log error
						continue;
					}

					List<Line> segments = GetLineSegments(line, ref breakPoints);

					foreach (Line segment in segments)
					{
						LabelLine(line, segment, pipeLabelDialog, labelBlockName, transaction);
					}
				}

				transaction.Commit();
			}
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		protected PipeLabelDialog ShowPipeLabelDialog()
		{
			PipeLabelDialog pipeLabelDialog = new PipeLabelDialog();

			AcApplication.ShowModalDialog(null, pipeLabelDialog, false);

			return pipeLabelDialog;
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static string GetLabelBlockName(string lineLayer, string groupID)
		{
			// TODO new labelling system
			string labelBlockName = "PipeLabel_";

			string branchline = Layers.SystemPipe_Branchline.Get();

			if (lineLayer == Layers.SystemPipe_Branchline.Get())
			{
				labelBlockName += "BL-";
			}
			else if (lineLayer == Layers.SystemPipe_Main.Get())
			{
				labelBlockName += "MN-";
			}
			else if (lineLayer == Layers.SystemPipe_Armover.Get())
			{
				labelBlockName += "AO-";
			}
			else
			{
				return "";
			}

			labelBlockName += groupID;

			return labelBlockName;
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static List<Point3d> GetBreakPoints(bool breakOnBlocks, bool breakOnLines)
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

					TryGetBreakPointsAsBlock(dbObject, ref points);

					TryGetBreakPointsAsLine(dbObject, ref points);
				}
			}

			return points;
		}

		private static void TryGetBreakPointsAsBlock(DBObject dbObject, ref List<Point3d> breakPoints)
		{
			BlockReference block = dbObject as BlockReference;

			if (block == null)
				return;

			var blockLayers = new List<string>() { "SpkSystem_Fitting", "SpkSystem_Head", "SpkSystem_Device", "SpkPipe_Armover", "SpkPipe_Branchline", "SpkPipe_Main" };
			
			if (!blockLayers.Contains(block.Layer))
				return;

			breakPoints.Add(block.Position);
		}

		private static void TryGetBreakPointsAsLine(DBObject dbObject, ref List<Point3d> breakPoints)
		{
			Line line = dbObject as Line;

			if (line == null)
				return;

			var LineLayers = new List<string>() { "SpkPipe_Armover", "SpkPipe_Branchline", "SpkPipe_Main" };

			if (!LineLayers.Contains(line.Layer))
				return;

			breakPoints.Add(line.StartPoint);
			breakPoints.Add(line.EndPoint);
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		private static List<Line> GetLineSegments(Line line, ref List<Point3d> breakPoints)
		{
			List<Line> segments = new List<Line>();

			// iterate over all sprinkler and fitting blocks. If a block is on the line, break the line into two and restart this process on both segments.
			Document document = AcApplication.DocumentManager.MdiActiveDocument;

			Database database = document.Database;

			using (Transaction transaction = database.TransactionManager.StartTransaction())
			{
				bool lineSplit = false;

				foreach (Point3d blockPoint in breakPoints)
				{
					const double threshold = 0.5;

					Point3d closestPoint = line.GetClosestPointTo(blockPoint, false);

					double distance = closestPoint.DistanceTo(blockPoint);

					if (distance <= threshold)
					{
						// Ignore points on the ends of the line
						if (blockPoint.DistanceTo(line.StartPoint) < threshold || blockPoint.DistanceTo(line.EndPoint) < threshold)
							continue;

						Line firstSegment = new Line(line.StartPoint, closestPoint);
						Line secondSegment = new Line(closestPoint, line.EndPoint);

						List<Line> firstSegmentSegments = GetLineSegments(firstSegment, ref breakPoints);
						List<Line> secondSegmentSegments = GetLineSegments(secondSegment, ref breakPoints);

						segments.AddRange(firstSegmentSegments);
						segments.AddRange(secondSegmentSegments);

						lineSplit = true;

						break;
					}
				}

				if (!lineSplit)
				{
					segments.Add(line);
				}
			}

			return segments;
		}

		/// ---------------------------------------------------------------------------------------
		/**  */
		struct TextInfo
		{
			public Point3d Position { get; private set; }
			public Point3d Alignment { get; private set; }
			public bool IsAligned { get; private set; }
			public double Rotation { get; private set; }
			public TextInfo(Point3d position, Point3d alignment, bool aligned, double rotation)
			{
				Position = position;
				Alignment = alignment;
				IsAligned = aligned;
				Rotation = rotation;
			}
		}

		class InsertBlockJig : EntityJig
		{
			BlockReference br;
			Point3d pt;
			Dictionary<string, TextInfo> attInfos;

			public InsertBlockJig(BlockReference br, Dictionary<string, TextInfo> attInfos) : base(br)
			{
				this.br = br;
				this.attInfos = attInfos;
			}

			protected override SamplerStatus Sampler(JigPrompts prompts)
			{
				var options = new JigPromptPointOptions("\nSpecify insertion point: ");
				options.UserInputControls = UserInputControls.Accept3dCoordinates;
				var result = prompts.AcquirePoint(options);
				if (result.Value.IsEqualTo(pt))
					return SamplerStatus.NoChange;
				pt = result.Value;
				return SamplerStatus.OK;
			}

			protected override bool Update()
			{
				br.Position = pt;
				if (br.AttributeCollection.Count > 0)
				{
					var tr = br.Database.TransactionManager.TopTransaction;
					foreach (ObjectId id in br.AttributeCollection)
					{
						var attRef = (AttributeReference)tr.GetObject(id, OpenMode.ForRead);
						string tag = attRef.Tag.ToUpper();
						if (attInfos.ContainsKey(tag))
						{
							TextInfo ti = attInfos[tag];
							attRef.Position = ti.Position.TransformBy(br.BlockTransform);
							if (ti.IsAligned)
							{
								attRef.AlignmentPoint =
									ti.Alignment.TransformBy(br.BlockTransform);
								attRef.AdjustAlignment(br.Database);
							}
							if (attRef.IsMTextAttribute)
							{
								attRef.UpdateMTextAttribute();
							}
						}
					}
				}
				return true;
			}
		}

		protected double GetScaleFactor()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			int Lunits = db.Lunits;

			switch (Lunits)
			{
				case 2:
					{
						return 100.0;
					}
				case 4:
					{
						//return 96.0;
						return 3.93701;
					}
				default:
					{
						return 1.0;
					}
			}
		}

		protected double Degrees(double inRadians)
		{
			return inRadians / Math.PI * 180.0;
		}

		protected double Radians(double inDegrees)
		{
			return inDegrees * Math.PI / 180.0;
		}

		protected void LabelLine(Line line, Line segment, PipeLabelDialog pipeLabelDialog, string labelBlockName, Transaction transaction)
		{
			Vector3d startToEndVector = segment.StartPoint.GetVectorTo(segment.EndPoint);
			Point3d midPoint = segment.StartPoint + 0.5f * startToEndVector;

			double actualLength = pipeLabelDialog.GetSlopeLengthMultiplier() * segment.Length;

			int displayLength = 0;
			var attributeText = new Dictionary<string, string>();

			bool bShowLength = false;

			double omitLengthThreshold = PipeLabelDialog.omitLengthLabelLength;
			double skipLabelThreshold = PipeLabelDialog.ignoreLineLength;

			if (line.Length < skipLabelThreshold)
				return;

			switch (line.Layer)
			{
				case "SpkPipe_Armover":
					attributeText["DIA"] = PipeLabelDialog.armoverLabel;
					bShowLength = PipeLabelDialog.showArmoverLengths;
					break;
				case "SpkPipe_Branchline":
					attributeText["DIA"] = PipeLabelDialog.branchlineLabel;
					bShowLength = PipeLabelDialog.showBranchlineLengths;
					break;
				case "SpkPipe_Main":
					attributeText["DIA"] = PipeLabelDialog.mainLabel;
					bShowLength = PipeLabelDialog.showMainLengths;
					break;
				default:
					attributeText["DIA"] = "ERROR";
					break;
			}

			if (bShowLength && line.Length > omitLengthThreshold)
			{
				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Metric:
						displayLength = (int)Math.Round(actualLength / 25) * 25;

						if (displayLength < 100)
						{
							return;
						}

						attributeText["LGTH"] = displayLength.ToString();
						break;

					case DrawingUnits.Imperial:
						displayLength = (int)Math.Round(actualLength);

						if (displayLength < 4)
						{
							return;
						}

						int feet = displayLength / 12;
						int inches = displayLength % 12;

						attributeText["LGTH"] = feet.ToString() + "'-" + inches.ToString() + "\"";
						break;
				}
			}

			BlockReference blockReference = BlockDictionary.InsertBlock(labelBlockName);

			if (blockReference == null)
			{
				// TODO: Log error
				return;
			}

			Vector3d segmentDirection = (segment.EndPoint - segment.StartPoint).GetNormal();

			
			blockReference.Position = midPoint;
			blockReference.ScaleFactors = new Scale3d(GetScaleFactor());
			blockReference.Rotation = Session.SanitizeAngle(segment.Angle, segmentDirection);
			blockReference.Layer = "SpkPipeLabel";

			BlockDictionary.SetBlockAttributes(transaction, blockReference, attributeText);
		}
	}
}
