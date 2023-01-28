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

[assembly: CommandClass(typeof(Ironwill.Commands.ToggleLayerCommands))]

namespace Ironwill.Commands
{
	public class ToggleLayerCommands
	{
		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist", "ToggleDraftAid", CommandFlags.NoBlockEditor)]
		public void ToggleDraftAidCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerHelper.ToggleFrozen(transaction, Layer.DraftAid.Get());
				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist", "ToggleXref", CommandFlags.NoBlockEditor)]
		public void ToggleXrefCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerHelper.ToggleFrozen(transaction, Layer.XREF.Get());
				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist", "ToggleCoverage", CommandFlags.NoBlockEditor)]
		public void ToggleCoverageCmd()
        {
			LayerStatus headCoverageLayerStatus = new LayerStatus();
			LayerStatus headCoverageFillLayerStatus = new LayerStatus();

			using (Transaction transaction = Session.StartTransaction())
			{
				if (!LayerHelper.GetLayerState(transaction, Layer.HeadCoverage.Get(), ref headCoverageLayerStatus))
				{
					Session.Log("Failed to access layer " + Layer.HeadCoverage.Get());
					return;
				}

				if (!LayerHelper.GetLayerState(transaction, Layer.HeadCoverage_Fill.Get(), ref headCoverageFillLayerStatus))
				{
					Session.Log("Failed to access layer " + Layer.HeadCoverage_Fill.Get());
					return;
				}

				if (!headCoverageLayerStatus.HasFlag(LayerStatus.Frozen) && !headCoverageFillLayerStatus.HasFlag(LayerStatus.Frozen))
				{
					LayerHelper.ToggleFrozen(transaction, Layer.HeadCoverage_Fill.Get());
				}
				else if (!headCoverageLayerStatus.HasFlag(LayerStatus.Frozen) && headCoverageFillLayerStatus.HasFlag(LayerStatus.Frozen))
				{
					LayerHelper.ToggleFrozen(transaction, Layer.HeadCoverage.Get());
				}
				else
				{
					LayerHelper.ToggleFrozen(transaction, Layer.HeadCoverage.Get());
					LayerHelper.ToggleFrozen(transaction, Layer.HeadCoverage_Fill.Get());
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist", "TogglePipeLabels", CommandFlags.NoBlockEditor)]
		public void TogglePipeLabelsCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerHelper.ToggleFrozen(transaction, Layer.PipeLabel.Get());
				transaction.Commit();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		[CommandMethod("SpkAssist", "ToggleLineweightDisplay", CommandFlags.NoBlockEditor)]
		public void ToggleLineweightDisplayCmd()
		{
			bool lwdisplay = System.Convert.ToBoolean(AcApplication.GetSystemVariable("LWDISPLAY"));

			AcApplication.SetSystemVariable("LWDISPLAY", System.Convert.ToInt32(!lwdisplay));
		}
	}
}
