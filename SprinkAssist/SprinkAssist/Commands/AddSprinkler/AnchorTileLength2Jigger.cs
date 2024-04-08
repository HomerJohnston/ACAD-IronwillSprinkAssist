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
	// Jigger responsible for setting tile length 2 (only!) of the anchor
	internal class AnchorTileLength2Jigger : DrawJig
	{
		public TileAnchor anchor;

		public AnchorTileLength2Jigger(TileAnchor inAnchor)
		{
			anchor = new TileAnchor(inAnchor);
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Specify length of tile in perpendicular direction");
			PromptPointResult pointResult = prompts.AcquirePoint(jigPromptPointOptions);

			if (pointResult.Status != PromptStatus.OK)
			{
				return SamplerStatus.Cancel;
			}

			Vector3d vector2 = Vector3d.ZAxis.CrossProduct(anchor.tileVector);

			Vector3d cursorVec = pointResult.Value - anchor.anchorPos;

			double projected = cursorVec.DotProduct(vector2);

			anchor.tileLength2 = projected;

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			anchor.Draw(draw);
			return true;
		}
	}
}
