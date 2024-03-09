using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Ironwill;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

[assembly: CommandClass(typeof(Ironwill.Commands.DisableObjectSelection))]

namespace Ironwill.Commands
{
	internal class DisableObjectSelection : SprinkAssistCommand
	{
		private static bool active = false;

		private static Point3d cursorPos;

		private static bool selectionInProgress = false;

		private static bool isQuiescent = true;

		private static int lastLeavingQuiescentTime = 0;

		private static bool manualSelectInProgress = false;

		public static void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
		{
			ObjectId[] addedIds = e.AddedObjects.GetObjectIds();

			bool removed = false;

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
						removed = true;
					}

					tr.Commit();
				}
			}

			int currentTime = System.Convert.ToInt32(Application.GetSystemVariable("MILLISECS"));

			bool wasJustQuiescent = currentTime - lastLeavingQuiescentTime < 100;

			if (e.Selection.Count == 0 && removed && wasJustQuiescent)
			{
				Session.Log("Attempting to start manual selection box");

				if (selectionInProgress)
				{
					Session.Log("Another selection is in progress, aborting");
					return;
				}

				using (DocumentLock documentLock = Session.LockDocument())
				{
					using (Transaction transaction = Session.StartTransaction())
					{
						Session.Log("Starting manual box!");
						
						SelectionSet currentSelection = Session.GetEditor().SelectImplied().Value;
						
						Session.GetDocument().SendStringToExecute($"SELECT b {cursorPos.X},{cursorPos.Y} ", true, false, true);
						
						manualSelectInProgress = true;
						
						transaction.Commit();
					}
				}
			}


			// TODO can I get the current selection here and, if nothing was selected and the background was filtered out, start a new selection window from the click point instead? This might not work nicely if the previous selection attempt was a window though.
		}

		[CommandMethod("SpkAssist", "ToggleXrefSelectable", CommandFlags.NoBlockEditor)]
		public static void ToggleXrefSelectable()
		{
			active = !active;

			if (active)
			{
				Session.Log($"Xref layer selection prevention: enabled");
				Session.GetEditor().SelectionAdded += new SelectionAddedEventHandler(OnSelectionAdded);
			}
			else
			{
				Session.Log($"Xref layer selection prevention: disabled");
				Session.GetEditor().SelectionAdded -= new SelectionAddedEventHandler(OnSelectionAdded);
			}
		}

		[CommandMethod("CC")]
		public void CursorCoords()
		{
			var doc = Application.DocumentManager.MdiActiveDocument;
			if (doc == null) return;
			var ed = doc.Editor;

			ed.PromptingForSelection += (s, e) => { Session.Log("Editor.PromptingForSelection"); selectionInProgress = true; };
			ed.PromptForSelectionEnding += (s, e) => { Session.Log("Editor.PromptedForSelectionEnding"); selectionInProgress = false; };
			ed.PromptedForSelection += (s, e) => { Session.Log("Editor.PromptedForSelection"); };
			ed.SelectionAdded += (s, e) => 
			{
				Session.Log("Editor.SelectionAdded");
				if (manualSelectInProgress)
				{
					Session.Log("EXITING MANUAL SELECT");
					Session.GetDocument().SendStringToExecute($" ", true, false, true);
					manualSelectInProgress = false;
				}
			};
			ed.SelectionRemoved += (s, e) => { Session.Log("Editor.SelectionRemoved"); };
			ed.LeavingQuiescentState += (s, e) => 
			{
				Session.Log("Editor.LeavingQuiescentState"); isQuiescent = false;
				lastLeavingQuiescentTime = System.Convert.ToInt32(Application.GetSystemVariable("MILLISECS"));
			};
			ed.EnteringQuiescentState += (s, e) => { Session.Log("Editor.EnteringQuiescentState"); isQuiescent = true; };

			

			Session.GetDocument().ImpliedSelectionChanged += (s, e) => { Session.Log("Document.ImpliedSelectionChange"); };
			
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
	}
}