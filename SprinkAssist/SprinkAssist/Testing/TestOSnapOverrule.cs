using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;

[assembly: CommandClass(typeof(ObjectSnapping.Commands))]

namespace ObjectSnapping
{
	public class Commands
	{
		const string regAppName = "TTIF_SNAP";

		private static OSOverrule _osOverrule = null;

		// Object Snap Overrule to prevent snapping to objects
		// with certain XData attached

		public class OSOverrule : OsnapOverrule
		{
			public OSOverrule()
			{
				// Tell AutoCAD to filter on our application name
				// (this should mean our overrule only gets called
				// on objects possessing XData with this name)

				SetXDataFilter(regAppName);
			}

			public override void GetObjectSnapPoints(
			  Entity ent,
			  ObjectSnapModes mode,
			  IntPtr gsm,
			  Point3d pick,
			  Point3d last,
			  Matrix3d view,
			  Point3dCollection snaps,
			  IntegerCollection geomIds
			)
			{
				if (mode == ObjectSnapModes.ModeCenter)
				{
					BlockReference blockReference = ent as BlockReference;

					if (blockReference != null)
					{
						snaps.Add(blockReference.Position);
					}
				}
			}

			public override void GetObjectSnapPoints(
			  Entity ent,
			  ObjectSnapModes mode,
			  IntPtr gsm,
			  Point3d pick,
			  Point3d last,
			  Matrix3d view,
			  Point3dCollection snaps,
			  IntegerCollection geomIds,
			  Matrix3d insertion
			)
			{
				if (mode == ObjectSnapModes.ModeCenter)
				{
					BlockReference blockReference = ent as BlockReference;

					if (blockReference != null)
					{
						snaps.Add(blockReference.Position);
					}
				}
			}

			public override bool IsContentSnappable(Entity entity)
			{
				return false;
			}
		}

		private static void ToggleOverruling(bool on)
		{
			if (on)
			{
				if (_osOverrule == null)
				{
					_osOverrule = new OSOverrule();

					ObjectOverrule.AddOverrule(
					  RXObject.GetClass(typeof(Entity)),
					  _osOverrule,
					  false
					);
				}

				ObjectOverrule.Overruling = true;
			}
			else
			{
				if (_osOverrule != null)
				{
					ObjectOverrule.RemoveOverrule(
					  RXObject.GetClass(typeof(Entity)),
					  _osOverrule
					);

					_osOverrule.Dispose();
					_osOverrule = null;
				}

				// I don't like doing this and so have commented it out:
				// there's too much risk of stomping on other overrules...

				// ObjectOverrule.Overruling = false;
			}
		}

		[CommandMethod("DISNAP")]
		public static void DisableSnapping()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;
			var ed = doc.Editor;

			// Start by getting the entities to disable snapping for.
			// If none selected, turn off the overrule

			var psr = ed.GetSelection();

			if (psr.Status != PromptStatus.OK)
				return;

			ToggleOverruling(true);

			// Start a transaction to modify the entities' XData

			using (var tr = doc.TransactionManager.StartTransaction())
			{
				// Make sure our RegAppID is in the table

				var rat =
				  (RegAppTable)tr.GetObject(
					db.RegAppTableId,
					OpenMode.ForRead
				  );

				if (!rat.Has(regAppName))
				{
					rat.UpgradeOpen();
					var ratr = new RegAppTableRecord();
					ratr.Name = regAppName;
					rat.Add(ratr);
					tr.AddNewlyCreatedDBObject(ratr, true);
				}

				// Create the XData and set it on the object

				using (
				  var rb =
					new ResultBuffer(
					  new TypedValue(
						(int)DxfCode.ExtendedDataRegAppName, regAppName
					  ),
					  new TypedValue(
						(int)DxfCode.ExtendedDataInteger16, 1
					  )
					)
				)
				{
					foreach (SelectedObject so in psr.Value)
					{
						var ent =
						  tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
						if (ent != null)
						{
							ent.XData = rb;
						}
					}
				};

				tr.Commit();
			}
		}

		[CommandMethod("ENSNAP")]
		public static void EnableSnapping()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			var db = doc.Database;
			var ed = doc.Editor;

			// Start by getting the entities to enable snapping for

			var pso = new PromptSelectionOptions();
			pso.MessageForAdding =
			  "Select objects (none to remove overrule)";
			var psr = ed.GetSelection(pso);

			if (psr.Status == PromptStatus.Error)
			{
				ToggleOverruling(false);
				ed.WriteMessage("\nOverruling turned off.");
				return;
			}
			else if (psr.Status != PromptStatus.OK)
				return;

			// Start a transaction to modify the entities' XData

			using (var tr = doc.TransactionManager.StartTransaction())
			{
				// Create a ResultBuffer and use it to remove the XData
				// from the object

				using (
				  var rb =
					new ResultBuffer(
					  new TypedValue(
						(int)DxfCode.ExtendedDataRegAppName, regAppName
					  )
					)
				)
				{
					foreach (SelectedObject so in psr.Value)
					{
						var ent =
						  tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
						if (ent != null)
						{
							ent.XData = rb;
						}
					}
				};

				tr.Commit();
			}
		}
	}
}