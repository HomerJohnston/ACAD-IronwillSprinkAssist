using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Objects
{
	internal class Cross : IDisposable
	{
		Line line1 = new Line();
		Line line2 = new Line();
		
		double? screenSize = null;
		double? worldSize = null;

		private Color _color;
		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				line1.Color = value;
				line2.Color = value;
			}
		}

		public Point3d? Center { get; set; }

		public Cross()
		{
			Center = Point3d.Origin;
		}

		public Cross(Point3d inPosition)
		{
			Center = inPosition;
		}

		public void SetWorldSize(double inWorldSize)
		{
			screenSize = null;
			worldSize = inWorldSize;

			double r = 0.5 * 0.7071 * inWorldSize;

			line1.StartPoint = Center.Value + new Vector3d(-r, r, 0);
			line1.EndPoint = Center.Value + new Vector3d(r, -r, 0);

			line2.StartPoint = Center.Value + new Vector3d(r, r, 0);
			line2.EndPoint = Center.Value + new Vector3d(-r, -r, 0);
		}

		public void SetScreenSize(double inScreenSize)
		{
			worldSize = null;
			screenSize = inScreenSize;
		}

		public void Draw(WorldDraw drawer)
		{
			if (Center == null)
			{
				return;
			}

			if (worldSize != null)
			{
				line1.Draw(drawer);
				line2.Draw(drawer);
			}
			else if (screenSize != null)
			{
				using (ViewTableRecord viewTableRecord = Session.GetEditor().GetCurrentView())
				{
					double screenHeight = viewTableRecord.Height;

					double r = 0.5 * 0.7071 * screenHeight * screenSize.Value;

					line1.StartPoint = Center.Value + new Vector3d(-r, r, 0);
					line1.EndPoint = Center.Value + new Vector3d(r, -r, 0);

					line2.StartPoint = Center.Value + new Vector3d(r, r, 0);
					line2.EndPoint = Center.Value + new Vector3d(-r, -r, 0);

					line1.Draw(drawer);
					line2.Draw(drawer);
				}
			}
			else
			{
				Session.LogDebug("No size specified for cross!");
			}
		}

		public void Dispose()
		{
			line1.Dispose();
			line2.Dispose();
		}
	}
}
