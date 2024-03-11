using System;
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
using System.Data.Common;
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.ToggleLayer.ToggleLayerCmd))]

namespace Ironwill.Commands.ToggleLayer
{
	internal class ToggleLayerCmd : SprinkAssistCommand
	{
		CommandSetting<bool> coverageShowFill;

        public ToggleLayerCmd()
        {
            coverageShowFill = new CommandSetting<bool>("CoverageShowFill", true, cmdSettings);
        }

		[CommandDescription("Toggles the solid fill of the sprinkler head coverage layer on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleHeadCoverageFill", CommandFlags.NoBlockEditor)]
		public void ToggleHeadCoverageFillCmd()
        {
            using (Transaction transaction = Session.StartTransaction())
            {
                coverageShowFill.Set(transaction, !(coverageShowFill.Get(transaction)));

				ELayerStatus coverageLayerStatus = ELayerStatus.None;
                
                if (LayerHelper.GetLayerState(transaction, Layer.HeadCoverage, ref coverageLayerStatus))
				{
					bool coverageShowFillValue = coverageShowFill.Get(transaction);
					bool coverageLayerFrozen = coverageLayerStatus.HasFlag(ELayerStatus.Frozen);
					bool freezeFillLayer = coverageLayerFrozen || !coverageShowFillValue;

					LayerHelper.SetFrozen(transaction, freezeFillLayer, Layer.HeadCoverage_Fill);
                }
                
                transaction.Commit();
			}
		}
		
		[CommandDescription("Adjusts the transparency of the head coverage layers.", "Enter a value from 0 to 100.", "You can adjust transparency in layer settings but AutoCAD limits that to 90%. With this you can specify up to 100%.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "SetHeadCoverageTransparency", CommandFlags.NoBlockEditor)]
		public void SetHeadCoverageTransparency()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				float boxTransparency = LayerHelper.GetTransparency(transaction, Layer.HeadCoverage);
				float fillTransparency = LayerHelper.GetTransparency(transaction, Layer.HeadCoverage_Fill);

				PromptIntegerOptions promptIntegerOptions = new PromptIntegerOptions("Enter new fill transparency");
				promptIntegerOptions.LowerLimit = 0;
				promptIntegerOptions.UpperLimit = 100;

				PromptIntegerResult promptIntegerResult = Session.GetEditor().GetInteger(promptIntegerOptions);

				switch (promptIntegerResult.Status)
				{
					case PromptStatus.OK:
					{
						double newFillTransparency = (promptIntegerResult.Value) * 0.01;
						double newBoxTransparency = Math.Pow((promptIntegerResult.Value) * 0.01 * 0.75, 2.0);

						LayerHelper.SetTransparency(transaction, newBoxTransparency, Layer.HeadCoverage);
						LayerHelper.SetTransparency(transaction, newFillTransparency, Layer.HeadCoverage_Fill);

						break;
					}
					default:
					{
						return;
					}
				}

				transaction.Commit();
			}

			Session.GetEditor().Regen();
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandDescription("Turns the draft aid layer on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleDraftAidFrozen", CommandFlags.NoBlockEditor)]
		public void ToggleDraftAidFrozenCmd()
		{
            using (Transaction transaction = Session.StartTransaction())
            {
                LayerHelper.ToggleFrozen(transaction, Layer.DraftAid);
                transaction.Commit();
            }
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandDescription("Turns the Xref layer on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleXrefFrozen", CommandFlags.NoBlockEditor)]
		public void ToggleXrefFrozenCmd()
		{
            using (Transaction transaction = Session.StartTransaction())
            {
                LayerHelper.ToggleFrozen(transaction, Layer.XREF);
                transaction.Commit();
			}
		}


		/// <summary>
		/// 
		/// </summary>		
		[CommandDescription("Turns the head coverage layer on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleCoverage", CommandFlags.NoBlockEditor)]
		public void ToggleCoverageCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
            {
                LayerHelper.ToggleFrozen(transaction, Layer.HeadCoverage);

                ELayerStatus coverageLayerStatus = ELayerStatus.None;
                
				if (LayerHelper.GetLayerState(transaction, Layer.HeadCoverage, ref coverageLayerStatus))
				{
					bool coverageShowFillValue = coverageShowFill.Get(transaction);
					bool coverageLayerFrozen = coverageLayerStatus.HasFlag(ELayerStatus.Frozen);
					bool freezeFillLayer = coverageLayerFrozen || !coverageShowFillValue;

					LayerHelper.SetFrozen(transaction, freezeFillLayer, Layer.HeadCoverage_Fill);
				}
                
				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandDescription("Turns the pipe labels layer on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "TogglePipeLabels", CommandFlags.NoBlockEditor)]
		public void TogglePipeLabelsCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerHelper.ToggleFrozen(transaction, Layer.PipeLabel);
				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandDescription("Turns AutoCAD lineweight display on or off.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleLineweightDisplay", CommandFlags.NoBlockEditor)]
		public void ToggleLineweightDisplayCmd()
		{
			bool lwdisplay = Convert.ToBoolean(AcApplication.GetSystemVariable("LWDISPLAY"));

			AcApplication.SetSystemVariable("LWDISPLAY", Convert.ToInt32(!lwdisplay));
		}
	}
}
