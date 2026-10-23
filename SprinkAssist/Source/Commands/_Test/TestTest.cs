#region Namespaces

using System;
using System.Text;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;

using Autodesk.AutoCAD.GraphicsSystem;
using Autodesk.AutoCAD.GraphicsInterface;

using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.ComponentModel;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DataExtraction;
using Autodesk.AutoCAD.Diagnostics;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.LayerManager;
using Autodesk.AutoCAD.MacroRecorder;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Ribbon;

using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using MgdAcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcWindowsNS = Autodesk.AutoCAD.Windows;
using Ironwill;

#endregion

[assembly: CommandClass(typeof(AcadNetAddinWizard_Namespace.DrawJigger6))]

namespace AcadNetAddinWizard_Namespace
{
	public class DrawJigger6 : DrawJig
	{
		#region Fields

		private Point3dCollection mAllVertexes = new Point3dCollection();
		private Point3d mLastVertex;

		#endregion

		#region Constructors

		public DrawJigger6()
		{
		}

		~DrawJigger6()
		{
		}

		#endregion

		#region Properties

		public Point3d LastVertex
		{
			get { return mLastVertex; }
			set { mLastVertex = value; }
		}

		private Editor Editor
		{
			get
			{
				return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
			}
		}

		public Matrix3d UCS
		{
			get
			{
				return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem;
			}
		}

		#endregion

		#region Overrides

		protected override bool WorldDraw(Autodesk.AutoCAD.GraphicsInterface.WorldDraw draw)
		{
			WorldGeometry geo = draw.Geometry;
			if (geo != null)
			{
				geo.PushModelTransform(UCS);

				Point3dCollection tempPts = new Point3dCollection();
				foreach (Point3d pt in mAllVertexes)
				{
					tempPts.Add(pt);
				}
				if (mLastVertex != null)
					tempPts.Add(mLastVertex);
				if (tempPts.Count > 0)
					geo.Polyline(tempPts, Vector3d.ZAxis, IntPtr.Zero);

				geo.PopModelTransform();
			}

			return true;
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\nVertex(Enter to finish)");
			prOptions1.UseBasePoint = false;
			prOptions1.UserInputControls =
				UserInputControls.NullResponseAccepted | UserInputControls.Accept3dCoordinates |
				UserInputControls.GovernedByUCSDetect | UserInputControls.GovernedByOrthoMode |
				UserInputControls.AcceptMouseUpAsPoint;

			PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
			if (prResult1.Status == PromptStatus.Cancel || prResult1.Status == PromptStatus.Error)
				return SamplerStatus.Cancel;

			Point3d tempPt = prResult1.Value.TransformBy(UCS.Inverse());
			mLastVertex = tempPt;

			return SamplerStatus.OK;
		}

		#endregion

		#region Commands

		[CommandMethod("TestDrawJigger6")]
		public void TestDrawJigger6_Method()
		{
			Database db = HostApplicationServices.WorkingDatabase;
			DrawJigger6 jigger = new DrawJigger6();
			PromptResult jigRes;
			do
			{
				jigRes = Session.GetEditor().Drag(jigger);
				if (jigRes.Status == PromptStatus.OK)
					jigger.mAllVertexes.Add(jigger.mLastVertex);
			} while (jigRes.Status == PromptStatus.OK);

			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				tr.Commit();
			}
		}

		#endregion

	}
}