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
using Autodesk.AutoCAD.GraphicsInterface;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.OrientView))]

namespace Ironwill
{
	public class OrientView
	{
		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist_OrientView")]
		public void OrientViewCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Editor editor = Session.GetEditor();
				Database database = Session.GetDatabase();

				bool initialOrthoMode = database.Orthomode;
				
				database.Orthomode = false;

				PromptPointOptions promptPointOptions1 = new PromptPointOptions("\nPick first point (origin)");
				PromptPointResult promptPointResult1 = editor.GetPoint(promptPointOptions1);

				if (promptPointResult1.Status != PromptStatus.OK)
				{
					transaction.Commit();
					return;
				}

				OrientViewDrawJig jig = new OrientViewDrawJig(promptPointResult1.Value);
				PromptResult promptResult = editor.Drag(jig);

				if (promptResult.Status != PromptStatus.OK)
				{
					transaction.Commit();
					return;
				}

				Vector3d baseVector = jig.basePoint.GetAsVector();
				Vector3d endVector = jig.dragPoint.GetAsVector();

				Vector3d dragVector = endVector - baseVector;
				Vector3d localXAxis = Vector3d.XAxis.TransformBy(editor.CurrentUserCoordinateSystem);

				double angle = -localXAxis.GetAngleTo(dragVector, Vector3d.ZAxis);

				SetUcsBy2Points(jig.basePoint, jig.dragPoint);

				ViewTableRecord view = editor.GetCurrentView();

				Matrix3d initialDCStoWCS =
					Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
					Matrix3d.Displacement(view.Target - Point3d.Origin) *
					Matrix3d.PlaneToWorld(view.ViewDirection);

				Matrix3d endDCStoWCS =
					Matrix3d.Rotation(-view.ViewTwist - angle, view.ViewDirection, view.Target) *
					Matrix3d.Displacement(view.Target - Point3d.Origin) *
					Matrix3d.PlaneToWorld(view.ViewDirection);

				Point3d pp = new Point3d(view.CenterPoint.X, view.CenterPoint.Y, 0.0);
				pp = pp.TransformBy(initialDCStoWCS);
				pp = pp.TransformBy(endDCStoWCS.Inverse());

				view.CenterPoint = new Point2d(pp.X, pp.Y);
				view.ViewTwist += angle;
				
				editor.SetCurrentView(view);

				database.Orthomode = initialOrthoMode;

				transaction.Commit();
			}
		}

		[CommandMethod("SpkAssist_ResetView")]
		public void ResetViewCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				Editor editor = Session.GetEditor();

				Database database = Session.GetDatabase();

				bool initialOrthoMode = database.Orthomode;
				
				database.Orthomode = false;

				ViewTableRecord view = editor.GetCurrentView();

				Matrix3d initialDCStoWCS =
					Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
					Matrix3d.Displacement(view.Target - Point3d.Origin) *
					Matrix3d.PlaneToWorld(view.ViewDirection);

				Point3d pp = new Point3d(view.CenterPoint.X, view.CenterPoint.Y, 0.0);
				pp = pp.TransformBy(initialDCStoWCS);

				view.CenterPoint = new Point2d(pp.X, pp.Y);
				view.ViewTwist = 0;

				editor.SetCurrentView(view);

				database.Orthomode = initialOrthoMode;

				// TODO restore at same viewpoint centre
				Session.GetDocument().SendStringToExecute("ucs\n\n", false, false, true);

				transaction.Commit();
			}
		}

		private void SetUcsBy2Points(Point3d pt1, Point3d pt2)
		{
			Editor ed = Session.GetEditor();

			CoordinateSystem3d ucs = ed.CurrentUserCoordinateSystem.CoordinateSystem3d;
			
			Vector3d xAxis = pt1.GetVectorTo(pt2).GetNormal();
			Vector3d zAxis = Vector3d.ZAxis;
			Vector3d yAxis;

			if (xAxis.IsEqualTo(zAxis))
			{
				yAxis = ucs.Yaxis;
				zAxis = xAxis.CrossProduct(yAxis).GetNormal();
			}
			else
			{
				yAxis = zAxis.CrossProduct(xAxis).GetNormal();
			}

			Matrix3d mat = new Matrix3d(new double[]{
				xAxis.X, yAxis.X, zAxis.X, pt1.X,
				xAxis.Y, yAxis.Y, zAxis.Y, pt1.Y,
				xAxis.Z, yAxis.Z, zAxis.Z, pt1.Z,
				0.0, 0.0, 0.0, 1.0});
			ed.CurrentUserCoordinateSystem = mat;
		}

		class OrientViewDrawJig : DrawJig
		{
			public Point3d basePoint;
			public Point3d dragPoint;

			Line xAxis;
			Line yAxis;

			public OrientViewDrawJig(Point3d basePoint)
			{
				double ucsSize = 0.0;
				
				switch (Session.GetPrimaryUnits())
				{
					case DrawingUnits.Imperial:
						ucsSize = 48.0;
						break;
					case DrawingUnits.Metric:
						ucsSize = 1220.0;
						break;
				}

				basePoint = SprinkMath.ConvertFromUCStoWCS(basePoint);
				this.basePoint = basePoint;

				xAxis = new Line(basePoint, basePoint + new Point3d(ucsSize, 0, 0).GetAsVector());
				yAxis = new Line(basePoint, basePoint + new Point3d(0, ucsSize, 0).GetAsVector());

				xAxis.ColorIndex = 1;
				yAxis.ColorIndex = 3;
			}

			protected override SamplerStatus Sampler(JigPrompts prompts)
			{
				JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions("\nPick second point (+x Axis)");
				jigPromptPointOptions.BasePoint = this.basePoint;
				jigPromptPointOptions.UseBasePoint = true;
				jigPromptPointOptions.Cursor = CursorType.RubberBand;
				jigPromptPointOptions.UserInputControls = UserInputControls.Accept3dCoordinates;
				PromptPointResult promptPointResult = prompts.AcquirePoint(jigPromptPointOptions);

				if (promptPointResult.Value.IsEqualTo(this.dragPoint))
				{
					return SamplerStatus.NoChange;
				}

				this.dragPoint = promptPointResult.Value;

				return SamplerStatus.OK;
			}
			
			protected override bool WorldDraw(WorldDraw draw)
			{
				Vector3d basePointV = basePoint.GetAsVector();
				Vector3d dragPointV = dragPoint.GetAsVector();

				Vector3d v = dragPointV - basePointV;

				double angle = Vector3d.XAxis.GetAngleTo(v, Vector3d.ZAxis);

				WorldGeometry worldGeometry = draw.Geometry;
				Matrix3d displacement = Matrix3d.Displacement(this.basePoint.GetVectorTo(this.dragPoint));
				Matrix3d rotation = Matrix3d.Rotation(angle, Vector3d.ZAxis, basePoint);

				worldGeometry.PushModelTransform(rotation);

				worldGeometry.Draw(xAxis);
				worldGeometry.Draw(yAxis);

				worldGeometry.PopModelTransform();

				return true;
			}
		}
	}
}
