﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsSystem;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Ironwill.Commands.ToggleLayerCommands))]

namespace Ironwill.Commands
{
	public class ToggleLayerCommands
	{
		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist_ToggleDraftAid")]
		public void ToggleDraftAidCmd()
		{
			LayerHelper.ToggleFrozen(Layer.DraftAid.Get());
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist_ToggleXref")]
		public void ToggleXrefCmd()
		{
			LayerHelper.ToggleFrozen(Layer.XREF.Get());
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist_ToggleCoverage")]
		public void ToggleCoverageCmd()
		{
			LayerHelper.ToggleFrozen(Layer.HeadCoverage.Get());
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist_TogglePipeLabels")]
		public void TogglePipeLabelsCmd()
		{
			LayerHelper.ToggleFrozen(Layer.PipeLabel.Get());
		}
/*
		[CommandMethod("ToggleLineSmoothing")]
		public void ToggleLineSmoothingCmd()
		{
			Editor editor = Session.GetEditor();

			Autodesk.AutoCAD.GraphicsSystem.Manager gfxManager = Session.GetDocument().GraphicsManager;
			
			using (Autodesk.AutoCAD.GraphicsSystem.Configuration gfxConfig = new Autodesk.AutoCAD.GraphicsSystem.Configuration())
			{
				if (!gfxConfig.IsHardwareAccelerationAvailable())
				{
					AcApplication.ShowAlertDialog("Hardware acceleration not available");
					return;
				}

				if (!gfxConfig.IsHardwareAccelerationEnabled())
				{
					gfxConfig.setHardwareAcceleration(true);
				}

				UniqueString lineSmoothing = UniqueString.Intern("ACAD_LineSmoothing");

				if (gfxConfig.IsFeatureAvailable(lineSmoothing))
				{
					bool status = gfxConfig.IsFeatureEnabled(lineSmoothing);

					if (status)
					{
						Session.GetDocument().SendStringToExecute("LINESMOOTHING\n0\n", false, false, false);
					}
					else
					{
						Session.GetDocument().SendStringToExecute("LINESMOOTHING\n1\n", false, false, false);
					}

					// Can't seem to get this to work.
					// gfxConfig.SetFeatureEnabled(lineSmoothing, !status);
				}
			}
		}*/

		[CommandMethod("SpkAssist_ToggleLineweightDisplay")]
		public void ToggleLineweightDisplayCmd()
		{
			bool lwdisplay = System.Convert.ToBoolean(AcApplication.GetSystemVariable("LWDISPLAY"));

			AcApplication.SetSystemVariable("LWDISPLAY", System.Convert.ToInt32(!lwdisplay));
		}
	}
}
