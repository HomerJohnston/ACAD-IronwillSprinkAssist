using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Text;

[assembly: CommandClass(typeof(PreventSelection.Test))]

namespace PreventSelection
{
	public class Test : IExtensionApplication
	{
		void OnSelectionAdded(object sender, SelectionAddedEventArgs e)
		{
			ObjectId[] addedIds = e.AddedObjects.GetObjectIds();
			for (int i = 0; i < addedIds.Length; i++)
			{
				ObjectId oid = addedIds[i];

				if (IsInList(oid.ObjectClass.DxfName))
				{
					e.Remove(i);
				}
			}
		}

		[CommandMethod("US")]
		public static void Unselect()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;

			// Print the list of currently unhighlighted classes

			ed.WriteMessage(ListToPrint());

			// Get the type to add to the list

			PromptResult pr =
			  ed.GetString(
				"\nEnter the type of object to stop from " +
				"being selected: "
			  );
			if (pr.Status != PromptStatus.OK)
				return;

			if (IsInList(pr.StringResult))
			{
				ed.WriteMessage("\nItem already in the list.");
			}
			else
			{
				AddToList(pr.StringResult);
				ed.WriteMessage("\nItem added to the list.");
			}
		}

		// Would call this command RS, but it's taken by RSCRIPT,
		// so using the somewhat unwieldy UUS, instead

		[CommandMethod("UUS")]
		public static void Ununselect()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;

			// Print the list of currently unhighlighted classes

			ed.WriteMessage(ListToPrint());

			// Get the type to remove from the list

			PromptResult pr =
			  ed.GetString(
				  "\nEnter the type of object to remove from the " +
				"list: "
			  );
			if (pr.Status != PromptStatus.OK)
				return;

			if (!IsInList(pr.StringResult))
			{
				ed.WriteMessage("\nItem not currently in the list.");
			}
			else
			{
				RemoveFromList(pr.StringResult);
				ed.WriteMessage("\nItem removed from the list.");
			}
		}

		void IExtensionApplication.Initialize()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;

			ed.SelectionAdded +=
			  new SelectionAddedEventHandler(OnSelectionAdded);
		}

		void IExtensionApplication.Terminate()
		{
			Document doc = Application.DocumentManager.MdiActiveDocument;
			Editor ed = doc.Editor;

			ed.SelectionAdded -=
			  new SelectionAddedEventHandler(OnSelectionAdded);
		}

		// The list of types to unhighlight

		static List<string> _unhighlighted = new List<string>();

		// Add a type to the list

		public static void AddToList(string name)
		{
			string upper = name.ToUpper();
			if (!_unhighlighted.Contains(upper))
			{
				_unhighlighted.Add(upper);
			}
		}

		// Remove a type from the list

		public static void RemoveFromList(string name)
		{
			string upper = name.ToUpper();
			if (_unhighlighted.Contains(upper))
			{
				_unhighlighted.Remove(upper);
			}
		}

		// Check whether the list contains a type

		public static bool IsInList(string name)
		{
			return _unhighlighted.Contains(name.ToUpper());
		}

		// Get a string printing the contents of the list

		public static string ListToPrint()
		{
			string toPrint;

			if (_unhighlighted.Count == 0)
			{
				toPrint =
				  "\nThere are currently no objects in the list " +
				  "to stop from being selected.";
			}
			else
			{
				StringBuilder sb =
				  new StringBuilder(
					"\nObjects of these types will not be selected:"
				  );
				foreach (string name in _unhighlighted)
				{
					sb.Append(" " + name);
				}
			toPrint = sb.ToString();
			}

			return toPrint;
		}
	}
}