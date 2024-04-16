using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using AcTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
using static Autodesk.AutoCAD.EditorInput.Editor;
using Autodesk.AutoCAD.Internal;
using System.Windows.Forms;

namespace Ironwill
{
	public static class Session
	{
		public static DocumentCollection GetDocumentManager()
		{
			return AcApplication.DocumentManager;
		}

		public static Document GetDocument()
		{
			return GetDocumentManager().MdiActiveDocument;
		}

		public static DocumentLock LockDocument()
		{
			return GetDocument().LockDocument();
		}

		public static Transaction StartTransaction()
		{
			return GetDocument().TransactionManager.StartTransaction();
		}

		public static void StartUndoTransaction()
		{
			throw new NotImplementedException();
		}

		public static Transaction TopTransaction()
		{
			return GetDocument().TransactionManager.TopTransaction;
		}

		public static Database GetDatabase()
		{
			return GetDocument().Database;
		}

		public static Editor GetEditor()
		{
			return GetDocument().Editor;
		}

		public static LayerTable GetLayerTable(Transaction transaction, OpenMode openMode = OpenMode.ForRead)
		{
			return transaction.GetObject(GetDatabase().LayerTableId, openMode) as LayerTable;
		}

		public static BlockTableRecord GetModelSpaceBlockTableRecord(Transaction transaction)
		{
			return transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(GetDatabase()), OpenMode.ForRead) as BlockTableRecord;
		}

		public static void AddNewObject(Transaction transaction, Entity newEntity)
		{
			BlockTableRecord blockTableRecord = GetModelSpaceBlockTableRecord(transaction);
			
			blockTableRecord.UpgradeOpen();
			blockTableRecord.AppendEntity(newEntity);
			transaction.AddNewlyCreatedDBObject(newEntity, true);
			blockTableRecord.DowngradeOpen();
		}

		public static DrawingUnits GetPrimaryUnits()
		{
			int Lunits = GetDatabase().Lunits;

			switch (Lunits)
			{
				case 2:
					return DrawingUnits.Metric;
				case 4:
					return DrawingUnits.Imperial;
				default:
					return DrawingUnits.Undefined;
			}
		}

		public static DrawingUnits GetSecondaryUnits()
		{
			int Lunits = GetDatabase().Lunits;

			switch (Lunits)
			{
				case 2:
					return DrawingUnits.Imperial;
				case 4:
					return DrawingUnits.Metric;
				default:
					return DrawingUnits.Undefined;
			}
		}

		public static double GetBlockScaleFactor()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			int Lunits = db.Lunits;

			switch (Lunits)
			{
				case 2:
					return 100.0;
				case 4:
					return 3.937007874015748031496062992126;
				default:
					return 1.0;
			}
		}

		/** Assumes all standard input comes in metric units. */
		public static double AutoScaleFactor()
		{
			Document doc = AcApplication.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			int Lunits = db.Lunits;

			switch (Lunits)
			{
				case 2:
				return 1.0;
				case 4:
				return 0.03937007874015748031496062992126;
				default:
				return 1.0;
			}
		}

		public static double GlobalSelectDistance()
		{
			return 1000 * AutoScaleFactor();
		}

		// TODO move to a math helper class
		public static double SanitizeAngle(double inDegrees, Vector3d segmentDirection)
		{
			Editor ed = Session.GetEditor();

			CoordinateSystem3d ucs = ed.CurrentUserCoordinateSystem.CoordinateSystem3d;

			Vector3d textDirection = segmentDirection;

			int i = 0;

			Vector3d testAxis = ucs.Xaxis.RotateBy(Radians(1.0), Vector3d.ZAxis);
			double textAxisDotProduct = textDirection.DotProduct(testAxis);

			while ((textAxisDotProduct < 0.0))
			{
				textDirection = textDirection.RotateBy(Radians(180.0), Vector3d.ZAxis);
				inDegrees += Radians(180.0);
				if (i++ > 10)
				{
					break;
				}
				textAxisDotProduct = textDirection.DotProduct(testAxis);
			}

			return inDegrees;
		}

		// TODO move to a math helper class
		public static double Radians(double inDegrees)
		{
			return inDegrees * Math.PI / 180.0;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void LogDebug(string formattedMessage, params string[] args)
		{
			Log(String.Format(formattedMessage, args));
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void LogDebug(string message)
		{
			Log(message);
		}

		public static void Log(string formattedMessage, params string[] args)
		{
			Log(String.Format(formattedMessage, args));
		}

		public static void Log(string message)
		{
			GetEditor().WriteMessage(Environment.NewLine + message);
		}

		public static void Command(params object[] parameters)
		{
			GetEditor().Command(parameters);
		}

		public static CommandResult AsyncCommand(params object[] parameters)
		{
			return GetEditor().CommandAsync(parameters);
		}

		public static void SetUndoMark()
		{
			//Utils.SetUndoMark();
		}
	}
}
