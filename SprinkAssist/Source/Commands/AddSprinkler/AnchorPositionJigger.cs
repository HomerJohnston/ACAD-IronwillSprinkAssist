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
	// Jigger responsible for setting position of the anchor
	internal class AnchorPositionJigger : DrawJig
	{
		public TileAnchor anchor;

		public AnchorPositionJigger(TileAnchor inAnchor)
		{
			anchor = new TileAnchor(inAnchor);
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Select a tile intersection point");
			PromptPointResult pointResult = prompts.AcquirePoint(jigPromptPointOptions);

			if (pointResult.Status != PromptStatus.OK)
			{
				return SamplerStatus.Cancel;
			}

			anchor.anchorPos = pointResult.Value;

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			anchor.Draw(draw);
			return true;
		}
	}
}
