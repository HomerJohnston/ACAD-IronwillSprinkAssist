﻿using System;
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

[assembly: CommandClass(typeof(Ironwill.CountHeads))]

namespace Ironwill
{
	public class CountHeads
	{
		[CommandMethod("CountHeads")]
		public void CountHeadsCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				PromptSelectionOptions promptSelectionOptions = new PromptSelectionOptions();
				promptSelectionOptions.MessageForAdding = "Select heads";

				TypedValue[] filterValues =
				{
					new TypedValue((int)DxfCode.Operator, "<or"),
					new TypedValue((int)DxfCode.LayerName, Layers.SystemHead.Get()),
					new TypedValue((int)DxfCode.Operator, "or>"),
				};

				SelectionFilter selectionFilter = new SelectionFilter(filterValues);

				PromptSelectionResult promptSelectionResult = Session.GetEditor().GetSelection(promptSelectionOptions, selectionFilter);

				if (promptSelectionResult.Status != PromptStatus.OK)
				{
					Session.WriteMessage("Invalid selection, aborting");
					transaction.Commit();
					return;
				}

				Dictionary<string, int> sprinklerCounts = new Dictionary<string, int>();

				SelectionSet selectionSet = promptSelectionResult.Value;

				foreach (ObjectId objectId in selectionSet.GetObjectIds())
				{
					BlockReference head = transaction.GetObject(objectId, OpenMode.ForRead) as BlockReference;

					if (head == null)
					{
						continue;
					}

					BlockTableRecord originalHeadBlockTableRecord = head.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord;

					string name = originalHeadBlockTableRecord.Name;

					if (!sprinklerCounts.ContainsKey(name))
					{
						sprinklerCounts.Add(name, 0);
					}

					sprinklerCounts[name]++;
				}

				Session.WriteMessage("\n");

				sprinklerCounts.OrderBy(key => key.Key);

				foreach (var x in sprinklerCounts.OrderBy(key => key.Key))
				{
					string s = x.Key + ": ";
					
					while (s.Length < 21) { s += " "; }

					s += x.Value.ToString();

					Session.WriteMessage(s);
				}

				transaction.Commit();
			}
		}
	}
}
