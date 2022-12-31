using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.Colors;

[assembly: CommandClass(typeof(Ironwill.Commands.AddSprinkler))]

namespace Ironwill.Commands
{
	// --------------------------------------------------------------------------------------------
	// Class responsible for representing the actual tile grid anchor 
	internal class TileAnchor
	{
		// Settings -----------------------------
		const double anchorRadius = 100;
		const double anchorLegRadius = 50;

		Color anchorColor = Color.FromColorIndex(ColorMethod.ByAci, Colors.Yellow);
		Color anchorLeg1Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.Red);
		Color anchorLeg2Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.Green);

		// State --------------------------------
		/** Grid intersection point */
		Point3d _anchorPos;
		public Point3d anchorPos
		{
			get	{ return _anchorPos; }
			set { _anchorPos = value; _anchorPosSet = true; }
		}

		bool _anchorPosSet = false;
		public bool anchorPosSet
		{
			get { return _anchorPosSet; }
			protected set { _anchorPosSet = value; }
		}

		/** Vector parallel to tile grid */
		Vector3d _tileVector;
		public Vector3d tileVector
		{
			get { return _tileVector; }
			set { _tileVector = value; _tileVectorSet = true; }
		}

		bool _tileVectorSet = false;
		public bool tileVectorSet
		{
			get { return _tileVectorSet; }
			protected set { _tileVectorSet = value; }
		}

		/** Length of the tile in the direction of the tile vector */
		double _tileLength1;
		public double tileLength1
		{
			get { return _tileLength1; }
			set { _tileLength1 = value; _tileLength1Set = true; }
		}

		bool _tileLength1Set = false;
		public bool tileLength1Set
		{
			get { return _tileLength1Set; }
			protected set { _tileLength1Set = value; }
		}

		/** Length of the tile perpendicular to the tile vector */
		double _tileLength2;
		public double tileLength2
		{
			get { return _tileLength2; }
			set { _tileLength2 = value; _tileLength2Set = true; }
		}

		bool _tileLength2Set = false;
		public bool tileLength2Set
		{
			get { return _tileLength2Set; }
			protected set { _tileLength2Set = value; }
		}

		// Constructor --------------------------------
		public TileAnchor()
		{
			_tileVector = new Vector3d(1, 0, 0);
			_tileLength1 = 1220 * Session.AutoScaleFactor();
			_tileLength2 = 610 * Session.AutoScaleFactor();
		}

		public TileAnchor(TileAnchor refAnchor)
		{
			anchorPos = refAnchor.anchorPos;
			anchorPosSet = refAnchor.anchorPosSet;

			tileVector = refAnchor.tileVector;
			tileVectorSet = refAnchor.tileVectorSet;

			tileLength1 = refAnchor.tileLength1;
			tileLength1Set = refAnchor.tileLength1Set;

			tileLength2 = refAnchor.tileLength2;
			tileLength2Set = refAnchor.tileLength2Set;
		}

		// API -------------------------------------------
		public void Draw(WorldDraw draw)
		{
			DrawAnchor(draw, anchorPos, tileVector, tileLength1, tileLength2);
		}

		public void DrawAnchor(WorldDraw draw, Point3d inAnchorPos, Vector3d inTileVector, double inTileLength1, double inTileLength2)
		{
			Circle anchorCircle = new Circle(inAnchorPos, Vector3d.ZAxis, anchorRadius * Session.AutoScaleFactor());
			anchorCircle.Color = anchorColor;


			Line anchorLeg1 = new Line(inAnchorPos, inAnchorPos + inTileLength1 * inTileVector);
			anchorLeg1.Color = anchorLeg1Color;

			Vector3d vector2 = Vector3d.ZAxis.CrossProduct(inTileVector);

			Line anchorLeg2 = new Line(inAnchorPos, inAnchorPos + inTileLength2 * vector2);
			anchorLeg2.Color = anchorLeg2Color;

			Circle anchorLeg1Circle = new Circle(inAnchorPos + inTileLength1 * inTileVector, Vector3d.ZAxis, anchorLegRadius * Session.AutoScaleFactor());
			anchorLeg1Circle.Color = anchorLeg1Color;

			Circle anchorLeg2Circle = new Circle(inAnchorPos + inTileLength2 * vector2, Vector3d.ZAxis, anchorLegRadius * Session.AutoScaleFactor());
			anchorLeg2Circle.Color = anchorLeg2Color;

			draw.Geometry.Draw(anchorCircle);
			draw.Geometry.Draw(anchorLeg1Circle);
			draw.Geometry.Draw(anchorLeg2Circle);
			draw.Geometry.Draw(anchorLeg1);
			draw.Geometry.Draw(anchorLeg2);
		}

		public void MarkPositionDirty()
		{
			anchorPosSet = false;
		}

		public void MarkRotationDirty()
		{
			tileVectorSet = false;
		}

		public void MarkAllDirty()
		{
			anchorPosSet = false;
			tileVectorSet = false;
			tileLength1Set = false;
			tileLength2Set = false;
		}
	}

	// --------------------------------------------------------------------------------------------
	// Actual command
	internal class AddSprinkler : SprinkAssistCommand
	{
		// Settings -----------------------------------

		// State --------------------------------------
		TileAnchor anchor = new TileAnchor();

		ObjectId templateSprinklerId = ObjectId.Null;
		BlockReference templateSprinkler = null;

		public AddSprinkler()
		{
		}

		[CommandMethod("SpkAssist", "AddSprinkler", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
		public void AddSprinkler_Cmd()
		{
			PromptResult promptResult = null;

			while (promptResult == null || promptResult.Status == PromptStatus.OK || promptResult.Status == PromptStatus.Keyword)
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					if (!ValidateTemplateSprinkler(transaction))
					{
						Session.Log("Did not select a sprinkler to place");
						transaction.Commit();
						return;
					}

					if (!ValidateAnchor(anchor, transaction))
					{
						Session.Log("Did not specify valid tile");
						transaction.Commit();
						return;
					}

					AddSprinklerJigger jigger = new AddSprinklerJigger(transaction, templateSprinkler, anchor);

					promptResult = Session.GetEditor().Drag(jigger);

					if (promptResult.Status == PromptStatus.Keyword)
					{
						if (promptResult.StringResult == "Head")
						{
							templateSprinkler = null;
							templateSprinklerId = ObjectId.Null;
							ValidateTemplateSprinkler(transaction);
						}

						jigger.RemoveSprinkler();

						transaction.Commit();
					}

					if (promptResult.Status == PromptStatus.OK)
					{
						transaction.Commit();
					}

					//Session.Log("");
				}
			}
		}

		bool ValidateTemplateSprinkler(Transaction transaction)
		{
			if (templateSprinklerId.IsNull)
			{
				SelectTemplateSprinkler(transaction);
			}
			else if (templateSprinklerId.IsErased || templateSprinklerId.IsEffectivelyErased)
			{
				Session.Log("Template sprinkler was erased - pick new template sprinkler");
				SelectTemplateSprinkler(transaction);
			}
			else if (templateSprinklerId.IsValid && templateSprinklerId.IsWellBehaved)
			{
				templateSprinkler = transaction.GetObject(templateSprinklerId, OpenMode.ForRead) as BlockReference;
				return true;
			}

			return templateSprinkler != null;
		}

		bool ValidateAnchor(TileAnchor anchor, Transaction transaction)
		{
			if (!EnsureValidAnchorPosition(transaction))
			{
				return false;
			}

			if (!EnsureValidAnchorTileLengths())
			{
				return false;
			}

			if (!EnsureValidAnchorRotation())
			{
				return false;
			}

			return true;
		}

		bool EnsureValidAnchorPosition(Transaction transaction)
		{
			if (anchor.anchorPosSet)
			{
				return true;
			}

			SelectAnchorPosition(anchor);

			return anchor.anchorPosSet;
		}

		bool EnsureValidAnchorRotation()
		{
			if (anchor.tileVectorSet)
			{
				return true;
			}

			SelectAnchorRotation(anchor);

			return anchor.tileVectorSet;
		}

		bool EnsureValidAnchorTileLengths()
		{
			if (anchor.tileLength1Set)
			{
				return true;
			}

			SelectAnchorTileVectorLengths(anchor);

			return anchor.tileLength1Set && anchor.tileLength2Set;
		}

		void SelectTemplateSprinkler(Transaction transaction)
		{
			templateSprinkler = BlockOps.PickSprinkler(transaction, "Pick a template sprinkler block");

			if (templateSprinkler == null)
			{
				return;
			}

			templateSprinklerId = templateSprinkler.ObjectId;
		}

		void SelectAnchorPosition(TileAnchor inAnchor)
		{
			AnchorPositionJigger anchorJigger = new AnchorPositionJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.anchorPos = anchorJigger.anchor.anchorPos;

			Session.Log("");
		}

		void SelectAnchorRotation(TileAnchor inAnchor)
		{
			AnchorRotationJigger anchorJigger = new AnchorRotationJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger.anchor.tileVector;

			Session.Log("");
		}

		void SelectAnchorTileVectorLengths(TileAnchor inAnchor)
		{
			AnchorTileVectorJigger anchorJigger1 = new AnchorTileVectorJigger(inAnchor);

			PromptResult result1 = Session.GetEditor().Drag(anchorJigger1);

			if (result1.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger1.anchor.tileVector;
			anchor.tileLength1 = anchorJigger1.anchor.tileLength1;
			Session.Log("");

			AnchorTileLength2Jigger anchorJigger2 = new AnchorTileLength2Jigger(inAnchor);

			PromptResult result2 = Session.GetEditor().Drag(anchorJigger2);

			if (result2.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileLength2 = anchorJigger2.anchor.tileLength2;
			Session.Log("");
		}
	}

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

	// -----------------------------------------------------------------------------------------------
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
				jigSprinkler = BlockOps.InsertBlock(BlockOps.GetDynamicBlockName(templateSprinkler));

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

			draw.Geometry.Draw(jigSprinkler);

			Circle minimumRadius = new Circle(jigSprinkler.Position, Vector3d.ZAxis, 1828.8 * Session.AutoScaleFactor());
			minimumRadius.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.DarkGrey);

			Circle maximumRadius = new Circle(jigSprinkler.Position, Vector3d.ZAxis, 2286 * Session.AutoScaleFactor());
			maximumRadius.Color = Color.FromColorIndex(ColorMethod.ByAci, Colors.LightGreen);

			draw.Geometry.Draw(minimumRadius);
			draw.Geometry.Draw(maximumRadius);

			return true;
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

			if (shortAxisLength > longAxisLength)
			{
				Vector3d temp = shortAxis;
				shortAxis = longAxis;
				longAxis = temp;

				double temp2 = shortAxisLength;
				shortAxisLength = longAxisLength;
				longAxisLength = temp2;
			}

			if (longAxisLength > Math.Abs(shortAxisLength * 1.25))
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