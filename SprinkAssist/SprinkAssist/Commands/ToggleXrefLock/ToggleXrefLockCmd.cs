using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Ironwill;
using Ironwill.Commands.Help;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

[assembly: CommandClass(typeof(Ironwill.Commands.ToggleXrefLock.ToggleXrefLockCmd))]

namespace Ironwill.Commands.ToggleXrefLock
{
	internal class ToggleXrefLockCmd : SprinkAssistCommand
	{
		/*
		private static bool active = false;

		private static Point3d cursorPos;

		private static bool selectionInProgress = false;

		private static int lastLeavingQuiescentTime = 0;

		private static bool manualSelectInProgress = false;
		*/

		public static void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
		{
			ObjectId[] addedIds = e.AddedObjects.GetObjectIds();

			for (int i = 0; i < addedIds.Length; i++)
			{
				ObjectId oid = addedIds[i];

				if (oid == null)
				{
					continue;
				}

				using (Transaction tr = Session.StartTransaction())
				{
					DBObject dbObject = tr.GetObject(oid, OpenMode.ForRead);

					Entity entity = dbObject as Entity;

					if (entity != null && entity.Layer == Layer.XREF)
					{
						e.Remove(i);
					}

					tr.Commit();
				}
			}
		}

		[CommandDescription("Locks the XREF layer and makes it non-selectable", "The XREF layer can still be clicked on; it will simply be instantly deselected any time it is selected.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleXrefLock", CommandFlags.NoBlockEditor)]
		public static void Main()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				ELayerStatus layerStatus = ELayerStatus.None;
				LayerHelper.GetLayerState(transaction, Layer.XREF, ref layerStatus);

				if (layerStatus.HasFlag(ELayerStatus.Locked))
				{
					Session.Log($"Xref layer: UNLOCKED");

                    LayerHelper.SetLocked(transaction, false, Layer.XREF);
					
					Session.GetEditor().SelectionAdded -= new SelectionAddedEventHandler(OnSelectionAdded);
				}
				else
				{
					Session.Log($"Xref layer: LOCKED");

                    LayerHelper.SetLocked(transaction, true, Layer.XREF);

					Session.GetEditor().SelectionAdded += new SelectionAddedEventHandler(OnSelectionAdded);
				}

				transaction.Commit();
			}
		}

		public static void Initialize()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				ELayerStatus layerStatus = ELayerStatus.None;
				LayerHelper.GetLayerState(transaction, Layer.XREF, ref layerStatus);

				if (layerStatus.HasFlag(ELayerStatus.Locked))
				{
					Session.GetEditor().SelectionAdded += new SelectionAddedEventHandler(OnSelectionAdded);
					transaction.Commit();
				}
			}
		}

		// TODO: build a selection system that works the same as the normal click-click selection system and use it when you click on the xref
		// I can't use the normal Select command because it acts different!
		/*
		[CommandMethod("CC")]
		public void CursorCoords()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) return;
			var ed = doc.Editor;

			ed.PromptingForSelection += (s, e) => { selectionInProgress = true; };
			
			ed.PromptForSelectionEnding += (s, e) => { selectionInProgress = false; };
			
			ed.SelectionAdded += (s, e) => 
			{
				if (manualSelectInProgress)
				{
					Session.GetDocument().SendStringToExecute($" ", true, false, true);
					manualSelectInProgress = false;
				}
			};
			
			ed.LeavingQuiescentState += (s, e) => 
			{
				lastLeavingQuiescentTime = Convert.ToInt32(Application.GetSystemVariable("MILLISECS"));
			};

			ed.PointMonitor += (s, e) =>
			{
				var ed2 = (Editor)s;
				if (ed2 == null) return;

				// If the call is just to set the last point, ignore

				if (e.Context.History == PointHistoryBits.LastPoint)
					return;

				// Get the inverse of the current UCS matrix, to display in UCS

				var ucs = ed2.CurrentUserCoordinateSystem.Inverse();

				// Checked whether the point was snapped to

				var snapped = (e.Context.History & PointHistoryBits.ObjectSnapped) > 0;

				// Transform the snapped or computed point to the current UCS

				var pt =
				  (snapped ?
					e.Context.ObjectSnappedPoint :
					e.Context.ComputedPoint).TransformBy(ucs);

				// Display the point with each ordinate at 4 decimal places

				try
				{
					short ii = (short)Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CMDACTIVE");
					//Session.Log(ii.ToString());
					//ed2.WriteMessage("{0}: {1:F4}\n", snapped ? "Snapped" : "Found", pt);

					cursorPos = pt;
				}
				catch (Autodesk.AutoCAD.Runtime.Exception ex)
				{
					if (ex.ErrorStatus != ErrorStatus.NotApplicable)
						throw;
				}
			};
		}
		*/
	}
}