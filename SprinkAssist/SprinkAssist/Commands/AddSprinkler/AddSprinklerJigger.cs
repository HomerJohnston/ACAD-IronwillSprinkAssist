using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands.AddSprinkler
{// -----------------------------------------------------------------------------------------------
 // 
	internal class AddSprinklerJigger : DrawJig
	{
		// Settings -----------------------------

		// State --------------------------------------
		/** Current transaction, set by the owning command when it spawns this jig */
		Transaction transaction;

		/** Current mouse cursor position, updated by the sampler as it runs */
		protected Point3d cursorPosition;

		/** New sprinkler to be placed */
		BlockReference jigSprinkler = null;

		TileAnchor tileAnchor;

		public AddSprinklerJigger(Transaction inTransaction, BlockReference templateSprinkler, TileAnchor inAnchor)
		{
			transaction = inTransaction;

			tileAnchor = inAnchor;

			if (jigSprinkler == null)
			{
				jigSprinkler = BlockOps.InsertBlock(transaction, BlockOps.GetDynamicBlockName(templateSprinkler));

				BlockOps.CopyCommonProperties(templateSprinkler, jigSprinkler);
				BlockOps.CopyDynamicBlockProperties(templateSprinkler, jigSprinkler);
			}
		}

		// The sampler will be called whenever there is a mouse input of any sort
		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			JigPromptPointOptions jigPromptPointOptions = new JigPromptPointOptions(Environment.NewLine + "Click to place...");
			jigPromptPointOptions.Keywords.Add("poSition");
			jigPromptPointOptions.Keywords.Add("Reorient");
			jigPromptPointOptions.Keywords.Add("New");
			jigPromptPointOptions.Keywords.Add("Head");
			PromptPointResult promptPointResult = prompts.AcquirePoint(jigPromptPointOptions);

			switch (promptPointResult.Status)
			{
				case PromptStatus.Keyword:
				{
					if (promptPointResult.StringResult == "poSition")
					{
						tileAnchor.MarkPositionDirty();
					}
					else if (promptPointResult.StringResult == "Reorient")
					{
						tileAnchor.MarkPositionDirty();
						tileAnchor.MarkRotationDirty();
					}
					else if (promptPointResult.StringResult == "New")
					{
						tileAnchor.MarkAllDirty();
					}
					else if (promptPointResult.StringResult == "Head")
					{
					}
					return SamplerStatus.NoChange;
				}
				case PromptStatus.None:
				{
					return SamplerStatus.NoChange;
				}
				case PromptStatus.Cancel:
				{
					return SamplerStatus.Cancel;
				}
				case PromptStatus.Error:
				{
					return SamplerStatus.Cancel;
				}
			}

			cursorPosition = promptPointResult.Value;

			return SamplerStatus.OK;
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			jigSprinkler.Position = NearestSnap(cursorPosition);

			tileAnchor.Draw(draw);
			jigSprinkler.Draw(draw);

			DrawNearestRadius(draw);
			DrawMaximumRadius(draw);

			return true;
		}

		protected void DrawNearestRadius(WorldDraw draw)
		{
			Circle minimumRadius = new Circle(jigSprinkler.Position, Vector3d.ZAxis, 1828.8 * Session.AutoScaleFactor());
			minimumRadius.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.DarkGrey);

			minimumRadius.Draw(draw);
		}

		protected void DrawMaximumRadius(WorldDraw draw)
		{
			Circle maximumRadius = new Circle(jigSprinkler.Position, Vector3d.ZAxis, 2286 * Session.AutoScaleFactor());
			maximumRadius.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.LightGreen);

			maximumRadius.Draw(draw);
		}

		Point3d NearestSnap(Point3d cursorPos)
		{
			Point3d anchor = tileAnchor.anchorPos;

			int numLongInterval = 2;
			int numShortInterval = 2;

			Vector3d longAxis = tileAnchor.tileVector;
			Vector3d shortAxis = Vector3d.ZAxis.CrossProduct(longAxis);

			double longAxisLength = tileAnchor.tileLength1;
			double shortAxisLength = tileAnchor.tileLength2;

			if (Math.Abs(shortAxisLength) > Math.Abs(longAxisLength))
			{
				Vector3d temp = shortAxis;
				shortAxis = longAxis;
				longAxis = temp;

				double temp2 = shortAxisLength;
				shortAxisLength = longAxisLength;
				longAxisLength = temp2;
			}

			if (Math.Abs(longAxisLength) > Math.Abs(shortAxisLength * 1.25))
			{
				numLongInterval = 4;
			}

			double longInterval = longAxisLength / numLongInterval;
			double shortInterval = shortAxisLength / numShortInterval;

			Vector3d cursorPosLoc = cursorPos - anchor;

			double longDist = cursorPosLoc.DotProduct(longAxis);
			double shortDist = cursorPosLoc.DotProduct(shortAxis);

			double longDistRounded = Math.Round(longDist / longInterval) * longInterval;
			double shortDistRounded = Math.Round(shortDist / shortInterval) * shortInterval;

			Point3d roundedPoint = anchor + longDistRounded * longAxis + shortDistRounded * shortAxis;

			return roundedPoint;
		}

		public void RemoveSprinkler()
		{
			jigSprinkler.Erase();
		}
	}
}
