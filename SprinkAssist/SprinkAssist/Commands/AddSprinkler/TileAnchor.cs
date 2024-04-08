using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands.AddSprinkler
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
			get { return _anchorPos; }
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
}
