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
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.DrawPipeCmd))]

namespace Ironwill.Commands
{
	public class DrawPipeCmd
	{
		[CommandDescription("Switches to the draft aid layer and starts the line command.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DrawDraftAid", CommandFlags.Modal)]
		public void DrawDraftAid()
		{
			if (SetLayer(Layer.DraftAid.Get()))
			{
				Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
			}
		}

		[CommandDescription("Switches to the pipe main layer and starts the line command.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DrawPipeMain", CommandFlags.Modal)]
		public void DrawPipeMain()
		{
			if (SetLayer(Layer.SystemPipe_Main.Get()))
			{
				Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
			}
		}

		[CommandDescription("Switches to the pipe branchline layer and starts the line command.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DrawPipeBranchline", CommandFlags.Modal)]
		public void DrawPipeBranchline()
		{
			if (SetLayer(Layer.SystemPipe_Branchline.Get()))
			{
				Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
			}
		}

		[CommandDescription("Switches to the pipe armover layer and starts the line command.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DrawPipeArmover", CommandFlags.Modal)]
		public void DrawPipeArmover()
		{
			if (SetLayer(Layer.SystemPipe_Armover.Get()))
			{
				Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
			}
		}

		[CommandDescription("Switches to the pipe drain layer and starts the line command.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "DrawPipeDrain", CommandFlags.Modal)]
		public void DrawPipeDrain()
		{
			if (SetLayer(Layer.SystemPipe_AuxDrain.Get()))
			{
				Session.GetDocument().SendStringToExecute("_line\n", false, false, true);
			}
		}

		public bool SetLayer(string layerName)
		{
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				LayerTable layerTable;
				layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

				if (layerTable.Has(layerName))
				{
					ObjectId layerObjectId = layerTable[layerName];

					LayerTableRecord layer = transaction.GetObject(layerObjectId, OpenMode.ForRead) as LayerTableRecord;

					if (layer == null)
					{
						return false;
					}

					if (layer.IsFrozen)
					{
						Session.Log("Warning: Layer {0} is frozen, can't switch!", layerName);
						return false;
					}

					if (layer.IsOff)
					{
						Session.Log("Warning: Layer {0} was off, turning layer on.", layerName);
						layer.UpgradeOpen();
						layer.IsOff = false;
						layer.DowngradeOpen();
					}

					database.Clayer = layerTable[layerName];
					transaction.Commit();

					return true;
				}

				Session.Log("Warning: Could not find layer {0}!", layerName);
				return false;
			}
		}
	}
}
