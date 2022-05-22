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
using AcTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

namespace Ironwill
{
	public static class Session
	{
		public static Document GetDocument()
		{
			return AcApplication.DocumentManager.MdiActiveDocument;
		}

		public static DocumentLock LockDocument()
		{
			return GetDocument().LockDocument();
		}

		public static Transaction StartTransaction()
		{
			return GetDocument().TransactionManager.StartTransaction();
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

		public static BlockTableRecord GetBlockTableRecord(Transaction transaction)
		{
			return transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(GetDatabase()), OpenMode.ForRead) as BlockTableRecord;
		}

		public static void AddNewObject(Transaction transaction, Entity newEntity)
		{
			BlockTableRecord blockTableRecord = GetBlockTableRecord(transaction);
			
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

		public static double GetScaleFactor()
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
		public static void LogDebug(string message)
		{
			GetEditor().WriteMessage(message + "\n");
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void LogDebug(string formattedMessage, params string[] args)
		{
			LogDebug(String.Format(formattedMessage, args));
		}

		public static void Log(string message)
		{
			GetEditor().WriteMessage(message + "\n");
		}

		public static void Log(string formattedMessage, params string[] args)
		{
			Log(String.Format(formattedMessage, args));
		}

		public static void Command(params object[] parameters)
		{
			GetEditor().Command(parameters);
		}
	}
}
