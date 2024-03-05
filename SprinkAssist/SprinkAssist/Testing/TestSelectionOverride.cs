using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Ironwill;
using System.Collections.Generic;
using System.Text;

[assembly: CommandClass(typeof(Ironwill.Commands.DisableObjectSelection))]

namespace Ironwill.Commands
{
	internal class DisableObjectSelection : SprinkAssistCommand
	{
		private static bool active = false;

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
	}
}