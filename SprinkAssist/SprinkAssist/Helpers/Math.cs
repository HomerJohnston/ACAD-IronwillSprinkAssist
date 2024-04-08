using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Ironwill
{
	public static class SprinkMath
	{
		public static Point3d ConvertFromUCStoWCS(Point3d point)
		{
			Matrix3d currentUCS = Session.GetEditor().CurrentUserCoordinateSystem;
			CoordinateSystem3d coordinateSystem3D = currentUCS.CoordinateSystem3d;

			Matrix3d mat = Matrix3d.AlignCoordinateSystem(Point3d.Origin, Vector3d.XAxis, Vector3d.YAxis, Vector3d.ZAxis, coordinateSystem3D.Origin, coordinateSystem3D.Xaxis, coordinateSystem3D.Yaxis, coordinateSystem3D.Zaxis);

			return point.TransformBy(mat);
		}

		public static double RadToDeg(double rad)
		{
			return rad * 180.0 / Math.PI;
		}

		public static double DegToRad(double deg)
		{
			return deg / 180.0 * Math.PI;
		}

		/**
		 
		 */
		public static string FormatArea(double area, DrawingUnits displayUnits, DrawingUnits inputUnits = DrawingUnits.Undefined)
		{
			if (inputUnits == DrawingUnits.Undefined)
			{
				inputUnits = Session.GetPrimaryUnits();
			}

			if (inputUnits == DrawingUnits.Imperial && displayUnits == DrawingUnits.Metric)
			{
				area = AreaINtoMM(area);
			}
			else if (inputUnits == DrawingUnits.Metric && displayUnits == DrawingUnits.Imperial)
			{
				area = AreaMMtoIN(area);
			}

			switch (displayUnits)
			{
				case DrawingUnits.Imperial:
					double sqft = Math.Round(area * 10.0 / 144.0) / 10.0;
					return sqft.ToString() + " ft²";
				case DrawingUnits.Metric:
					double sqm = Math.Round(area * 10.0 / 1000000.0) / 10.0;
					return sqm.ToString() + " m²";
			}

			return "ERROR IN FORMATAREA";
		}

		public static double AreaMMtoIN(double squareMM)
		{
			return 0.001550003 * squareMM;
		}

		public static double AreaINtoMM(double squareIN)
		{
			return 645.16 * squareIN;
		}

		public static bool NearlyEqual(double a, double b, double threshold = 0.0001)
		{
			return (Math.Abs(a - b) < threshold);
		}
	}
}
