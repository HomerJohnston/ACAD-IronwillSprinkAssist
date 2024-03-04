using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands.AddSprinkler
{
	// -----------------------------------------------------------------------------------------------
	// Jigger responsible for setting tile length 1 (only!) of the anchor
	internal class AnchorTileVectorJigger : DrawJig
	{
		public TileAnchor anchor;

		public AnchorTileVectorJigger(TileAnchor inAnchor)
		{
			anchor = new TileAnchor(inAnchor);
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Select an adjacent tile intersection");
			PromptPointResult pointResult = prompts.AcquirePoint(jigPromptPointOptions);

			if (pointResult.Status != PromptStatus.OK)
			{
				return SamplerStatus.Cancel;
			}

			anchor.tileVector = anchor.anchorPos.GetVectorTo(pointResult.Value).GetNormal();
			anchor.tileLength1 = pointResult.Value.DistanceTo(anchor.anchorPos);

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			anchor.Draw(draw);
			return true;
		}
	}
}
