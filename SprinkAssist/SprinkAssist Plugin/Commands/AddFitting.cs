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

[assembly: CommandClass(typeof(Ironwill.AddFitting))]


namespace Ironwill
{
	public class AddFitting
	{
		const string ElbowKeyword = "Elbow";
		const string TeeKeyword = "Tee";
		const string CapKeyword = "Cap";
		const string RiserKeyword = "Riser";
		const string ReducerKeyword = "REducer";
		const string CouplingKeyword = "COupling";

		readonly IList<string> Keywords = new ReadOnlyCollection<string>( new List<string> { ElbowKeyword, TeeKeyword, CapKeyword, RiserKeyword, ReducerKeyword, CouplingKeyword } );

		StringSetting fittingTypeSetting = new StringSetting(new DictionaryPath("AddFitting"), "SelectedFitting", ElbowKeyword);

		[CommandMethod("SpkAssist_AddFitting")]
		public void AddFittingCmd()
		{
			PromptEntityOptions promptEntityOptions = new PromptEntityOptions("\nPlace " + fittingTypeSetting.stringValue);

			foreach (string key in Keywords)
			{
				promptEntityOptions.Keywords.Add(key);
			}

			promptEntityOptions.Keywords.Default = fittingTypeSetting.stringValue;

			bool bStopCommand = false;

			using (new WorldUCS())
			{
				while (!bStopCommand)
				{
					using (Transaction transaction = Session.StartTransaction())
					{
						PromptEntityResult promptEntityResult = Session.GetEditor().GetEntity(promptEntityOptions);

						switch (promptEntityResult.Status)
						{
							case PromptStatus.Keyword:
								fittingTypeSetting.Set(promptEntityResult.StringResult);
								promptEntityOptions.Message = "Place " + promptEntityResult.StringResult;
								promptEntityOptions.Keywords.Default = promptEntityResult.StringResult;
								break;
							case PromptStatus.Cancel:
								bStopCommand = true;
								break;
							case PromptStatus.OK:
								Line pickedLine = transaction.GetObject(promptEntityResult.ObjectId, OpenMode.ForRead) as Line;

								if (pickedLine == null)
								{
									Session.Log("You must pick a line");
									continue;
								}

								Point3d cursorPoint = promptEntityResult.PickedPoint;
								Point3d fittingPosition;

								string fittingName = GetFittingName();
								bool succ = FindPlacementPoint(transaction, cursorPoint, pickedLine, out fittingPosition);
								double fittingRotation = GetFittingRotation(cursorPoint, fittingPosition, pickedLine);
								string fittingLayer = pickedLine.Layer;

								PlaceFitting(fittingName, fittingPosition, fittingRotation, fittingLayer);

								break;
						}

						transaction.Commit();
					}
				}
			}
		}

		string GetFittingName()
		{
			switch (fittingTypeSetting.stringValue)
			{
				case ElbowKeyword:
					return Blocks.Fitting_Elbow.Get();
				case TeeKeyword:
					return Blocks.Fitting_Tee.Get();
				case CapKeyword:
					return Blocks.Fitting_Cap.Get();
				case RiserKeyword:
					return Blocks.Fitting_Riser.Get();
				case CouplingKeyword:
					return Blocks.Fitting_GroovedCoupling.Get();
				case ReducerKeyword:
					return Blocks.Fitting_GroovedReducingCoupling.Get();
			}

			return "";
		}

		bool FindPlacementPoint(Transaction transaction, Point3d cursorPoint, Line pickedLine, out Point3d foundPoint)
		{
			Point3d closestCandidatePoint = new Point3d(double.MaxValue, double.MaxValue, double.MaxValue);

			bool foundFittingPoint = false;

			BlockTableRecord blockTableRecord = Session.GetBlockTableRecord(transaction);

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
			blockRef.ScaleFactors = new Scale3d(Session.GetScaleFactor());
			blockRef.Layer = layer;
		}
	}
}
