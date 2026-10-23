using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands.AddSprinkler
{
	// -----------------------------------------------------------------------------------------------
	// Jigger responsible for setting rotation (only!) of the anchor
	internal class AnchorRotationJigger : DrawJig
	{
		public TileAnchor anchor;

		public AnchorRotationJigger(TileAnchor inAnchor)
		{
			anchor = new TileAnchor(inAnchor);
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Select new tile anchor rotation");
			PromptPointResult pointResult = prompts.AcquirePoint(jigPromptPointOptions);

			if (pointResult.Status != PromptStatus.OK)
			{
				return SamplerStatus.Cancel;
			}

			Vector3d tileVectorNew = pointResult.Value - anchor.anchorPos;

			anchor.tileVector = tileVectorNew.GetNormal();

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			anchor.Draw(draw);
			return true;
		}
	}

}
